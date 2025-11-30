import { Board } from "../entities/Board";
import { Player } from "../entities/Player";

export class Utils {
    static toBaseZero(tokenTile: number): number {
        return tokenTile - 1;
    }

    static toBaseOne(tokenTile: number): number {
        return tokenTile + 1;
    }

    static printGame(players: Player[]) {
        players.forEach(player => {
            console.log("---------");
            console.log(`${player.playerNum}: ${player.nickname} - Board`);
            console.table(player.board.tiles);
        });
    }
}