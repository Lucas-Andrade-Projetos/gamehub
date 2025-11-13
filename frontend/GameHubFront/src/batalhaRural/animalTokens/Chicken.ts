import { Token } from "../Token";

export class Chicken extends Token {
    override character: string = "c";
    override size: number = 1;

    constructor(positionX: number, positionY: number) {
        super(positionX, positionY, positionX, positionY);
    }
}