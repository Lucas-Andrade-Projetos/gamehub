import { Component, effect, inject, OnDestroy, signal } from '@angular/core';
import { Router } from '@angular/router';
import { GameHubService } from '../../../core/services/game-hub-service';
import { Battle } from './battle/battle';
import { Lobby } from './lobby/lobby';
import { Placement } from './placement/placement';
import { RoomSelect } from './room-select/room-select';

type Phase = 'room-select' | 'lobby' | 'placement' | 'battle';

@Component({
  selector: 'app-batalha-rural',
  imports: [RoomSelect, Lobby, Placement, Battle],
  templateUrl: './batalha-rural.html',
  styleUrl: './batalha-rural.css',
})
export class BatalhaRural implements OnDestroy {
  router = inject(Router);
  gameHubService = inject(GameHubService);

  phase = signal<Phase>('room-select');
  roomCode = signal<string | null>(null);
  gameId = signal<string | null>(null);
  connectError = signal<string | null>(null);

  constructor() {
    this.gameHubService
      .connect()
      .catch(() => this.connectError.set('Não foi possível conectar ao servidor. Tente recarregar a página.'));

    effect(() => {
      const gameId = this.gameHubService.gameReadyGameId();
      if (gameId) {
        this.gameId.set(gameId);
        this.phase.set('placement');
      }
    });

    effect(() => {
      if (this.gameHubService.battleStarting()) {
        this.phase.set('battle');
      }
    });
  }

  onRoomJoined(code: string) {
    this.roomCode.set(code);
    this.phase.set('lobby');
  }

  onLeftRoom() {
    this.gameHubService.resetBattleFlow();
    this.roomCode.set(null);
    this.gameId.set(null);
    this.phase.set('room-select');
  }

  onReturnedToRoom() {
    this.gameId.set(null);
    this.phase.set('lobby');
  }

  ngOnDestroy() {
    this.gameHubService.disconnect();
  }

  returnHome() {
    this.router.navigate(['/home']);
  }
}
