type tuple = any[];

export function size(a: tuple) {
  return a.length;
}

export function at(a: tuple, index: bigint) {
  return a[Number(index)];
}

export function has(a: tuple, index: bigint) {
  return Number(index) in a;
}