import { Component, computed, ElementRef, HostListener, inject, input, signal, ViewChild } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AccountService } from '../../../../core/services/account-service';
import { ApiDirection, BatalhaRuralService } from '../../../../core/services/batalha-rural-service';
import { GameHubService } from '../../../../core/services/game-hub-service';
import { Board } from '../../../../Games/batalhaRural/entities/Board';
import { Token } from '../../../../Games/batalhaRural/entities/Token';
import { Tokens } from '../../../../Games/batalhaRural/entities/Tokens';
import { Direction } from '../../../../Games/batalhaRural/enums/Direction';
import { defaultDirectionFor, PlacementHelper } from '../../../../Games/batalhaRural/helpers/PlacementHelper';

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

const DIRECTION_ARROWS: Record<Direction, string> = {
  [Direction.Left]: '←',
  [Direction.Right]: '→',
  [Direction.Centered]: '',
  [Direction.Up]: '↑',
  [Direction.Down]: '↓',
};

const ROTATION_ORDER = [Direction.Right, Direction.Down, Direction.Left, Direction.Up];

function nextDirection(current: Direction): Direction {
  const index = ROTATION_ORDER.indexOf(current);
  return ROTATION_ORDER[(index + 1) % ROTATION_ORDER.length];
}

@Component({
  selector: 'app-batalha-rural-placement',
  imports: [],
  templateUrl: './placement.html',
  styleUrl: './placement.css',
})
export class Placement {
  gameId = input.required<string>();
  roomCode = input.required<string>();

  accountService = inject(AccountService);
  private batalhaRuralService = inject(BatalhaRuralService);
  private gameHubService = inject(GameHubService);

  @ViewChild('boardGrid') boardGridRef?: ElementRef<HTMLDivElement>;

  boardRange = Array.from({ length: BOARD_SIZE }, (_, i) => i);

  board = signal(new Board(BOARD_SIZE));
  tokenViewModels = signal<TokenViewModel[]>(this.createInitialTokens());

  draggingIndex = signal<number | null>(null);
  hoverCell = signal<Cell | null>(null);
  dragDirection = signal<Direction | null>(null);

  submitting = signal(false);
  waitingForOpponent = signal(false);
  errorMessage = signal<string | null>(null);

  allPlaced = computed(() => this.tokenViewModels().every(vm => vm.placed));

  preview = computed(() => {
    const index = this.draggingIndex();
    const hover = this.hoverCell();
    const direction = this.dragDirection();

    if (index === null || hover === null || direction === null) return null;

    const candidate = this.buildCandidate(this.tokenViewModels()[index].token, hover, direction);

    return PlacementHelper.previewFootprint(candidate, this.board());
  });

  dragDirectionArrow = computed(() => {
    const direction = this.dragDirection();
    return direction === null ? '' : DIRECTION_ARROWS[direction];
  });

  private createInitialTokens(): TokenViewModel[] {
    return new Tokens().tokenCollection.map((token, index) => ({ token, index, placed: false }));
  }

  private buildCandidate(token: Token, hover: Cell, direction: Direction): Token {
    return {
      ...token,
      positionX: hover.x + 1,
      positionY: hover.y + 1,
      direction,
    } as Token;
  }

  onPieceMouseDown(event: MouseEvent, index: number) {
    event.preventDefault();

    const viewModel = this.tokenViewModels()[index];
    const startDirection = viewModel.placed ? viewModel.token.direction : defaultDirectionFor(viewModel.token);

    if (viewModel.placed) {
      this.clearToken(index);
    }

    this.draggingIndex.set(index);
    this.dragDirection.set(startDirection);
  }

  @HostListener('document:keydown', ['$event'])
  onDocumentKeyDown(event: KeyboardEvent) {
    if (this.draggingIndex() === null || event.key.toLowerCase() !== 'r') return;

    event.preventDefault();
    this.dragDirection.update(direction => nextDirection(direction ?? Direction.Right));
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
    const direction = this.dragDirection();

    if (index !== null && hover !== null && direction !== null) {
      this.commitPlacement(index, hover, direction);
    }

    this.draggingIndex.set(null);
    this.hoverCell.set(null);
    this.dragDirection.set(null);
  }

  private commitPlacement(index: number, hover: Cell, direction: Direction) {
    const viewModels = this.tokenViewModels();
    const viewModel = viewModels[index];
    const candidate = this.buildCandidate(viewModel.token, hover, direction);

    const boardCopy = new Board(BOARD_SIZE);
    boardCopy.tiles = this.board().tiles.map(row => [...row]);

    if (!PlacementHelper.tryPlaceToken(candidate, boardCopy)) return;

    viewModel.token.positionX = candidate.positionX;
    viewModel.token.positionY = candidate.positionY;
    viewModel.token.direction = candidate.direction;

    this.board.set(boardCopy);
    this.tokenViewModels.set(viewModels.map((vm, i) => (i === index ? { ...vm, placed: true } : vm)));
  }

  rotatePlacedToken(index: number) {
    const viewModels = this.tokenViewModels();
    const viewModel = viewModels[index];

    if (!viewModel.placed) return;

    const candidate = { ...viewModel.token, direction: nextDirection(viewModel.token.direction) } as Token;

    const boardCopy = new Board(BOARD_SIZE);
    boardCopy.tiles = this.board().tiles.map(row => [...row]);

    PlacementHelper.getFootprintCells(viewModel.token, BOARD_SIZE)
      .forEach(cell => (boardCopy.tiles[cell.y][cell.x] = 'O'));

    if (!PlacementHelper.tryPlaceToken(candidate, boardCopy)) return;

    viewModel.token.direction = candidate.direction;
    this.board.set(boardCopy);
    this.tokenViewModels.set(viewModels.map((vm, i) => (i === index ? { ...vm } : vm)));
  }

  directionArrow(direction: Direction): string {
    return DIRECTION_ARROWS[direction];
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

    this.submitting.set(true);
    this.errorMessage.set(null);

    try {
      for (const viewModel of this.tokenViewModels()) {
        const result = await firstValueFrom(
          this.batalhaRuralService.placeToken(this.gameId(), viewModel.index, {
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

      await this.gameHubService.notifyPlacementReady(this.gameId(), this.roomCode());
      this.waitingForOpponent.set(true);
    } catch {
      this.errorMessage.set('Erro ao confirmar o posicionamento. Tente novamente.');
    } finally {
      this.submitting.set(false);
    }
  }
}
