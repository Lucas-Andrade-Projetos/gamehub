import { Bull } from "./animalTokens/Bull";
import { Chicken } from "./animalTokens/Chicken";
import { Cow } from "./animalTokens/Cow";
import { Horse } from "./animalTokens/Horse";
import { Token } from "./Token";

export class Tokens {
    tokenCollection: Token[] = [];

    constructor() {
        this.tokenCollection = [
            new Chicken(10, 1), new Chicken(2, 1), new Chicken(3, 1),
            new Cow(1, 2, 1, 3), 
            new Horse(1, 4, 1, 5), new Horse(2, 4, 2, 5),
            new Bull(6, 2, 10, 2), new Cow(4, 4, 3, 4)
        ];
    }
}