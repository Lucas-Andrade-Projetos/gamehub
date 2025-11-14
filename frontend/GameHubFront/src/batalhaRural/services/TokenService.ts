import { Axis } from "../enums/Axis";
import { Direction } from "../enums/Direction";
import { Token } from "../entities/Token";

export class TokenService {
    calcFinalPosition(token: Token, axis: Axis): number {
        if ((token.direction == Direction.Right) && (axis == Axis.X)) {
            token.finalPositionX = token.positionX + token.size - 1;
            return token.finalPositionX;
        }

        if ((token.direction == Direction.Left) && (axis == Axis.X)) {
            token.finalPositionX = token.positionX - token.size + 1
            return token.finalPositionX;
        }

        if (token.direction == Direction.Centered) {
            token.finalPositionX = token.positionX;
            token.finalPositionY = token.positionY;

            if (axis == Axis.X) return token.finalPositionX;
            if (axis == Axis.Y) return token.finalPositionY;
        }

        if ((token.direction == Direction.Down) && (axis == Axis.Y)) {
            token.finalPositionY = token.positionY + token.size - 1;
            return token.finalPositionY;
        }

        if ((token.direction == Direction.Up) && (axis == Axis.Y)) {
            token.finalPositionY = token.positionY - token.size + 1;
            return token.finalPositionY;
        }

        return verifyCheckedAxis(axis, token.direction, token);
    }
}

function verifyCheckedAxis(axis: Axis, direction: Direction, token: Token): number {
    if (axis == Axis.X && (direction == Direction.Down || direction == Direction.Up)) {
        token.finalPositionX = token.positionX;
        return token.finalPositionX;
    }

    if (axis == Axis.Y && (direction == Direction.Left || direction == Direction.Right)) {
        token.finalPositionY = token.positionY;
        return token.finalPositionY;
    }

    return 0;
}