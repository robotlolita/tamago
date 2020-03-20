type List = Cons | Empty;

//== Data type
export class Cons {
  constructor(readonly head: any, readonly tail: List) {}

  *[Symbol.iterator]() {
    yield this.head;
    yield* this.tail;
  }
}

class Empty {
  *[Symbol.iterator]() {}
}

export const empty = new Empty();