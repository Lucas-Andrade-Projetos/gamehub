import { Board } from "../batalhaRural/entities/Board";
import { BoardService } from "../batalhaRural/services/BoardService";
import { Tokens } from "../batalhaRural/entities/Tokens";

let game = new BoardService();

game.positionTokens(new Board(10), new Tokens());

