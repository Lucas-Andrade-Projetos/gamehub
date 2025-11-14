import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { Board } from "../entities/Board";
import { Token } from "../entities/Token";
import { Tokens } from "../entities/Tokens";
import { TokenService } from "./TokenService";


export class BoardService {

    tokenService = new TokenService();

    positionTokens(board: Board, tokens: Tokens) {
        for (let i = 0; i < tokens.tokenCollection.length; i++) {

            // peça a ser posicionada
            let token = tokens.tokenCollection[i];

            // coletando informações da peça atual
            const tokenPosX = toBaseZero(token.positionX);
            const tokenPosY = toBaseZero(token.positionY);
            const tokenSize = token.size;
            const tokenOri = token.direction;
            const tokenChar = token.character;

            // calculando a posição final das peças
            const finalTokenPosX = this.tokenService.calcFinalPosition(token, Axis.X);
            const finalTokenPosY = this.tokenService.calcFinalPosition(token, Axis.Y);

            // console.log("inicial  - " + token.character + " - " + token.positionX + "," + token.positionY)
            // console.log("final XY - " + token.character + " - " + token.finalPositionX + "," + token.finalPositionY)

            // verificando se a posição inicial do token é valida
            if (verifyTokenStartPosition(token, board)) {

                if (tokenSize > 1) {
                    // verificando se a posFinal é > que a de origem, apontando que Y+ (peça descendo)
                    if (tokenOri == Direction.Down) {
                        if (verifyTokenAxisPosition(token, board, Axis.Y, Direction.Down)) {
                            positionInDirection(token, board);
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que Y- (peça subindo)
                    if (tokenOri == Direction.Up) {
                        if (verifyTokenAxisPosition(token, board, Axis.Y, Direction.Up)) {
                            positionInDirection(token, board);
                        }
                    }

                    // verificando se a posFinal é > que a de origem, apontando que X+ (peça p/ direita)
                    if (tokenOri == Direction.Right) {
                        if (verifyTokenAxisPosition(token, board, Axis.X, Direction.Right)) {
                            positionInDirection(token, board);
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que X- (peça p/ esquerda)
                    if (tokenOri == Direction.Left) {
                        if (verifyTokenAxisPosition(token, board, Axis.X, Direction.Left)) {
                            positionInDirection(token, board);
                        }
                    }
                }
                else {
                    board.tiles[tokenPosY][tokenPosX] = tokenChar;
                }
            }
        }
        printBoard(board);
    }
}

function printBoard(board: Board) {
    console.table(board.tiles);
}

function getTokenName(tokenChar: string): string {
    switch (tokenChar) {
        case "c":
            return "Chicken"
        case "C":
            return "Cow"
        case "H":
            return "Horse"
        case "B":
            return "Bull"
        default:
            return "";
    }
}

function verifyTokenStartPosition(token: Token, board: Board): boolean {
    const baseZeroPosX = toBaseZero(token.positionX);
    const baseZeroPosY = toBaseZero(token.positionY);
    const baseFinalPosX = toBaseZero(token.finalPositionX);
    const baseFinalPosY = toBaseZero(token.finalPositionY);
    const boardLength = board.tiles.length;

    // verificando se a peça passa do tamanho do boarduleiro
    if ((baseZeroPosX + token.size - 1 > boardLength) || (baseZeroPosY + token.size - 1 > boardLength)) {
        console.log(`Pec > Tab - Coordenadas (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)}) inválidas para a peça ${getTokenName(token.character)}`);
        
        return false;
    }

    // verificando se as posições > 1
    if ((baseFinalPosX < 0) || (baseFinalPosY < 0) || (baseZeroPosY < 0) || (baseZeroPosX < 0)) {
        console.log(`Pos > 1 - Coordenadas (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)}) inválidas para a posição da peça ${getTokenName(token.character)}`)
        
        return false;
    }

    // verificando se a posição final é fora do boarduleiro (X+ Y+)
    if ((baseFinalPosX > boardLength) || (baseFinalPosY > boardLength)) {
        console.log(`PosF > Tab - Coordenadas (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)}) inválidas para a posição final da peça ${getTokenName(token.character)}`)
        
        return false;
    }

    // verificando se tem peça na posição inicial desejada
    if (board.tiles[baseZeroPosY][baseZeroPosX] != 'O') {
        let boardToken = board.tiles[baseZeroPosY][baseZeroPosX];

        console.log(`X - A peça ${getTokenName(boardToken)} já está na coordenada (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)})`);
        return false;
    }

    return true;
}

function verifyTokenAxisPosition(token: Token, board: Board, axis: Axis, direction: Direction): boolean {
    // verificando se há parte de uma peça em algum ponto da posição desejada
    const baseZeroPosX = toBaseZero(token.positionX);
    const baseZeroPosY = toBaseZero(token.positionY);

    if (axis == Axis.X && direction == Direction.Right) {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = baseZeroPosX; i < baseZeroPosX + token.size; i++) {
            let char = board.tiles[baseZeroPosY][i];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == Axis.X && direction == Direction.Left) {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = 0; i < token.size; i++) {
            let char = board.tiles[baseZeroPosY][baseZeroPosX - i];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == Axis.Y && direction == Direction.Up) {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = 0; i < token.size; i++) {
            let char = board.tiles[baseZeroPosY - i][baseZeroPosX];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == Axis.Y && direction == Direction.Down) {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = baseZeroPosY; i < baseZeroPosY + token.size; i++) {
            let char = board.tiles[i][baseZeroPosX];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${toBaseOne(baseZeroPosX)},Y${toBaseOne(baseZeroPosY)})`);
                return false;
            }
        }
    }

    return true;
}

function verifyDiagonal(token: Token, axis: Axis): boolean | void {

    const baseFinalPosX = toBaseZero(token.finalPositionX);
    const baseFinalPosY = toBaseZero(token.finalPositionY);
    const baseZeroPosX = toBaseZero(token.positionX);
    const baseZeroPosY = toBaseZero(token.positionY);

    if (axis == Axis.X) {
        if (baseFinalPosY != baseZeroPosY) {
            console.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${toBaseOne(baseFinalPosX)},Y${toBaseOne(baseFinalPosY)}) inválida`)
            return true;
        }
    }

    if (axis == Axis.Y) {
        if (baseFinalPosX != baseZeroPosX) {
            console.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${toBaseOne(baseFinalPosX)},Y${toBaseOne(baseFinalPosY)}) inválida`)
            return true;
        }
    }
}

function positionInDirection(token: Token, board: Board) {
    const baseZeroPosX = toBaseZero(token.positionX);
    const baseZeroPosY = toBaseZero(token.positionY);
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
        if (verifyTokenAxisPosition(token, board, Axis.X, Direction.Right)) {
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

function toBaseZero(tokenTile: number): number {
    return tokenTile - 1;
}

function toBaseOne(tokenTile: number): number {
    return tokenTile + 1;
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