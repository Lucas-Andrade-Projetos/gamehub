import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { BatalhaRuralService, GameStateDto } from '../../../../core/services/batalha-rural-service';

@Component({
  selector: 'app-batalha-rural-battle',
  imports: [],
  templateUrl: './battle.html',
  styleUrl: './battle.css',
})
export class Battle implements OnInit {
  gameId = input.required<string>();

  private batalhaRuralService = inject(BatalhaRuralService);

  game = signal<GameStateDto | null>(null);
  showingReveal = signal(true);
  errorMessage = signal<string | null>(null);

  ownPlayer = computed(() => this.game()?.players.find(p => p.playerNum === this.game()?.viewerPlayerNum) ?? null);
  opponentPlayer = computed(() => this.game()?.players.find(p => p.playerNum !== this.game()?.viewerPlayerNum) ?? null);

  ngOnInit() {
    this.batalhaRuralService.getGame(this.gameId()).subscribe({
      next: game => this.game.set(game),
      error: () => this.errorMessage.set('Não foi possível carregar a partida.'),
    });

    setTimeout(() => this.showingReveal.set(false), 3000);
  }
}
