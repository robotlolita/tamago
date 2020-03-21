import { map } from "./iterator";
import { Character } from "./character";

export function eq(a: string, b: string) {
  return a === b;
}

export function not_eq(a: string, b: string) {
  return a !== b;
}

export function concat(a: string, b: string) {
  return a + b;
}

export function from_code_points(points: bigint[]) {
  return String.fromCodePoint(...map(points, x => Number(x)));
}

export function to_code_points(a: string) {
  return [...a].map(x => BigInt(x.codePointAt(0)));
}

export function to_characters(a: string) {
  return [...a].map(x => new Character(x));
}

export function from_characters(a: Character[]) {
  return a.reduce((a, b) => a + b.value, "");
}