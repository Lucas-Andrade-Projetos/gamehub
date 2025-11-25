import { Utils } from "./Utils";
import { Token } from "../entities/Token";
import { CoordinateHelper } from "./CoordinateHelper";
import { Direction } from "../enums/Direction";
import { Axis } from "../enums/Axis";

export class TokenInfo {
    readonly baseOnePosX: number;
    readonly baseOnePosY: number;
    readonly tokenSize: number;
    readonly tokenDir: Direction;
    readonly tokenName: string;
    readonly tokenChar: string;
    
    get baseOneFinalPosX(): number {
        return CoordinateHelper.calcHorizontalFinalPosition(this.baseOnePosX, this.tokenSize, this.tokenDir);
    }

    get baseOneFinalPosY(): number {
        return CoordinateHelper.calcVerticalFinalPosition(this.baseOnePosY, this.tokenSize, this.tokenDir);
    }

    get baseZeroFinalPosX(): number {
        return Utils.toBaseZero(this.baseOneFinalPosX);
    }

    get baseZeroFinalPosY(): number {
        return Utils.toBaseZero(this.baseOneFinalPosY);
    }

    get baseZeroPosX(): number {
        return Utils.toBaseZero(this.baseOnePosX);
    }

    get baseZeroPosY(): number {
        return Utils.toBaseZero(this.baseOnePosY);
    }

    get tokenAxis(): Axis {
        if (this.tokenDir == Direction.Up || this.tokenDir == Direction.Down) { return Axis.Y; }
        else { return Axis.X; }
    }

    constructor(token: Token) {
        this.tokenSize = token.size;
        this.tokenDir = token.direction;
        this.tokenName = token.name;
        this.tokenChar = token.character;
        this.baseOnePosX = token.positionX;
        this.baseOnePosY = token.positionY;
    }
}