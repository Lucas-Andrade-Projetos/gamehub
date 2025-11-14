import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { Token } from "../entities/Token";
import { Utils } from "../helpers/Utils";
import { Board } from "../entities/Board";
import { ValidationService } from "./ValidationService";

export class TokenService {

    static calcFinalPosition(token: Token, axis: Axis) {
        if ((token.direction == Direction.Right) && (axis == Axis.X)) {
            token.finalPositionX = token.positionX + token.size - 1;
        }

        if ((token.direction == Direction.Left) && (axis == Axis.X)) {
            token.finalPositionX = token.positionX - token.size + 1
        }

        if (token.direction == Direction.Centered) {
            token.finalPositionX = token.positionX;
            token.finalPositionY = token.positionY;
        }

        if ((token.direction == Direction.Down) && (axis == Axis.Y)) {
            token.finalPositionY = token.positionY + token.size - 1;
        }

        if ((token.direction == Direction.Up) && (axis == Axis.Y)) {
            token.finalPositionY = token.positionY - token.size + 1;
        }

        return TokenService.verifyCheckedAxis(axis, token.direction, token);
    }
    
    static positionTokenInBoard(token: Token, board: Board) {
        const baseOnePosX = token.positionX;
        const baseOnePosY = token.positionY;
        const baseZeroPosX = Utils.toBaseZero(baseOnePosX);
        const baseZeroPosY = Utils.toBaseZero(baseOnePosY);
        const tokenSize = token.size;
        const tokenOri = token.direction;
        const tokenChar = token.character;
    
        if (tokenOri == Direction.Down) {
            for (let k = 0; k < tokenSize; k++) {
                board.tiles[baseZeroPosY + k][baseZeroPosX] = tokenChar;
            }
        }
    
        if (tokenOri == Direction.Up) {
            for (let k = 0; k < tokenSize; k++) {
                board.tiles[baseZeroPosY - k][baseZeroPosX] = tokenChar;
            }
        }
    
        if (tokenOri == Direction.Right) {
            if (ValidationService.isTokenPathFilled(token, board, Axis.X, Direction.Right)) {
                for (let k = 0; k < tokenSize; k++) {
                    board.tiles[baseZeroPosY][baseZeroPosX + k] = tokenChar;
                }
            }
        }
    
        if (tokenOri == Direction.Left) {
            for (let k = 0; k < tokenSize; k++) {
                board.tiles[baseZeroPosY][baseZeroPosX - k] = tokenChar;
            }
        }
    }
    
    private static verifyCheckedAxis(axis: Axis, direction: Direction, token: Token): number {
        if (axis == Axis.X && (direction == Direction.Down || direction == Direction.Up)) {
            token.finalPositionX = token.positionX;
        }
    
        if (axis == Axis.Y && (direction == Direction.Left || direction == Direction.Right)) {
            token.finalPositionY = token.positionY;
        }
    
        return 0;
    }
}
