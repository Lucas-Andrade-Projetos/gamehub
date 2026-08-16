import { Component, inject, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameHubService } from '../../../../core/services/game-hub-service';

@Component({
  selector: 'app-batalha-rural-room-select',
  imports: [FormsModule],
  templateUrl: './room-select.html',
  styleUrl: './room-select.css',
})
export class RoomSelect implements OnInit {
  private gameHubService = inject(GameHubService);

  roomJoined = output<string>();

  openRooms = this.gameHubService.openRooms;

  codeInput = '';
  errorMessage = signal<string | null>(null);
  creating = signal(false);
  joining = signal(false);

  ngOnInit() {
    this.gameHubService.listOpenRooms().catch(() => this.errorMessage.set('Não foi possível carregar as salas abertas.'));
  }

  async createRoom() {
    this.creating.set(true);
    this.errorMessage.set(null);

    try {
      const code = await this.gameHubService.createRoom();
      this.roomJoined.emit(code);
    } catch {
      this.errorMessage.set('Não foi possível criar a sala.');
    } finally {
      this.creating.set(false);
    }
  }

  async joinRoom(code: string) {
    const normalized = code.trim().toUpperCase();
    if (!normalized) return;

    this.joining.set(true);
    this.errorMessage.set(null);

    try {
      await this.gameHubService.joinRoom(normalized);
      this.roomJoined.emit(normalized);
    } catch {
      this.errorMessage.set('Sala não encontrada ou cheia.');
    } finally {
      this.joining.set(false);
    }
  }
}
