const { assert } = require('./assert');
const { force, delay, Thunk } = require('./thunk');
const { TamagoModule, TamagoSubmodule } = require('./module');
const { TamagoAnonymousRecord, TamagoRecord, TamagoUnion, TamagoRecordInstance } = require('./record');
const { empty, Cons } = require('./list');
const { match, patterns } = require('./match');

class TamagoRuntime {
  constructor() {
    this._modules = new Map();
    this._pending = [];
  }

  get primordials() {
    return {
      empty,
      Cons,
      Thunk,
      TamagoModule,
      TamagoSubmodule,
      TamagoAnonymousRecord,
      TamagoRecord,
      TamagoUnion,
      TamagoRecordInstance
    };
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
    return null;
  }

  assert_match(left, right) {
    assert(builtin.eq(left, right), `Assertion failed: ${left} ==> ${right}`);
    return null;
  }

  record(fields) {
    Object.setPrototypeOf(fields, null);
    return new TamagoAnonymousRecord(fields);
  }

  check_arity(n, f) {
    return function(...args) {
      assert(args.length === n, `Invalid arity: expected ${n}, got ${args.length}`);
      return f(...args);
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

  get builtin() {
    return builtin;
  }

  make_runtime(dirname, filename, require, module) {
    return new TamagoModuleRuntime(this, dirname, filename, require, module);
  }
}

class TamagoModuleRuntime {
  constructor(rt, dirname, filename, require, module) {
    this._runtime = rt;
    this._dirname = dirname;
    this._filename = filename;
    this._require = require;
    this._module = module;
  }

  import_external(id) {
    const trequire = this._require;
    return trequire(id);
  }
}

for (const key of Reflect.ownKeys(TamagoRuntime.prototype)) {
  if (key in TamagoModuleRuntime || ['constructor'].includes(key)) {
    continue;
  }
  if (typeof TamagoRuntime.prototype[key] === "function") {
    TamagoModuleRuntime.prototype[key] = function(...args) {
      return this._runtime[key](...args);
    }
  } else {
    TamagoModuleRuntime.prototype[key] = TamagoRuntime.prototype[key];
  }
}

module.exports = {
  TamagoRuntime
};