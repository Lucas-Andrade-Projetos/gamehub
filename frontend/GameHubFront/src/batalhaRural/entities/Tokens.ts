import { Direction } from "../enums/Direction";
import { Bull } from "./animalTokens/Bull";
import { Chicken } from "./animalTokens/Chicken";
import { Cow } from "./animalTokens/Cow";
import { Horse } from "./animalTokens/Horse";
import { Token } from "./Token";

export class Tokens {
    tokenCollection: Token[] = [];

    constructor() {
        this.tokenCollection = [
            new Bull(10, 7, Direction.Left),new Chicken(5, 1, Direction.Centered), new Chicken(2, 1, Direction.Centered), new Chicken(3, 1, Direction.Centered),
            new Cow(1, 2, Direction.Right),
            new Horse(1, 4, Direction.Down), new Horse(9, 3, Direction.Right),
            new Cow(6, 5, Direction.Left), 
        ];
    }
}