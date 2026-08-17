import { Board } from "../../entities/Board";
import { Logger } from "../../helpers/Logger";
import { TokenInfo } from "../../helpers/TokenInfo";

export class BoundRuler {

    static isOutOfBounds(tokenInfo: TokenInfo, board: Board): boolean {
        if ((tokenInfo.baseZeroFinalPosX >= board.tiles.length) || (tokenInfo.baseZeroFinalPosY >= board.tiles.length)) {
            Logger.outOfBounds(tokenInfo)
            return true;
        }

        if ((tokenInfo.baseZeroFinalPosX < 0) || (tokenInfo.baseZeroFinalPosY < 0)) {
            Logger.outOfBounds(tokenInfo);
            return true;
        }

        return false;
    }
}