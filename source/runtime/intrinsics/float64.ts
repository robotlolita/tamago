//== Comparison
export function eq(a: number, b: number) {
  return a === b;
}

export function not_eq(a: number, b: number) {
  return a !== b;
}

export function compare(a: number, b: number, lessThan: any, equal: any, greaterThan: any) {
  if (a < b) {
    return lessThan;
  } else if (a > b) {
    return greaterThan;
  } else {
    return equal;
  }
}

export function lt(a: number, b: number) {
  return a < b;
}

export function lte(a: number, b: number) {
  return a <= b;
}

export function gt(a: number, b: number) {
  return a > b;
}

export function gte(a: number, b: number) {
  return a >= b;
}

export function add(a: number, b: number) {
  return a + b;
}

export function sub(a: number, b: number) {
  return a - b;
}

export function mul(a: number, b: number) {
  return a * b;
}

export function div(a: number, b: number) {
  return a / b;
}

export function remainder(a: number, b: number) {
  return a % b;
}

export function power(a: number, b: number) {
  return a ** b;
}

//== Bitwise
export function bit_shr(a: number, b: bigint) {
  return a >> Number(b);
}

export function bit_unsigned_shr(a: number, b: bigint) {
  return a >>> Number(b);
}

export function bit_shl(a: number, b: bigint) {
  return a << Number(b);
}

export function bit_and(a: number, b: number) {
  return a & b;
}

export function bit_or(a: number, b: number) {
  return a | b;
}

export function bit_xor(a: number, b: number) {
  return a ^ b;
}

export function bit_not(a: number) {
  return ~a;
}

export function bit_at(a: number, index: bigint) {
  return (a & (1 << Number(index))) === 0? 0n : 1n;
}

export function bit_set(a: number, index: bigint) {
  return a | (1 << Number(index));
}

export function bit_clear(a: number, index: bigint) {
  return a & ~(1 << Number(index));
}

export function bit_toggle(a: number, index: bigint) {
  return a ^ (1 << Number(index));
}

//== Conversions
export function to_integer(a: number) {
  return BigInt(a);
}

export function to_digits(a: number, base: bigint) {
  const baseF = Number(base);
  return a.toString(baseF).split("").map(x => BigInt(parseInt(x, baseF)));
}


//== Floating point
export const max_value = Number.MAX_VALUE;
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