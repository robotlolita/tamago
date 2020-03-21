export function* map<a, b>(iter: Iterable<a>, f: (_: a) => b) {
  for (const x of iter) {
    yield f(x);
  }
}