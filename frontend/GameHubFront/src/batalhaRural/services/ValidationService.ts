import { Board } from "../entities/Board";
import { Token } from "../entities/Token";
import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { Logger } from "../helpers/Logger";
import { Utils } from "../helpers/Utils";

export class ValidationService {
    static canPlaceTokenOnStartPos(token: Token, board: Board): boolean {
        const baseOnePosX = token.positionX;
        const baseOnePosY = token.positionY;
        const baseZeroPosX = Utils.toBaseZero(baseOnePosX);
        const baseZeroPosY = Utils.toBaseZero(baseOnePosY);
        const baseFinalPosX = Utils.toBaseZero(token.finalPositionX);
        const baseFinalPosY = Utils.toBaseZero(token.finalPositionY);
        const boardLength = board.tiles.length;

        // verificando se a peça passa do tamanho do tabuleiro
        if ((baseZeroPosX + token.size - 1 > boardLength) || (baseZeroPosY + token.size - 1 > boardLength)) {
            Logger.log(`Pec > Tab - Coordenadas (X${baseOnePosX},Y${baseOnePosY}) inválidas para a peça ${Utils.getTokenName(token.character)}`);

            return false;
        }

        // verificando se as posições > 1
        if ((baseFinalPosX < 0) || (baseFinalPosY < 0) || (baseZeroPosY < 0) || (baseZeroPosX < 0)) {
            Logger.log(`Pos > 1 - Coordenadas (X${baseOnePosX},Y${baseOnePosY}) inválidas para a posição da peça ${Utils.getTokenName(token.character)}`)

            return false;
        }

        // verificando se a posição final é fora do boarduleiro (X+ Y+)
        if ((baseFinalPosX > boardLength) || (baseFinalPosY > boardLength)) {
            Logger.log(`PosF > Tab - Coordenadas (X${baseOnePosX},Y${baseOnePosY}) inválidas para a posição final da peça ${Utils.getTokenName(token.character)}`)

            return false;
        }

        // verificando se tem peça na posição inicial desejada
        if (board.tiles[baseZeroPosY][baseZeroPosX] != 'O') {
            let boardToken = board.tiles[baseZeroPosY][baseZeroPosX];

            Logger.log(`X - A peça ${Utils.getTokenName(boardToken)} já está na coordenada (X${baseOnePosX},Y${baseOnePosY})`);
            return false;
        }

        return true;
    }

    static isTokenPathFilled(token: Token, board: Board, axis: Axis, direction: Direction): boolean {
        // verificando se há parte de uma peça em algum ponto da posição desejada
        const baseOnePosX = token.positionX;
        const baseOnePosY = token.positionY;
        const baseZeroPosX = Utils.toBaseZero(baseOnePosX);
        const baseZeroPosY = Utils.toBaseZero(baseOnePosY);

        if (axis == Axis.X && direction == Direction.Right) {

            if (ValidationService.IsTokenDiagonal(token, axis)) return false;

            for (let i = baseZeroPosX; i < baseZeroPosX + token.size; i++) {
                let char = board.tiles[baseZeroPosY][i];

                if (char != 'O') {
                    Logger.log(`X - Uma parte da peça ${Utils.getTokenName(char)} já está na coordenada (X${baseOnePosX},Y${baseOnePosY})`);
                    return false;
                }
            }
        }

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (axis == Axis.X && direction == Direction.Left) {

            if (ValidationService.IsTokenDiagonal(token, axis)) return false;

            for (let i = 0; i < token.size; i++) {
                let char = board.tiles[baseZeroPosY][baseZeroPosX - i];

                if (char != 'O') {
                    Logger.log(`X - Uma parte da peça ${Utils.getTokenName(char)} já está na coordenada (X${baseOnePosX},Y${baseOnePosY})`);
                    return false;
                }
            }
        }

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (axis == Axis.Y && direction == Direction.Up) {

            if (ValidationService.IsTokenDiagonal(token, axis)) return false;

            for (let i = 0; i < token.size; i++) {
                let char = board.tiles[baseZeroPosY - i][baseZeroPosX];

                if (char != 'O') {
                    Logger.log(`X - Uma parte da peça ${Utils.getTokenName(char)} já está na coordenada (X${baseOnePosX},Y${baseOnePosY})`);
                    return false;
                }
            }
        }

        // verificando se há parte de uma peça em algum ponto da posição desejada
        if (axis == Axis.Y && direction == Direction.Down) {

            if (ValidationService.IsTokenDiagonal(token, axis)) return false;

            for (let i = baseZeroPosY; i < baseZeroPosY + token.size; i++) {
                let char = board.tiles[i][baseZeroPosX];

                if (char != 'O') {
                    Logger.log(`X - Uma parte da peça ${Utils.getTokenName(char)} já está na coordenada (X${baseOnePosX},Y${baseOnePosY})`);
                    return false;
                }
            }
        }

        return true;
    }

    private static IsTokenDiagonal(token: Token, axis: Axis): boolean | void {

    const baseOnePosX = token.positionX;
    const baseOnePosY = token.positionY;
    const baseOneFinalPosX = token.finalPositionX;
    const baseOneFinalPosY = token.finalPositionY;
    const baseZeroPosX = Utils.toBaseZero(baseOnePosX);
    const baseZeroPosY = Utils.toBaseZero(baseOnePosY);
    const baseZeroFinalPosX = Utils.toBaseZero(baseOneFinalPosX);
    const baseZeroFinalPosY = Utils.toBaseZero(baseOneFinalPosY);

    if (axis == Axis.X) {
        if (baseZeroFinalPosY != baseZeroPosY) {
            Logger.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${baseOneFinalPosX},Y${baseOneFinalPosY}) inválida`)
            return true;
        }
    }

    if (axis == Axis.Y) {
        if (baseZeroFinalPosX != baseZeroPosX) {
            Logger.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${baseOneFinalPosX},Y${baseOneFinalPosY}) inválida`)
            return true;
        }
    }
}
}