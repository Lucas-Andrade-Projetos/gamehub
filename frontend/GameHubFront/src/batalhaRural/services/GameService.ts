import { Direction } from "../enums/Direction";
import { Board } from "../entities/Board";
import { Tokens } from "../entities/Tokens";
import { TokenService } from "./TokenService";
import { Utils } from "../helpers/Utils";
import { InputValidator } from "./validators/InputValidator";
import { TokenInfo } from "../helpers/TokenInfo";
import { PositionService } from "./PositionService";

export class GameService {

    positionTokens(board: Board, tokens: Tokens) {
        for (let i = 0; i < tokens.tokenCollection.length; i++) {

            // peça a ser posicionada
            let token = tokens.tokenCollection[i];

            // coletando informações da peça atual
            const tokenInfo = new TokenInfo(token);

            // verificando se a posição inicial do token é valida
            if (InputValidator.isInputValid(tokenInfo, board) && PositionService.canPlaceToken(tokenInfo, board)) {

                if (tokenInfo.tokenSize > 1) {

                    if (tokenInfo.tokenDir == Direction.Down) {
                        if (!PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, Direction.Down)) {
                            TokenService.placeTokenDown(tokenInfo, board);
                        }
                    }

                    if (tokenInfo.tokenDir == Direction.Up) {
                        if (!PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, Direction.Up)) {
                            TokenService.placeTokenUp(tokenInfo, board);
                        }
                    }

                    if (tokenInfo.tokenDir == Direction.Right) {
                        if (!PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, Direction.Right)) {
                            TokenService.placeTokenRight(tokenInfo, board);
                        }
                    }

                    if (tokenInfo.tokenDir == Direction.Left) {
                        if (!PositionService.isTokenPathOccupied(tokenInfo, board, tokenInfo.tokenAxis, Direction.Left)) {
                            TokenService.placeTokenLeft(tokenInfo, board);
                        }
                    }
                }
                else {
                    board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX] = tokenInfo.tokenChar;
                }
            }
        }
        Utils.printBoard(board);
    }
}