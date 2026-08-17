import { Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GameHubService } from '../../../../core/services/game-hub-service';

@Component({
  selector: 'app-batalha-rural-lobby',
  imports: [FormsModule],
  templateUrl: './lobby.html',
  styleUrl: './lobby.css',
})
export class Lobby {
  roomCode = input.required<string>();

  leftRoom = output<void>();

  private gameHubService = inject(GameHubService);

  roomState = this.gameHubService.roomState;
  chatMessages = this.gameHubService.chatMessages;

  ready = signal(false);
  messageInput = '';

  toggleReady() {
    const next = !this.ready();
    this.ready.set(next);
    this.gameHubService.setReady(this.roomCode(), next);
  }

  sendMessage() {
    const text = this.messageInput.trim();
    if (!text) return;

    this.gameHubService.sendMessage(this.roomCode(), text);
    this.messageInput = '';
  }

  async leaveRoom() {
    await this.gameHubService.leaveRoom(this.roomCode());
    this.leftRoom.emit();
  }
}
