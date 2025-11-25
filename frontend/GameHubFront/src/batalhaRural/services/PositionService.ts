import { Board } from "../entities/Board";
import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { TokenInfo } from "../helpers/TokenInfo";
import { BoundRuler } from "./rules/BoundRuler";
import { CollisionRuler } from "./rules/CollisionRuler";

export class PositionService {

    static canPlaceToken(tokenInfo: TokenInfo, board: Board): boolean {

        if (BoundRuler.isOutOfBounds(tokenInfo, board) || CollisionRuler.isTileOccupied(tokenInfo, board)) return false;

        return true;
    }

    static isTokenPathOccupied(tokenInfo: TokenInfo, board: Board, axis: Axis, direction: Direction): boolean {
        switch (axis) {
            case Axis.X:
                if (!CollisionRuler.isTokenHorizontallyOverlaping(tokenInfo, board, direction)) return false;
                break;
            case Axis.Y:
                if (!CollisionRuler.isTokenVerticallyOverlaping(tokenInfo, board, direction)) return false;
                break;               
        }
        
        return true;
    }
}