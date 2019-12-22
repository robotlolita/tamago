const { assert } = require('./assert');
const { force, delay } = require('./thunk');
const { TamagoModule } = require('./module');
const { TamagoAnonymousRecord } = require('./record');
const { empty, Cons } = require('./list');
const { match, patterns } = require('./match');

class TamagoRuntime {
  constructor() {
    this._modules = new Map();
    this._pending = [];
  }

  define_module(id, init) {
    assert(!this._modules.has(id), `Duplicated module ${id}`);

    const module = new TamagoModule(id);
    this._modules.set(id, module);
    this._pending.push({ module, init });
  }

  import_module(id) {
    const module = this._modules.get(id);
    assert(module != null, `Undefined module ${id}`);
    return module;
  }

  thunk(expr) {
    return delay(expr);
  }

  force(expr) {
    return force(expr);
  }

  assert(expr) {
    assert(expr, "Assertion failed");
  }

  assert_match(left, right) {
    assert(equals(left, right), `Assertion failed: ${left} ==> ${right}`);
  }

  record(fields) {
    Object.setPrototypeOf(fields, null);
    return new TamagoAnonymousRecord(fields);
  }

  check_arity(n, f) {
    return function(arguments) {
      assert(arguments.length === n, `Invalid arity: expected ${n}, got ${arguments.length}`);
      return f(...arguments);
    }
  }

  cons(head, tail) {
    return new Cons(head, tail);
  }

  empty() {
    return empty;
  }

  match(value, cases) {
    return match(value, cases);
  }

  initialise() {
    const pending = this._pending;
    this._pending = [];
    for (const { module, init } of pending) {
      init(module);
    }
  }

  run_tests() {
    let errors = 0;
    for (const module of this._modules.values()) {
      errors += module.run_tests();
    }
    return errors;
  }

  get pattern() {
    return patterns;
  }
}

module.exports = {
  TamagoRuntime
};