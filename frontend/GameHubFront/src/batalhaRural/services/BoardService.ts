import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { Board } from "../entities/Board";
import { Token } from "../entities/Token";
import { Tokens } from "../entities/Tokens";
import { TokenService } from "./TokenService";
import { Utils } from "../helpers/Utils";
import { ValidationService } from "./ValidationService";


export class BoardService {

    positionTokens(board: Board, tokens: Tokens) {
        for (let i = 0; i < tokens.tokenCollection.length; i++) {

            // peça a ser posicionada
            let token = tokens.tokenCollection[i];

            // coletando informações da peça atual
            const tokenPosX = Utils.toBaseZero(token.positionX);
            const tokenPosY = Utils.toBaseZero(token.positionY);
            const tokenSize = token.size;
            const tokenOri = token.direction;
            const tokenChar = token.character;

            // calculando a posição final das peças
            TokenService.calcFinalPosition(token, Axis.X);
            TokenService.calcFinalPosition(token, Axis.Y);

            // Logger.log("inicial  - " + tokenChar + " - " + tokenPosX + "," + tokenPosY)
            // Logger.log("final XY - " + tokenChar + " - " + token.finalPositionX + "," + token.finalPositionY)

            // verificando se a posição inicial do token é valida
            if (ValidationService.canPlaceTokenOnStartPos(token, board)) {

                if (tokenSize > 1) {
                    // verificando se a posFinal é > que a de origem, apontando que Y+ (peça descendo)
                    if (tokenOri == Direction.Down) {
                        if (ValidationService.isTokenPathFilled(token, board, Axis.Y, Direction.Down)) {
                            TokenService.positionTokenInBoard(token, board);
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que Y- (peça subindo)
                    if (tokenOri == Direction.Up) {
                        if (ValidationService.isTokenPathFilled(token, board, Axis.Y, Direction.Up)) {
                            TokenService.positionTokenInBoard(token, board);
                        }
                    }

                    // verificando se a posFinal é > que a de origem, apontando que X+ (peça p/ direita)
                    if (tokenOri == Direction.Right) {
                        if (ValidationService.isTokenPathFilled(token, board, Axis.X, Direction.Right)) {
                            TokenService.positionTokenInBoard(token, board);
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que X- (peça p/ esquerda)
                    if (tokenOri == Direction.Left) {
                        if (ValidationService.isTokenPathFilled(token, board, Axis.X, Direction.Left)) {
                            TokenService.positionTokenInBoard(token, board);
                        }
                    }
                }
                else {
                    board.tiles[tokenPosY][tokenPosX] = tokenChar;
                }
            }
        }
        Utils.printBoard(board);
    }
}

// proximos passos:
// implementar enum para direção e eixo - ok
//
// padronizar coordenadas padrão-0 (calcrealPosition) - ok
// interface do usuário - padrão-1 - ok
// no programa - padrão-0 - ok
//
// ao invés de ! utilizar checagem de undefined - ok
//
// tornar posFinal dinamica, calculada automaticamente baseada na direção desejada - ok