import { Token } from "../Token";

export class Cow extends Token {
    override character: string = "C";
    override size: number = 2;

    constructor(positionX: number, positionY: number, finalPositionX: number, finalPositionY: number) {
        super(positionX, positionY, finalPositionX, finalPositionY);
    }
}
