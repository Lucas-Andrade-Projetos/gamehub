export class Tabuleiro {
    casas: string[][] = [];
    
    constructor(numCasas: number) {
        for (let i = 0; i < numCasas; i++) {
            this.casas[i] = [];
            for (let j = 0; j < numCasas; j++) {
                this.casas[i][j] = "O";
            }
        }
    }
}