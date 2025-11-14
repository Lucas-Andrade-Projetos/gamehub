import { Board } from "../entities/Board";

export class Utils {
    static toBaseZero(tokenTile: number): number {
        return tokenTile - 1;
    }
    
    static toBaseOne(tokenTile: number): number {
        return tokenTile + 1;
    }

    static printBoard(board: Board) {
        console.table(board.tiles);
    }

    static getTokenName(tokenChar: string): string {
    switch (tokenChar) {
        case "c":
            return "Chicken"
        case "C":
            return "Cow"
        case "H":
            return "Horse"
        case "B":
            return "Bull"
        default:
            return "";
    }
}
}