import { BatalhaRural } from "../entities/BatalhaRural";
import { GameStatus } from "../enums/gameEnums/GameStatus";
import { PlayerNum } from "../enums/gameEnums/PlayerNum";
import { GameHelper } from "../helpers/GameHelper";
import { Utils } from "../helpers/Utils";

export class GameService {

    static prepareGame(game: BatalhaRural,) {
        game.status = GameStatus.Preparing;

        const player = game.players[0];

        for (let i = 0; i < player.tokens.tokenCollection.length; i++) { // passando pelos tokens do player
            const token = player.tokens.tokenCollection[i];
            
        }
    }

    static startGame() { }

    static refreshGame(game: BatalhaRural) {
        GameHelper.positionTokens(game.players);

        Utils.printGame(game.players);
    }
}