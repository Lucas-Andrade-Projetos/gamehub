const ANIMAL_EMOJI: Record<string, string> = {
    c: '🐔',
    C: '🐄',
    B: '🐂',
    H: '🐴',
};

export function animalEmoji(character: string): string {
    return ANIMAL_EMOJI[character] ?? character;
}
