import { Direction } from "../../enums/Direction";
import { Token } from "../Token";

export class Bull extends Token {
    override name: string = "Bull";
    override character: string = "B";
    override size: number = 5;

    constructor(positionX: number, positionY: number, orientation: Direction) {
        super(positionX, positionY, orientation);
    }
}