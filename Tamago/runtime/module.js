const { assert } = require('./assert');
const { force } = require('./thunk');
const { define_record, define_variant, define_union } = require('./record');
const { TestCase } = require('./test');

class TamagoModule {
  constructor(id) {
    this._id = id;
    this._tests = [];
    this._bindings = new Map();
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

  $project(name) {
    const value = this._bindings.get(name);
    assert(value !== undefined, `Undefined ${name} in module ${this.id}`);
    return force(value);
  }

  toString() {
    return `TamagoModule<${this._id}>`;
  }
}


class TamagoSubmodule extends TamagoModule {
  constructor(parent, name) {
    super(parent._id);
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