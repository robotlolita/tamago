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

export function textual_representation(x: Character) {
  return `<character: ${JSON.stringify(x.value)}>`;
}