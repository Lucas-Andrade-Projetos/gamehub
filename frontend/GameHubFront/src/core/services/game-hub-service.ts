import { inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { AccountService } from './account-service';

export interface RoomPlayerInfo {
  nickname: string;
  ready: boolean;
}

export interface RoomState {
  code: string;
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

    return this.hubConnection.start();
  }

  async disconnect(): Promise<void> {
    await this.hubConnection?.stop();
    this.hubConnection = null;
    this.roomState.set(null);
    this.chatMessages.set([]);
    this.openRooms.set([]);
    this.gameReadyGameId.set(null);
    this.battleStarting.set(false);
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

  private requireConnection(): HubConnection {
    if (!this.hubConnection) throw new Error('Hub connection not established');
    return this.hubConnection;
  }
}
