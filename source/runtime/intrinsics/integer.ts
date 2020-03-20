//== Comparison
export function eq(a: bigint, b: bigint) {
  return a === b;
}

export function not_eq(a: bigint, b: bigint) {
  return a !== b;
}

export function compare(a: bigint, b: bigint, lessThan: any, equal: any, greaterThan: any) {
  if (a < b) {
    return lessThan;
  } else if (a > b) {
    return greaterThan;
  } else {
    return equal;
  }
}

export function lt(a: bigint, b: bigint) {
  return a < b;
}

export function lte(a: bigint, b: bigint) {
  return a <= b;
}

export function gt(a: bigint, b: bigint) {
  return a > b;
}

export function gte(a: bigint, b: bigint) {
  return a >= b;
}

//== Arithmetic
export function add(a: bigint, b: bigint) {
  return a + b;
}

export function sub(a: bigint, b: bigint) {
  return a - b;
}

export function mul(a: bigint, b: bigint) {
  return a * b;
}

export function div(a: bigint, b: bigint) {
  return a / b;
}

export function remainder(a: bigint, b: bigint) {
  return a % b;
}

export function power(a: bigint, b: bigint) {
  return a ** b;
}

//== Bitwise
export function bit_shr(a: bigint, b: bigint) {
  return a >> b;
}

export function bit_shl(a: bigint, b: bigint) {
  return a << b;
}

export function bit_and(a: bigint, b: bigint) {
  return a & b;
}

export function bit_or(a: bigint, b: bigint) {
  return a | b;
}

export function bit_xor(a: bigint, b: bigint) {
  return a ^ b;
}

export function bit_not(a: bigint) {
  return ~a;
}

export function bit_at(a: bigint, index: bigint) {
  return (a & (1n << index)) === 0n? 0n : 1n;
}

export function bit_set(a: bigint, index: bigint) {
  return a | (1n << index);
}

export function bit_clear(a: bigint, index: bigint) {
  return a & ~(1n << index);
}

export function bit_toggle(a: bigint, index: bigint) {
  return a ^ (1n << index);
}

//== Conversions
export function to_float64(a: bigint) {
  return Number(a);
}

export function to_digits(a: bigint, base: bigint) {
  const baseF = Number(base);
  return a.toString(baseF).split("").map(x => BigInt(parseInt(x, baseF)));
}

//== Floating point
export function max_value() {
  return Number.MAX_VALUE;
}

export const min_value = Number.MIN_VALUE;
export const max_safe_integer = Number.MAX_SAFE_INTEGER;
export const min_safe_integer = Number.MIN_SAFE_INTEGER;
export const nan = Number.NaN;
export const positive_infinity = Number.POSITIVE_INFINITY;
export const negative_infinity = Number.NEGATIVE_INFINITY;

export function is_nan(a: number) {
  return Number.isNaN(a);
}

export function is_finite(a: number) {
  return Number.isFinite(a);
}

export function is_integer(a: number) {
  return Number.isInteger(a);
}

export function is_safe_integer(a: number) {
  return Number.isSafeInteger(a);
}

export function abs(a: number) {
  return Math.abs(a);
}

export function floor(a: number) {
  return Math.floor(a);
}

export function ceil(a: number) {
  return Math.ceil(a);
}

export function round(a: number) {
  return Math.round(a);
}

export function truncate(a: number) {
  return Math.trunc(a);
}