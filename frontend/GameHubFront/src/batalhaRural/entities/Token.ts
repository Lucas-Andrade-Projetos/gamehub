import { Direction } from "../enums/Direction";

export class Token {
    size: number = 0;
    positionX: number;
    positionY: number;
    direction: Direction;
    finalPositionX: number = 0;
    finalPositionY: number = 0;
    character: string = "";

    constructor(positionX: number, positionY: number, direction: Direction) {
        this.positionX = positionX;
        this.positionY = positionY;
        this.direction = direction;
    }
}