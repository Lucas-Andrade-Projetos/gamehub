import { Component, computed, ElementRef, HostListener, inject, signal, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AccountService } from '../../../core/services/account-service';
import { ApiDirection, BatalhaRuralService } from '../../../core/services/batalha-rural-service';
import { Board } from '../../../Games/batalhaRural/entities/Board';
import { Token } from '../../../Games/batalhaRural/entities/Token';
import { Tokens } from '../../../Games/batalhaRural/entities/Tokens';
import { Direction } from '../../../Games/batalhaRural/enums/Direction';
import { defaultDirectionFor, PlacementHelper } from '../../../Games/batalhaRural/helpers/PlacementHelper';

interface TokenViewModel {
  token: Token;
  index: number;
  placed: boolean;
}

interface Cell {
  x: number;
  y: number;
}

const BOARD_SIZE = 10;

const DIRECTION_NAMES: Record<Direction, ApiDirection> = {
  [Direction.Left]: 'Left',
  [Direction.Right]: 'Right',
  [Direction.Centered]: 'Centered',
  [Direction.Up]: 'Up',
  [Direction.Down]: 'Down',
};

@Component({
  selector: 'app-batalha-rural',
  imports: [],
  templateUrl: './batalha-rural.html',
  styleUrl: './batalha-rural.css',
})
export class BatalhaRural {
  router = inject(Router);
  accountService = inject(AccountService);
  private batalhaRuralService = inject(BatalhaRuralService);

  @ViewChild('boardGrid') boardGridRef?: ElementRef<HTMLDivElement>;

  boardRange = Array.from({ length: BOARD_SIZE }, (_, i) => i);

  gameStarted = signal(false);
  board = signal(new Board(BOARD_SIZE));
  tokenViewModels = signal<TokenViewModel[]>(this.createInitialTokens());

  draggingIndex = signal<number | null>(null);
  hoverCell = signal<Cell | null>(null);

  submitting = signal(false);
  finalized = signal(false);
  errorMessage = signal<string | null>(null);

  allPlaced = computed(() => this.tokenViewModels().every(vm => vm.placed));

  preview = computed(() => {
    const index = this.draggingIndex();
    const hover = this.hoverCell();

    if (index === null || hover === null) return null;

    const candidate = this.buildCandidate(this.tokenViewModels()[index].token, hover);

    return PlacementHelper.previewFootprint(candidate, this.board());
  });

  private createInitialTokens(): TokenViewModel[] {
    return new Tokens().tokenCollection.map((token, index) => ({ token, index, placed: false }));
  }

  private buildCandidate(token: Token, hover: Cell): Token {
    return {
      ...token,
      positionX: hover.x + 1,
      positionY: hover.y + 1,
      direction: defaultDirectionFor(token),
    } as Token;
  }

  startGame() {
    this.gameStarted.set(true);
  }

  onPieceMouseDown(event: MouseEvent, index: number) {
    event.preventDefault();

    if (this.tokenViewModels()[index].placed) {
      this.clearToken(index);
    }

    this.draggingIndex.set(index);
  }

  @HostListener('document:mousemove', ['$event'])
  onDocumentMouseMove(event: MouseEvent) {
    if (this.draggingIndex() === null || !this.boardGridRef) return;

    const rect = this.boardGridRef.nativeElement.getBoundingClientRect();
    const x = Math.floor((event.clientX - rect.left) / (rect.width / BOARD_SIZE));
    const y = Math.floor((event.clientY - rect.top) / (rect.height / BOARD_SIZE));

    if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE) {
      this.hoverCell.set(null);
      return;
    }

    this.hoverCell.set({ x, y });
  }

  @HostListener('document:mouseup')
  onDocumentMouseUp() {
    const index = this.draggingIndex();
    const hover = this.hoverCell();

    if (index !== null && hover !== null) {
      this.commitPlacement(index, hover);
    }

    this.draggingIndex.set(null);
    this.hoverCell.set(null);
  }

  private commitPlacement(index: number, hover: Cell) {
    const viewModels = this.tokenViewModels();
    const viewModel = viewModels[index];
    const candidate = this.buildCandidate(viewModel.token, hover);

    const boardCopy = new Board(BOARD_SIZE);
    boardCopy.tiles = this.board().tiles.map(row => [...row]);

    if (!PlacementHelper.tryPlaceToken(candidate, boardCopy)) return;

    viewModel.token.positionX = candidate.positionX;
    viewModel.token.positionY = candidate.positionY;
    viewModel.token.direction = candidate.direction;

    this.board.set(boardCopy);
    this.tokenViewModels.set(viewModels.map((vm, i) => (i === index ? { ...vm, placed: true } : vm)));
  }

  private clearToken(index: number) {
    const viewModels = this.tokenViewModels();
    const viewModel = viewModels[index];

    const boardCopy = new Board(BOARD_SIZE);
    boardCopy.tiles = this.board().tiles.map(row => [...row]);

    PlacementHelper.getFootprintCells(viewModel.token, BOARD_SIZE)
      .forEach(cell => (boardCopy.tiles[cell.y][cell.x] = 'O'));

    this.board.set(boardCopy);
    this.tokenViewModels.set(viewModels.map((vm, i) => (i === index ? { ...vm, placed: false } : vm)));
  }

  cellClass(x: number, y: number): string {
    const preview = this.preview();

    if (preview && preview.cells.some(c => c.x === x && c.y === y)) {
      return preview.valid ? 'bg-green-500/70' : 'bg-red-500/70';
    }

    return this.board().tiles[y][x] === 'O' ? 'bg-amber-100/40' : 'bg-stone-700 text-white';
  }

  cellChar(x: number, y: number): string {
    const char = this.board().tiles[y][x];
    return char === 'O' ? '' : char;
  }

  async finalizePlacement() {
    if (!this.allPlaced() || this.submitting()) return;

    const nickname = this.accountService.currentUser()?.nickname ?? 'Jogador';

    this.submitting.set(true);
    this.errorMessage.set(null);

    try {
      const game = await firstValueFrom(this.batalhaRuralService.createGame(nickname, 'Aguardando oponente'));

      for (const viewModel of this.tokenViewModels()) {
        const result = await firstValueFrom(
          this.batalhaRuralService.placeToken(game.id, 'Player1', viewModel.index, {
            positionX: viewModel.token.positionX,
            positionY: viewModel.token.positionY,
            direction: DIRECTION_NAMES[viewModel.token.direction],
          })
        );

        if (!result.success) {
          this.errorMessage.set(`Não foi possível confirmar a peça ${viewModel.token.name} (${result.reason}).`);
          return;
        }
      }

      this.finalized.set(true);
    } catch {
      this.errorMessage.set('Erro ao confirmar o posicionamento. Tente novamente.');
    } finally {
      this.submitting.set(false);
    }
  }

  returnHome() {
    this.router.navigate(['/home']);
  }
}
