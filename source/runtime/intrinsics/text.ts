import { map, reduce } from "./iterator";
import { Character } from "./character";
import { LazySequence } from "./lazy-sequence";

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
  return new LazySequence(map(a, x => BigInt(x.codePointAt(0))));
}

export function to_characters(a: string) {
  return new LazySequence(map(a, x => new Character(x)));
}

export function from_characters(a: Iterable<Character>) {
  return reduce(a, "", (b, a) => a + b.value);
}