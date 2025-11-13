export class Token {
    size: number = 0;
    positionX: number;
    positionY: number;
    finalPositionX: number | undefined;
    finalPositionY: number | undefined;
    character: string = "";

    constructor(positionX: number, positionY: number);
    constructor(positionX: number, positionY: number, finalPositionX: number, finalPositionY: number);

    constructor(positionX: number, positionY: number, finalPositionX?: number, finalPositionY?: number) {
        this.positionX = positionX;
        this.positionY = positionY;
        this.finalPositionX = finalPositionX;
        this.finalPositionY = finalPositionY;
    }
}