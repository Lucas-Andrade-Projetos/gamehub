import { Token } from "../Token";

export class Bull extends Token {
    override character: string = "B";
    override size: number = 5;

    constructor(positionX: number, positionY: number, finalPositionX: number, finalPositionY: number) {
        super(positionX, positionY, finalPositionX, finalPositionY);
    }
}