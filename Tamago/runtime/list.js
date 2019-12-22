const { assert } = require('./assert');

const empty = new class Empty {

}

class Cons {
  constructor(value, rest) {
    this._value = value;
    this._rest = rest;
  }
}

module.exports = {
  empty,
  Cons
};
