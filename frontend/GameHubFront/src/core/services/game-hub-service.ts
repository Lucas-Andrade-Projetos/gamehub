import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { AccountService } from './account-service';
import { ApiPlayerNum } from './batalha-rural-service';

// sessionStorage, not localStorage: it's scoped per tab, so opening a second tab for the same
// account doesn't auto-rejoin and steal the room's connection slot from the tab already playing.
const ROOM_CODE_STORAGE_KEY = 'batalha-rural-room-code';

export interface RoomPlayerInfo {
  userId: string;
  nickname: string;
  ready: boolean;
  connected: boolean;
}

export interface RoomState {
  code: string;
  gameId: string | null;
  player1: RoomPlayerInfo | null;
  player2: RoomPlayerInfo | null;
}

export interface RoomSummary {
  code: string;
  hostNickname: string;
}

export interface ChatMessage {
  nickname: string;
  text: string;
  sentAt: string;
  isSystem: boolean;
}

export interface AttackResolvedEvent {
  success: boolean;
  reason: string | null;
  hit: boolean;
  x: number;
  y: number;
  attackerPlayerNum: ApiPlayerNum;
  defenderPlayerNum: ApiPlayerNum;
  gameEnded: boolean;
  nextTurnPlayerNum: ApiPlayerNum | null;
  winnerPlayerNum: ApiPlayerNum | null;
}

export interface GameEndedEvent {
  winnerPlayerNum: ApiPlayerNum;
  reason: 'Normal' | 'Abandonment';
}

@Injectable({
  providedIn: 'root',
})
export class GameHubService {
  private accountService = inject(AccountService);
  private hubConnection: HubConnection | null = null;

  hubUrl = 'https://localhost:5001/hubs/game';

  roomState = signal<RoomState | null>(null);
  chatMessages = signal<ChatMessage[]>([]);
  openRooms = signal<RoomSummary[]>([]);
  gameReadyGameId = signal<string | null>(null);
  battleStarting = signal(false);
  attackResolved = signal<AttackResolvedEvent | null>(null);
  turnTimedOut = signal(0);
  gameEnded = signal<GameEndedEvent | null>(null);
  rejoined = signal<RoomState | null>(null);

  connect(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state !== HubConnectionState.Disconnected) {
      return Promise.resolve();
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.accountService.currentUser()?.token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('RoomUpdated', (state: RoomState) => this.roomState.set(state));
    this.hubConnection.on('ChatMessageReceived', (message: ChatMessage) =>
      this.chatMessages.update(messages => [...messages, message])
    );
    this.hubConnection.on('RoomListUpdated', (rooms: RoomSummary[]) => this.openRooms.set(rooms));
    this.hubConnection.on('GameReady', (gameId: string) => this.gameReadyGameId.set(gameId));
    this.hubConnection.on('BattleStarting', () => this.battleStarting.set(true));
    this.hubConnection.on('AttackResolved', (event: AttackResolvedEvent) => this.attackResolved.set(event));
    this.hubConnection.on('TurnTimedOut', () => this.turnTimedOut.update(n => n + 1));
    this.hubConnection.on('GameEnded', (event: GameEndedEvent) => this.gameEnded.set(event));

    this.hubConnection.onreconnected(() => this.attemptAutoRejoin());

    return this.hubConnection.start().then(() => this.attemptAutoRejoin());
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
    this.roomState.set(null);
    this.chatMessages.set([]);
    this.openRooms.set([]);
    this.resetBattleFlow();
  }

  createRoom(): Promise<string> {
    return this.requireConnection().invoke<string>('CreateRoom');
  }

  joinRoom(code: string): Promise<void> {
    return this.requireConnection().invoke('JoinRoom', code);
  }

  sendMessage(code: string, text: string): Promise<void> {
    return this.requireConnection().invoke('SendMessage', code, text);
  }

  setReady(code: string, ready: boolean): Promise<void> {
    return this.requireConnection().invoke('SetReady', code, ready);
  }

  async listOpenRooms(): Promise<void> {
    const rooms = await this.requireConnection().invoke<RoomSummary[]>('ListOpenRooms');
    this.openRooms.set(rooms);
  }

  notifyPlacementReady(gameId: string, code: string): Promise<void> {
    return this.requireConnection().invoke('NotifyPlacementReady', gameId, code);
  }

  attack(gameId: string, code: string, x: number, y: number): Promise<void> {
    return this.requireConnection().invoke('Attack', gameId, code, x, y);
  }

  returnToRoom(code: string): Promise<void> {
    return this.requireConnection().invoke('ReturnToRoom', code);
  }

  leaveRoom(code: string): Promise<void> {
    this.forgetRoom();
    return this.requireConnection().invoke('LeaveRoom', code);
  }

  rememberRoom(code: string) {
    sessionStorage.setItem(ROOM_CODE_STORAGE_KEY, code);
  }

  forgetRoom() {
    sessionStorage.removeItem(ROOM_CODE_STORAGE_KEY);
  }

  resetBattleFlow() {
    this.gameReadyGameId.set(null);
    this.battleStarting.set(false);
    this.attackResolved.set(null);
    this.turnTimedOut.set(0);
    this.gameEnded.set(null);
    this.rejoined.set(null);
  }

  private async attemptAutoRejoin(): Promise<void> {
    const code = sessionStorage.getItem(ROOM_CODE_STORAGE_KEY);
    if (!code) return;

    try {
      const room = await this.requireConnection().invoke<RoomState | null>('Rejoin', code);
      if (room) {
        this.rejoined.set(room);
      } else {
        this.forgetRoom();
      }
    } catch {
      this.forgetRoom();
    }
  }

  private requireConnection(): HubConnection {
    if (!this.hubConnection) throw new Error('Hub connection not established');
    return this.hubConnection;
  }
}
