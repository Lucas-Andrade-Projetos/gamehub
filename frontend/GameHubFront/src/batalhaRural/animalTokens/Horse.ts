import { Token } from "../Token";

export class Horse extends Token {
    override character: string = "H";
    override size: number = 2;

    constructor(positionX: number, positionY: number, finalPositionX: number, finalPositionY: number) {
        super(positionX, positionY, finalPositionX, finalPositionY);
    }
}
