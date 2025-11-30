import { Player } from "../entities/Player";
import { Direction } from "../enums/Direction";
import { PositionService } from "../services/PositionService";
import { TokenService } from "../services/TokenService";
import { InputValidator } from "../services/validators/InputValidator";
import { TokenInfo } from "./TokenInfo";

export class GameHelper {

    static positionTokens(players: Player[]) {
        
        players.forEach(player => {
            for (let i = 0; i < player.tokens.tokenCollection.length; i++) {

                let token = player.tokens.tokenCollection[i];

                const tokenInfo = new TokenInfo(token);

                if (InputValidator.isInputValid(tokenInfo, player.board) && PositionService.canPlaceToken(tokenInfo, player.board)) {

                    if (tokenInfo.tokenSize > 1) {

                        if (tokenInfo.tokenDir == Direction.Down) {
                            if (!PositionService.isTokenPathOccupied(tokenInfo, player.board, tokenInfo.tokenAxis, Direction.Down)) {
                                TokenService.placeTokenDown(tokenInfo, player.board);
                            }
                        }

                        if (tokenInfo.tokenDir == Direction.Up) {
                            if (!PositionService.isTokenPathOccupied(tokenInfo, player.board, tokenInfo.tokenAxis, Direction.Up)) {
                                TokenService.placeTokenUp(tokenInfo, player.board);
                            }
                        }

                        if (tokenInfo.tokenDir == Direction.Right) {
                            if (!PositionService.isTokenPathOccupied(tokenInfo, player.board, tokenInfo.tokenAxis, Direction.Right)) {
                                TokenService.placeTokenRight(tokenInfo, player.board);
                            }
                        }

                        if (tokenInfo.tokenDir == Direction.Left) {
                            if (!PositionService.isTokenPathOccupied(tokenInfo, player.board, tokenInfo.tokenAxis, Direction.Left)) {
                                TokenService.placeTokenLeft(tokenInfo, player.board);
                            }
                        }
                    }
                    else {
                        player.board.tiles[tokenInfo.baseZeroPosY][tokenInfo.baseZeroPosX] = tokenInfo.tokenChar;
                    }
                }
            }
        });
    }
}