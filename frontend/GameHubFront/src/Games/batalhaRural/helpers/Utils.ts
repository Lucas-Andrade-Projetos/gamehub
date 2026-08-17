export class Utils {
    static toBaseZero(tokenTile: number): number {
        return tokenTile - 1;
    }

    static toBaseOne(tokenTile: number): number {
        return tokenTile + 1;
    }
}