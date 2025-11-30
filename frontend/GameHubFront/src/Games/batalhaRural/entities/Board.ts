export class Board {
    tiles: string[][] = [];
    
    constructor(numTiles: number) {
        for (let i = 0; i < numTiles; i++) {
            this.tiles[i] = [];
            for (let j = 0; j < numTiles; j++) {
                this.tiles[i][j] = "O";
            }
        }
    }
}