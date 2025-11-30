import { BatalhaRural } from "../Games/batalhaRural/entities/BatalhaRural";
import { GameStatus } from "../Games/batalhaRural/enums/gameEnums/GameStatus";
import { GameService } from "../Games/batalhaRural/services/GameService";

let game = new BatalhaRural();

while (game.status == GameStatus.inProgress) GameService.refreshGame(game);