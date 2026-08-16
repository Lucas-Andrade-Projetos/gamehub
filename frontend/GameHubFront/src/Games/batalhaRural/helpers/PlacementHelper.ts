import { Board } from "../entities/Board";
import { Token } from "../entities/Token";
import { Direction } from "../enums/Direction";
import { BoundRuler } from "../services/rules/BoundRuler";
import { CollisionRuler } from "../services/rules/CollisionRuler";
import { PositionService } from "../services/PositionService";
import { TokenService } from "../services/TokenService";
import { InputValidator } from "../services/validators/InputValidator";
import { TokenInfo } from "./TokenInfo";

export function defaultDirectionFor(token: Token): Direction {
    return token.size > 1 ? Direction.Right : Direction.Centered;
}

export class PlacementHelper {

    static getFootprintCells(token: Token, boardSize: number): { x: number, y: number }[] {
        const tokenInfo = new TokenInfo(token);

        const startX = Math.min(tokenInfo.baseZeroPosX, tokenInfo.baseZeroFinalPosX);
        const endX = Math.max(tokenInfo.baseZeroPosX, tokenInfo.baseZeroFinalPosX);
        const startY = Math.min(tokenInfo.baseZeroPosY, tokenInfo.baseZeroFinalPosY);
        const endY = Math.max(tokenInfo.baseZeroPosY, tokenInfo.baseZeroFinalPosY);

        const cells: { x: number, y: number }[] = [];

        for (let y = startY; y <= endY; y++) {
            for (let x = startX; x <= endX; x++) {
                if (x >= 0 && x < boardSize && y >= 0 && y < boardSize) {
                    cells.push({ x, y });
                }
            }
        }

        return cells;
    }

    static previewFootprint(token: Token, board: Board): { cells: { x: number, y: number }[], valid: boolean } {
        const tokenInfo = new TokenInfo(token);

        let valid = InputValidator.isInputValid(tokenInfo, board)
            && !BoundRuler.isOutOfBounds(tokenInfo, board)
            && !CollisionRuler.isTileOccupied(tokenInfo, board);

        if (valid && tokenInfo.tokenSize > 1 && PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, tokenInfo.tokenDir)) {
            valid = false;
        }

        return { cells: PlacementHelper.getFootprintCells(token, board.tiles.length), valid };
    }

    static tryPlaceToken(token: Token, board: Board): boolean {
        const tokenInfo = new TokenInfo(token);

        if (!InputValidator.isInputValid(tokenInfo, board)) return false;
        if (BoundRuler.isOutOfBounds(tokenInfo, board)) return false;
        if (CollisionRuler.isTileOccupied(tokenInfo, board)) return false;

        if (tokenInfo.tokenSize > 1) {
            if (PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, tokenInfo.tokenDir)) return false;

            switch (tokenInfo.tokenDir) {
                case Direction.Down:
                    TokenService.placeTokenDown(tokenInfo, board);
                    break;
                case Direction.Up:
                    TokenService.placeTokenUp(tokenInfo, board);
                    break;
                case Direction.Right:
                    TokenService.placeTokenRight(tokenInfo, board);
                    break;
                case Direction.Left:
                    TokenService.placeTokenLeft(tokenInfo, board);
                    break;
            }
        } else {
            board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX] = tokenInfo.tokenChar;
        }

        return true;
    }
}
