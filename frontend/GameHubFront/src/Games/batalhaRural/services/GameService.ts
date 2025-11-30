import { BatalhaRural } from "../entities/BatalhaRural";
import { Player } from "../entities/Player";
import { GameStatus } from "../enums/gameEnums/GameStatus";
import { PlayerNum } from "../enums/gameEnums/PlayerNum";
import { PlayerStatus } from "../enums/gameEnums/PlayerStatus";
import { GameHelper } from "../helpers/GameHelper";
import { Utils } from "../helpers/Utils";

export class GameService {

    static prepareGame(game: BatalhaRural, ) {
        game.status = GameStatus.Preparing;

        const player = PlayerNum.Player1;

    }

    static startGame() {}

    static refreshGame(game: BatalhaRural) {
        GameHelper.positionTokens(game.players);

        Utils.printGame(game.players);
    }
}