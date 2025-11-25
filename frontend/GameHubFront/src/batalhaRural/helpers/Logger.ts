import { TokenInfo } from "./TokenInfo";

export class Logger {
    static outOfBounds(tokenInfo: TokenInfo) {
        console.log(`X - Coordenadas excedendo o tamanho do tabuleiro`);
    }

    static collision() {
        console.log(`X - Há uma peça no caminho, não foi possível posicionar`);
    }

    static diagonal() {
        console.log(`X - Não é possível posicionar peças na diagonal`)
    }

    static invalidInput() {
        console.log(`X - Input de coordenadas inválido`)
    }
}