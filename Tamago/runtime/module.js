const { assert } = require('./assert');
const { force } = require('./thunk');
const { define_record, define_variant, define_union } = require('./record');
const { TestCase } = require('./test');

class TamagoModule {
  constructor(id) {
    this._id = id;
    this._tests = [];
    this._bindings = new Map();
    this._open = new Map();
  }

  expose(name, value) {
    assert(!this._bindings.has(name), `Duplicated definition ${name} in module ${this._id}`);
    this._bindings.set(name, value);
  }

  define_record(name, props) {
    return define_record(this, name, props);
  }

  define_variant(tag, props) {
    return define_variant(this, tag, props);
  }

  define_union(name, cases) {
    return define_union(this, name, cases);
  }

  define_test(description, test) {
    this._tests.push(new TestCase(this, description, test));
  }

  define_module(name, init) {
    const module = new TamagoSubmodule(this, name);
    init(module);
    return module;
  }

  open(object) {
    assert(object instanceof TamagoModule, `Can only open modules.`);
    for (const [key, value] of object.$exposed()) {
      assert(!this._open.has(key) && !this._bindings.has(key), `Conflicting definition ${key}`);
      this._open.set(key, value);
    }
  }

  $exposed() {
    return this._bindings.entries();
  }

  $project(name) {
    const value = this._bindings.get(name);
    assert(value !== undefined, `Undefined ${name} in module ${this.id}`);
    return force(value);
  }

  $project_scoped(name) {
    const value = this._open.get(name);
    assert(value !== undefined, `Undefined ${name} in module ${this.id}`);
    return force(value);
  }

  toString() {
    return `TamagoModule<${this._id}>`;
  }

  run_tests() {
    let errors = 0;
    console.log(`\n${this._id}\n---`);
    for (const test of this._tests) {
      errors += test.run();
    }
    for (const value of this._bindings.values()) {
      if (value instanceof TamagoSubmodule) {
        value.run_tests();
      }
    }
    return errors;
  }
}


class TamagoSubmodule extends TamagoModule {
  constructor(parent, name) {
    super(parent._id + "/" + name);
    this._parent = parent;
    this._name = name;
  }

  toString() {
    return `TamagoSubmodule<${this._name} in ${this._parent}>`;
  }
}


module.exports = {
  TamagoModule,
  TamagoSubmodule
};