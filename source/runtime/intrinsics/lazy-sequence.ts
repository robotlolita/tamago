// This currently only works for iterables that can
// produce all their values synchronously. It's also
// innefficient
export class LazySequence {
  private _buffer: any[];
  constructor(iterable: Iterable<any>) {
    this._buffer = [...iterable];
  }

  *[Symbol.iterator]() {
    return this._buffer;
  }
}