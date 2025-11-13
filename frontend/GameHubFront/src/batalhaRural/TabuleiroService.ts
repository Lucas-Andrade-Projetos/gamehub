import { Tabuleiro } from "./Tabuleiro";
import { Token } from "./Token";
import { Tokens } from "./Tokens";

export class TabuleiroService {

    static positionTokens(tab: Tabuleiro, tokens: Tokens) {
        for (let i = 0; i < tokens.tokenCollection.length; i++) {

            // peça a ser posicionada
            let token = tokens.tokenCollection[i];

            // verificando se a posição inicial do token é valida
            if (verifyTokenStartPosition(token, tab)) {

                // coletando informações da peça atual
                const tokenPosX = token.positionX;
                const tokenPosY = token.positionY;
                const tokenSize = token.size;
                const tokenChar = token.character;

                if (tokenSize > 1) {
                    const finalTokenPosX = token.finalPositionX!;
                    const finalTokenPosY = token.finalPositionY!;

                    // verificando se a posFinal é > que a de origem, apontando que Y+ (peça descendo)
                    if (calcRealPosition(finalTokenPosY) > calcRealPosition(tokenPosY)) {
                        if (verifyTokenAxisPosition(token, tab, "y", "down")) {
                            for (let k = 0; k < tokenSize; k++) {
                                tab.casas[calcRealPosition(tokenPosY) + k][calcRealPosition(tokenPosX)] = tokenChar;
                            }
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que Y- (peça subindo)
                    if (calcRealPosition(finalTokenPosY) < calcRealPosition(tokenPosY)) {
                        if (verifyTokenAxisPosition(token, tab, "y", "up")) {
                            for (let k = 0; k < tokenSize; k++) {
                                tab.casas[calcRealPosition(tokenPosY) - k][calcRealPosition(tokenPosX)] = tokenChar;
                            }
                        }
                    }

                    // verificando se a posFinal é > que a de origem, apontando que X+ (peça p/ direita)
                    if (calcRealPosition(finalTokenPosX) > calcRealPosition(tokenPosX)) {
                        if (verifyTokenAxisPosition(token, tab, "x", "right")) {
                            for (let k = 0; k < tokenSize; k++) {
                                tab.casas[calcRealPosition(tokenPosY)][calcRealPosition(tokenPosX) + k] = tokenChar;
                            }
                        }
                    }

                    // verificando se a posFinal é < que a de origem, apontando que X- (peça p/ esquerda)
                    if (calcRealPosition(finalTokenPosX) < calcRealPosition(tokenPosX)) {
                        if (verifyTokenAxisPosition(token, tab, "x", "left")) {
                            for (let k = 0; k < tokenSize; k++) {
                                tab.casas[calcRealPosition(tokenPosY)][calcRealPosition(tokenPosX) - k] = tokenChar;
                            }
                        }
                    }
                }
                else {
                    tab.casas[calcRealPosition(tokenPosY)][calcRealPosition(tokenPosX)] = tokenChar;
                }
            }
        }
        printBoard(tab);
    }
}

function printBoard(board: Tabuleiro) {
    console.table(board.casas);
}

function getTokenName(tokenChar: string): string {
    let animalName: string;

    (tokenChar == "c") ? animalName = "Chicken" : (tokenChar == "C") ? animalName = "Cow" :
        (tokenChar == "H") ? animalName = "Horse" : (tokenChar == "B") ? animalName = "Bull" : animalName = ""

    return animalName;
}

function verifyTokenStartPosition(token: Token, tab: Tabuleiro): boolean {
    // verificando se a peça passa do tamanho do tabuleiro
    if ((calcRealPosition(token.positionX) + token.size - 1 > tab.casas.length) || (calcRealPosition(token.positionY) + token.size - 1 > tab.casas.length)) {
        console.log(`X - Coordenadas (X${token.positionX},Y${token.positionY}) inválidas para a peça ${getTokenName(token.character)}`);
        return false;
    }

    // verificando se a posição é pelo menos 1
    if ((calcRealPosition(token.finalPositionX!) < 0) || (calcRealPosition(token.finalPositionY!) < 0) || (calcRealPosition(token.positionY!) < 0) || (calcRealPosition(token.positionX) < 0)) {
        console.log(`X - Coordenadas (X${token.positionX},Y${token.positionY}) inválidas para a posição da peça ${getTokenName(token.character)}`)
        return false;
    }

    // verificando se a posição final é fora do tabuleiro
    if ((calcRealPosition(token.finalPositionX!) > tab.casas.length) || (calcRealPosition(token.finalPositionY!) > tab.casas.length)) {
        console.log(`X - Coordenadas (X${token.positionX},Y${token.positionY}) inválidas para a posição final da peça ${getTokenName(token.character)}`)
        return false;
    }

    // verificando se tem peça na posição inicial desejada
    if (tab.casas[calcRealPosition(token.positionY)][calcRealPosition(token.positionX)] != 'O') {
        let boardToken = tab.casas[calcRealPosition(token.positionY)][calcRealPosition(token.positionX)];

        console.log(`X - A peça ${getTokenName(boardToken)} já está na coordenada (X${token.positionX},Y${token.positionY})`);
        return false;
    }

    return true;
}

function verifyTokenAxisPosition(token: Token, tab: Tabuleiro, axis: "x" | "y", way: "up" | "down" | "left" | "right"): boolean {
    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == "x" && way == "right") {

        if (verifyDiagonal(token, axis)) return false;

        const realPos = calcRealPosition(token.positionX);
        for (let i = realPos; i < realPos + token.size; i++) {
            let char = tab.casas[calcRealPosition(token.positionY)][i];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${token.positionX},Y${token.positionY})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == "x" && way == "left") {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = 0; i < token.size; i++) {
            let char = tab.casas[calcRealPosition(token.positionY)][calcRealPosition(token.positionX) - i];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${token.positionX},Y${token.positionY})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == "y" && way == "up") {

        if (verifyDiagonal(token, axis)) return false;

        for (let i = 0; i < token.size; i++) {
            let char = tab.casas[calcRealPosition(token.positionY) - i][calcRealPosition(token.positionX)];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${token.positionX},Y${token.positionY})`);
                return false;
            }
        }
    }

    // verificando se há parte de uma peça em algum ponto da posição desejada
    if (axis == "y" && way == "down") {

        if (verifyDiagonal(token, axis)) return false;

        const realPos = calcRealPosition(token.positionY);
        for (let i = realPos; i < realPos + token.size; i++) {
            let char = tab.casas[i][calcRealPosition(token.positionX)];

            if (char != 'O') {
                console.log(`X - Uma parte da peça ${getTokenName(char)} já está na coordenada (X${token.positionX},Y${token.positionY})`);
                return false;
            }
        }
    }

    return true;
}

function verifyDiagonal(token: Token, axis: string): boolean | void {

    if (axis == "x") {
        if (calcRealPosition(token.finalPositionY!) != calcRealPosition(token.positionY)) {
            console.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${token.finalPositionX},Y${token.finalPositionY}) inválida`)
            return true;
        }
    }

    if (axis == "y") {
        if (calcRealPosition(token.finalPositionX!) != calcRealPosition(token.positionX)) {
            console.log(`X - Não é possível posicionar peças na diagonal, coordenada final (X${token.finalPositionX},Y${token.finalPositionY}) inválida`)
            return true;
        }
    }
}

function calcRealPosition(tokenTile: number): number {
    return tokenTile - 1;
}