import { Board } from "../../entities/Board";
import { TokenInfo } from "../../helpers/TokenInfo";
import { Logger } from "../../helpers/Logger";

export class InputValidator {

    static isInputValid(tokenInfo: TokenInfo, board: Board): boolean {
        const boardLength = board.tiles.length;

        // verificando se as coordenadas estão dentro do tabuleiro
        if ((tokenInfo.baseOnePosX > boardLength) || (tokenInfo.baseOnePosY > boardLength) || (tokenInfo.baseOnePosX < 1) || (tokenInfo.baseOnePosY < 1)) {
            Logger.invalidInput();

            return false;
        }

        return true;
    }
}