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
            new Chicken(0, 0, Direction.Centered), new Chicken(0, 0, Direction.Centered), new Chicken(0, 0, Direction.Centered),
            new Bull(0, 0, Direction.Centered), 
            new Cow(0, 0, Direction.Centered),
            new Horse(0, 0, Direction.Centered), new Horse(0, 0, Direction.Centered),
            new Cow(0, 0, Direction.Centered) 
        ];
    }
}