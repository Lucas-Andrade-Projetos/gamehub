import { Direction } from "../../enums/Direction";
import { Token } from "../Token";

export class Horse extends Token {
    override character: string = "H";
    override size: number = 2;

    constructor(positionX: number, positionY: number, orientation: Direction) {
        super(positionX, positionY, orientation);
    }
}
