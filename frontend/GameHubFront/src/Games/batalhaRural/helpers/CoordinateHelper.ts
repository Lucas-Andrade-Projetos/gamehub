import { Direction } from "../enums/Direction";

export class CoordinateHelper {

    static calcHorizontalFinalPosition(baseOnePosX: number, size: number, direction: Direction): number {

        if (direction == Direction.Right) return baseOnePosX + size - 1;

        if (direction == Direction.Left) return baseOnePosX - size + 1;

        return baseOnePosX;
    }

    static calcVerticalFinalPosition(baseOnePosY: number, size: number, direction: Direction): number {

        if (direction == Direction.Down) return baseOnePosY + size - 1;

        if (direction == Direction.Up) return baseOnePosY - size + 1;

        return baseOnePosY;
    }
}