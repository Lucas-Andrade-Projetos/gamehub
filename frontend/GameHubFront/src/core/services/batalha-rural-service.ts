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

export interface PlaceTokenRequest {
  positionX: number;
  positionY: number;
  direction: ApiDirection;
}

export interface PlaceTokenResultDto {
  success: boolean;
  reason: string | null;
  player: PlayerStateDto;
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

  placeToken(gameId: string, tokenIndex: number, body: PlaceTokenRequest) {
    return this.http.post<PlaceTokenResultDto>(`${this.baseUrl}/${gameId}/tokens/${tokenIndex}/place`, body);
  }
}
