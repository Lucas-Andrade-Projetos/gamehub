import { GameStatus } from "../enums/gameEnums/GameStatus";
import { PlayerNum } from "../enums/gameEnums/PlayerNum";
import { Player } from "./Player";

export class BatalhaRural {
    players: Player[];
    status: GameStatus;

    constructor() {
        this.players = [new Player("mirio", PlayerNum.Player1), 
            new Player("mirio2", PlayerNum.Player2)];
        this.status = GameStatus.Preparing;
    }
}