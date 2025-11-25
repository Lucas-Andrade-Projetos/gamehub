import { Board } from "../entities/Board";
import { TokenInfo } from "../helpers/TokenInfo";

export class TokenService {

    static placeTokenUp(tokenInfo: TokenInfo, board: Board) {
        for (let k = 0; k < tokenInfo.tokenSize; k++) {
            board.tiles[tokenInfo.baseZeroPosY - k][tokenInfo.baseZeroPosX] = tokenInfo.tokenChar;
        }
    }

    static placeTokenRight(tokenInfo: TokenInfo, board: Board) {
        for (let k = 0; k < tokenInfo.tokenSize; k++) {
            board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX + k] = tokenInfo.tokenChar;
        }
    }

    static placeTokenDown(tokenInfo: TokenInfo, board: Board) {
        for (let k = 0; k < tokenInfo.tokenSize; k++) {
            board.tiles[tokenInfo.baseZeroPosY + k][tokenInfo.baseZeroPosX] = tokenInfo.tokenChar;
        }
    }

    static placeTokenLeft(tokenInfo: TokenInfo, board: Board) {
        for (let k = 0; k < tokenInfo.tokenSize; k++) {
            board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX - k] = tokenInfo.tokenChar;
        }
    }
}
