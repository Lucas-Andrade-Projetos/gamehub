import { Direction } from "../enums/Direction";

export class Token {
    positionX: number;
    positionY: number;
    direction: Direction;
    size: number = 0;
    name: string = "";
    character: string = "";

    constructor(positionX: number, positionY: number, direction: Direction) {
        this.positionX = positionX;
        this.positionY = positionY;
        this.direction = direction;
    }
}