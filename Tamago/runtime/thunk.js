class Thunk {
  constructor(expr) {
    this._expr = expr;
    this._computed = false;
    this._value = undefined;
  }

  force() {
    if (this._computed) {
      return this._value;
    } else {
      const expr = this._expr;
      const value = expr();
      this._computed = true;
      this._value = value;
      return value;
    }
  }
}

function force(value) {
  if (value instanceof Thunk) {
    return value.force();
  } else {
    return value;
  }
}

function delay(expr) {
  return new Thunk(expr);
}

module.exports = {
  Thunk, force, delay
};