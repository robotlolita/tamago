export class Character {
  constructor(readonly value: string) {
  }
}

export function code_point(x: Character) {
  return x.value.codePointAt(0);
}

export function from_code_point(x: bigint) {
  return new Character(String.fromCodePoint(Number(x)));
}

export function to_text(x: Character) {
  return x.value;
}