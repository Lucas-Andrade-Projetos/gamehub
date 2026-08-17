import { Board } from "../../entities/Board";
import { Axis } from "../../enums/Axis";
import { Direction } from "../../enums/Direction";
import { Logger } from "../../helpers/Logger";
import { TokenInfo } from "../../helpers/TokenInfo";

export class CollisionRuler {

    static isTileOccupied(tokenInfo: TokenInfo, board: Board): boolean {
        const tileStatus = board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX];

        if (tileStatus != 'O') { Logger.collision(); return true; }

        return false;
    }

    static isTokenHorizontallyOverlaping(tokenInfo: TokenInfo, board: Board, direction: Direction): boolean {
        if (direction == Direction.Right) {
            if (this.isTokenDiagonal(tokenInfo, Axis.X)) return true;

            for (let i = tokenInfo.baseZeroPosX; i < tokenInfo.baseZeroPosX + tokenInfo.tokenSize; i++) {
                let char = board.tiles[tokenInfo.baseZeroPosY][i];

                if (char != 'O') { Logger.collision(); return true; }
            }
        }

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (direction == Direction.Left) {
            if (this.isTokenDiagonal(tokenInfo, Axis.X)) return false;

            for (let i = 0; i < tokenInfo.tokenSize; i++) {
                let char = board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX - i];

                if (char != 'O') { Logger.collision(); return true; }
            }
        }

        return false;
    }

    static isTokenVerticallyOverlaping(tokenInfo: TokenInfo, board: Board, direction: Direction): boolean {

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (direction == Direction.Up) {
            if (this.isTokenDiagonal(tokenInfo, Axis.Y)) return false;

            for (let i = 0; i < tokenInfo.tokenSize; i++) {
                let char = board.tiles[tokenInfo.baseZeroPosY - i][tokenInfo.baseZeroPosX];

                if (char != 'O') { Logger.collision(); return true; }
            }
        }

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (direction == Direction.Down) {
            if (this.isTokenDiagonal(tokenInfo, Axis.Y)) return false;

            for (let i = tokenInfo.baseZeroPosY; i < tokenInfo.baseZeroPosY + tokenInfo.tokenSize; i++) {
                let char = board.tiles[i][tokenInfo.baseZeroPosX];

                if (char != 'O') { Logger.collision(); return true; }
            }
        }

        return false;
    }

    private static isTokenDiagonal(tokenInfo: TokenInfo, axis: Axis): boolean {
        switch (axis) {
            case Axis.X:
                if (tokenInfo.baseZeroFinalPosY != tokenInfo.baseZeroPosY) { Logger.diagonal(); return true; }
                break;
            case Axis.Y:
                if (tokenInfo.baseZeroFinalPosX != tokenInfo.baseZeroPosX) { Logger.diagonal(); return true; }
                break;
        }

        return false;
    }
}