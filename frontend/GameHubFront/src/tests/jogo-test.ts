import { Tabuleiro } from "../batalhaRural/Tabuleiro";
import { TabuleiroService } from "../batalhaRural/TabuleiroService";
import { Tokens } from "../batalhaRural/Tokens";

TabuleiroService.positionTokens(new Tabuleiro(10), new Tokens());

