import { Board } from "../batalhaRural/entities/Board";
import { GameService } from "../batalhaRural/services/GameService";
import { Tokens } from "../batalhaRural/entities/Tokens";

let game = new GameService();

game.positionTokens(new Board(10), new Tokens());

