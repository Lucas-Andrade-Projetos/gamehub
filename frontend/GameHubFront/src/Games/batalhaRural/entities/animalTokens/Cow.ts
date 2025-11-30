import { Direction } from "../../enums/Direction";
import { Token } from "../Token";

export class Cow extends Token {
    override name: string = "Cow";
    override character: string = "C";
    override size: number = 2;

    constructor(positionX: number, positionY: number, orientation: Direction) {
        super(positionX, positionY, orientation);
    }
}
