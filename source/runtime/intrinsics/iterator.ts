export function* map<a, b>(iter: Iterable<a>, f: (_: a) => b) {
  for (const x of iter) {
    yield f(x);
  }
}

export function reduce<a, b>(iter: Iterable<a>, initial: b, f: (_: a, __: b) => b) {
  let acc: b = initial;
  for (const x of iter) {
    acc = f(x, acc);
  }
  return acc;
}