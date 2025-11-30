import { PlayerNum } from "../enums/gameEnums/PlayerNum";
import { PlayerStatus } from "../enums/gameEnums/PlayerStatus";
import { Board } from "./Board";
import { Tokens } from "./Tokens";

export class Player {
    id: number = Math.random();
    nickname: string;
    board: Board;
    tokens: Tokens;
    playerNum: PlayerNum;
    playerStatus: PlayerStatus;

    constructor(nickname: string, playerNum: PlayerNum) {
        this.nickname = nickname;
        this.board = new Board(10);
        this.tokens = new Tokens();
        this.playerNum = playerNum;
        this.playerStatus = PlayerStatus.Preparing;
    }
}