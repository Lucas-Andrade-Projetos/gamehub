import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

export type ApiPlayerNum = 'Player1' | 'Player2';
export type ApiDirection = 'Left' | 'Right' | 'Centered' | 'Up' | 'Down';

export interface TokenStateDto {
  index: number;
  type: string;
  name: string;
  character: string;
  size: number;
  positionX: number;
  positionY: number;
  direction: ApiDirection;
}

export interface ShotDto {
  x: number;
  y: number;
  hit: boolean;
}

export interface PlayerStateDto {
  id: string;
  nickname: string;
  playerNum: ApiPlayerNum;
  playerStatus: string;
  board: string[][];
  tokens: TokenStateDto[];
  shotsReceived: ShotDto[];
}

export interface GameStateDto {
  id: string;
  status: string;
  viewerPlayerNum: ApiPlayerNum;
  currentTurnPlayerNum: ApiPlayerNum | null;
  turnExpiresAt: string | null;
  players: PlayerStateDto[];
}

export interface TokenPlacementRequest {
  index: number;
  positionX: number;
  positionY: number;
  direction: ApiDirection;
}

export type PlacementFailureReason = 'InvalidInput' | 'OutOfBounds' | 'Collision' | 'NotAllowed' | 'IncompleteSet';

export interface PlacementResultDto {
  success: boolean;
  reason: PlacementFailureReason | null;
  tokenIndex: number | null;
}

@Injectable({
  providedIn: 'root',
})
export class BatalhaRuralService {
  private http = inject(HttpClient);

  baseUrl = 'https://localhost:5001/api/batalha-rural';

  getGame(gameId: string) {
    return this.http.get<GameStateDto>(`${this.baseUrl}/${gameId}`);
  }

  placeTokens(gameId: string, tokens: TokenPlacementRequest[]) {
    return this.http.put<PlacementResultDto>(`${this.baseUrl}/${gameId}/placement`, { tokens });
  }
}
