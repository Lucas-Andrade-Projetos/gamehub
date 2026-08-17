import { Component, computed, effect, inject, input, OnInit, output, signal } from '@angular/core';
import { BatalhaRuralService, GameStateDto, PlayerStateDto, ShotDto } from '../../../../core/services/batalha-rural-service';
import { GameHubService } from '../../../../core/services/game-hub-service';

const BOARD_SIZE = 10;

@Component({
  selector: 'app-batalha-rural-battle',
  imports: [],
  templateUrl: './battle.html',
  styleUrl: './battle.css',
})
export class Battle implements OnInit {
  gameId = input.required<string>();
  roomCode = input.required<string>();

  returnedToRoom = output<void>();

  private batalhaRuralService = inject(BatalhaRuralService);
  private gameHubService = inject(GameHubService);

  boardRange = Array.from({ length: BOARD_SIZE }, (_, i) => i);

  game = signal<GameStateDto | null>(null);
  showingReveal = signal(true);
  revealFadingOut = signal(false);
  errorMessage = signal<string | null>(null);

  ownPlayer = computed<PlayerStateDto | null>(() => this.game()?.players.find(p => p.playerNum === this.game()?.viewerPlayerNum) ?? null);
  opponentPlayer = computed<PlayerStateDto | null>(() => this.game()?.players.find(p => p.playerNum !== this.game()?.viewerPlayerNum) ?? null);

  gameEnded = computed(() => this.game()?.status === 'Ended');
  isMyTurn = computed(() => {
    const game = this.game();
    return !!game && game.currentTurnPlayerNum === game.viewerPlayerNum;
  });

  private attacking = signal(false);

  constructor() {
    effect(() => {
      if (this.gameHubService.attackResolved()) this.refreshGame();
    });

    effect(() => {
      if (this.gameHubService.gameEndedWinner()) this.refreshGame();
    });
  }

  ngOnInit() {
    setTimeout(() => this.revealFadingOut.set(true), 2000);
    setTimeout(() => this.showingReveal.set(false), 2700);

    this.refreshGame();
  }

  private refreshGame() {
    this.batalhaRuralService.getGame(this.gameId()).subscribe({
      next: game => this.game.set(game),
      error: () => this.errorMessage.set('Não foi possível carregar a partida.'),
    });
  }

  private shotAt(player: PlayerStateDto | null, x: number, y: number): ShotDto | undefined {
    return player?.shotsReceived.find(s => s.x === x && s.y === y);
  }

  ownCellClass(x: number, y: number): string {
    const shot = this.shotAt(this.ownPlayer(), x, y);
    if (shot) return shot.hit ? 'bg-red-500/80 text-white' : 'bg-stone-400/60';

    const cell = this.ownPlayer()?.board[y]?.[x];
    return cell && cell !== 'O' ? 'bg-stone-700 text-white' : 'bg-amber-100/40';
  }

  ownCellChar(x: number, y: number): string {
    const shot = this.shotAt(this.ownPlayer(), x, y);
    if (shot) return shot.hit ? 'X' : '•';

    const cell = this.ownPlayer()?.board[y]?.[x];
    return cell && cell !== 'O' ? cell : '';
  }

  canAttack(x: number, y: number): boolean {
    return this.isMyTurn() && !this.gameEnded() && !this.attacking() && !this.shotAt(this.opponentPlayer(), x, y);
  }

  opponentCellClass(x: number, y: number): string {
    const shot = this.shotAt(this.opponentPlayer(), x, y);
    if (shot) return shot.hit ? 'bg-red-500/80 text-white' : 'bg-stone-400/60';

    return this.canAttack(x, y) ? 'bg-amber-100/40 hover:bg-amber-200/70 cursor-pointer' : 'bg-amber-100/40';
  }

  opponentCellChar(x: number, y: number): string {
    const shot = this.shotAt(this.opponentPlayer(), x, y);
    return shot ? (shot.hit ? 'X' : '•') : '';
  }

  async attackCell(x: number, y: number) {
    if (!this.canAttack(x, y)) return;

    this.attacking.set(true);

    try {
      await this.gameHubService.attack(this.gameId(), this.roomCode(), x, y);
    } finally {
      this.attacking.set(false);
    }
  }

  async returnToRoom() {
    await this.gameHubService.returnToRoom(this.roomCode());
    this.gameHubService.resetBattleFlow();
    this.returnedToRoom.emit();
  }
}
