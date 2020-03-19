const {Panic, show, prop, load_natives, equals} = require("./native");

const TagRecord = new class TRecord {};
const EFFECT_HANDLED = new class Handled {};
const EFFECT_NOT_HANDLED = new class NotHandled {};
const TAMAGO_TAG = Symbol("Tamago Tag");
const COLOUR_DEF='\x1b[0m'
const COLOUR_RED='\x1b[35m'
const COLOUR_GREEN='\x1b[32m'

function cgreen(text) {
  return `${COLOUR_GREEN}${text}${COLOUR_DEF}`;
}

function cred(text) {
  return `${COLOUR_RED}${text}${COLOUR_DEF}`;
}

function indent(n, text) {
  return text.split(/\n/).map(l => `${" ".repeat(n)}${l}`).join("\n");
}

function all_keys(a) {
  const proto = Object.getPrototypeOf(a);
  if (proto == null) {
    return Object.getOwnPropertyNames(a);
  } else {
    return Object.getOwnPropertyNames(a).concat(all_keys(proto));
  }
}

function same_record(a, b) {
  const ak = new Set(all_keys(a));
  const bk = new Set(all_keys(b));

  if (ak.size !== bk.size) {
    return false;
  }
  
  for (const key of ak.values()) {
    if (!/^@t:/.test(key)) {
      continue;
    }
    if (!bk.has(key) || !equals(a[key], b[key])) {
      return false;
    }
  }

  return true;
}

class Tamago {
  constructor() {
    this._namespaces = new Map();
  }

  define_namespace(id, initialiser) {
    if (this._namespaces.has(id)) {
      throw new Panic(`Duplicated namespace ${id}`);
    }
    this._namespaces.set(id, new Namespace(id, initialiser));
  }

  use_namespace(id) {
    const namespace = this._namespaces.get(id);
    if (!namespace) {
      throw new Panic(`Undefined namespace ${id}`);
    }
    return namespace.load();
  }

  symbol(value) {
    if (typeof value !== "symbol") {
      throw new Panic(`Expected symbol`);
    }
    return value;
  }

  check_arity(name, expected, actual) {
    if (expected !== actual) {
      throw new Panic(`${name} expects ${expected} arguments; got ${actual}`);
    }
  }

  assertion_failed(message) {
    throw new Panic(`Assertion failed: ${message}`);
  }

  unmatched(value) {
    throw new Panic(`Unmatched ${show(value)}`);
  }

  unreachable(message) {
    throw new Panic(`Unreachable ${message}`);
  }

  use_natives() {
    load_natives(this);
  }

  instance_of(type, value) {
    return type.tamago_has_instance(value);
  }

  as_type(type) {
    return {
      tamago_has_instance(value) {
        return value === type;
      },
      tamago_show() {
        return `<type>`;
      }
    };
  }

  update(type, data) {
    return type.tamago_update(data);
  }

  show(value) {
    return show(value);
  }

  thunk(fn) {
    return new Lazy(fn);
  }

  alias(fn) {
    return new Alias(fn);
  }

  force(value) {
    if (value instanceof Thunk) {
      return value.force();
    } else {
      return value;
    }
  }

  list_cons(head, tail) {
    return new ListCons(head, tail);
  }

  list_empty() {
    return list_empty;
  }

  is_list_cons(x) {
    return x instanceof ListCons;
  }

  is_list_empty(x) {
    return x === list_empty;
  }

  equals(a, b) {
    return equals(a, b);
  }

  record(data) {
    data[TAMAGO_TAG] = TagRecord;

    Object.defineProperty(data, "tamago_show", {
      value() {
        const props = Object.getOwnPropertyNames(data);
        const pairs =
          props.filter(p => /^@t:/.test(p))
               .map(p => `${p.replace(/^@t:/, '')} = ${this[p]}`);
        return `{${pairs.join(", ")}}`;
      }
    });

    Object.defineProperty(data, "tamago_update", {
      value(new_record) {
        Object.setPrototypeOf(new_record, this);
        return new_record;
      }
    });

    Object.defineProperty(data, "tamago_equals", {
      value(that) {
        if (that === this) {
          return true;
        }

        return that != null
        &&     that[TAMAGO_TAG] === TagRecord
        &&     same_record(this, that);
      }
    })

    return data;
  }

  is_record(value) {
    return value != null && value[TAMAGO_TAG] === TagRecord;
  }

  is_tuple(value, arity) {
    return Array.isArray(value) && value.length === arity;
  }

  extract_record(type, value, keys) {
    return type.tamago_extract_record(value, keys);
  }

  is_effect(value) {
    return (value instanceof PerformFun)
    ||     (value instanceof PerformHandle);
  }

  run_effects(value, top_handler) {
    if (value instanceof PerformFun) {
      const frame = new EffectFrame(null, top_handler, value.coroutine());
      return run_effects(frame);
    } else if (value instanceof PerformHandle) {
      const frame = new EffectFrame(null, top_handler, function*() {
        const result = yield value;
        return result;
      }());
      return run_effects(frame);
    } else {
      throw new Error(`Not a valid effect: ${show(value)}`);
    }
  }

  get handled() {
    return EFFECT_HANDLED;
  }

  get not_handled() {
    return EFFECT_NOT_HANDLED;
  }

  effect(computation) {
    return new PerformFun(computation);
  }

  perform(value) {
    if (value instanceof Perform) {
      return value;
    } else {
      return new PerformValue(value);
    }
  }

  initialise() {
    for (const ns of this._namespaces.values()) {
      ns.load();
    }
  }

  run_tests() {
    const errors = [];
    for (const [id, ns] of this._namespaces.entries()) {
      errors.push(...ns.run_tests());
      console.log(" ");
    }
    return errors;
  } 
}

class Namespace {
  constructor(id, initialiser) {
    this._id = id;
    this._initialised = false;
    this._initialiser = initialiser;
    this._tests = [];
    this._protocol_implementations = [];
    this._default_handlers = [];
  }

  load() {
    if (!this._initialised) {
      this._initialiser(this);
      this._initialised = true;
    }
    return this;
  }

  expose(key, value) {
    if (value instanceof Thunk) {
      Object.defineProperty(this, key, {
        get() {
          return value.force();
        },
        configurable: true
      });
    } else {
      Object.defineProperty(this, key, {
        value: value,
        configurable: true
      });
    }
  }

  define_record(tag, fields) {
    return make_record(this, tag, fields);
  }

  define_union(tag, init) {
    return make_union(this, tag, init);
  }

  define_protocol(name, types, init) {
    return make_protocol(this, name, types, init);
  }

  implement_protocol(proto, types, init) {
    const impl = proto.tamago_add_implementation(types, init);
    this._protocol_implementations.push({
      protocol: proto,
      implementation: impl
    });
  }

  define_test(description, test) {
    this._tests.push(new Test(this, description, test));
  }

  define_handler(name, handler) {
    const result = new Handler(this, name, handler);
    this._default_handlers.push(result);
    return result;
  }

  handle(computation, handlers) {
    return new PerformHandle(handlers, computation);
  }

  define_module(name, init) {
    const module = new TamagoModule(this, null, name);
    return new Thunk(() => {
      init(module);
      return module;
    });
  }

  run_tests() {
    const errors = [];
    console.log(`${this._id}\n${"-".repeat(this._id.length)}`);
    if (this._tests.length === 0) {
      console.log("(No tests defined)");
    }

    for (const test of this._tests) {
      const result = test.run();
      if (result.passed) {
        console.log(cgreen(`✔️ ${test.describe()}`));
      } else {
        console.log(cred(`❌ ${test.describe()}`));
        console.log(cred(indent(4, result.error.stack)));
        console.log("---");
      }
      errors.push(test.run());
    }
    return errors;
  }

  tamago_show() {
    return `<namespace ${this._id}>`;
  }
}

class TamagoModule {
  constructor(namespace, parent, name) {
    this._namespace = namespace;
    this._name = name;
    this._parent = parent;
  }

  get id() {
    if (this._parent == null) {
      return this._name;
    } else {
      return this._parent.id + "." + this._name;
    }
  }

  expose(key, value) {
    if (value instanceof Thunk) {
      Object.defineProperty(this, key, {
        get() {
          return value.force();
        },
        configurable: true
      });
    } else {
      Object.defineProperty(this, key, {
        value: value,
        configurable: true
      });
    }    
  }

  define_record(tag, fields) {
    return this._namespace.define_record(this.id + "." + tag, fields);
  }

  define_union(tag, init) {
    return this._namespace.define_union(this.id + "." + tag, init);
  }

  define_protocol(name, types, init) {
    return this._namespace.define_protocol(this.id + "." + name, types, init);
  }

  implement_protocol(proto, types, init) {
    return this._namespace.implement_protocol(proto, types, init);
  }

  define_test(description, test) {
    return this._namespace.define_test(this.id + ": " + description, test);
  }

  define_handler(name, handler) {
    return this._namespace.define_handler(this.id + "." + name, handler);
  }

  handle(computation, handlers) {
    return this._namespace.handle(computation, handlers);
  }

  define_module(name, init) {
    const module = new TamagoModule(this._namespace, this, name);
    return new Thunk(() => {
      init(module);
      return module;
    });
  }

  tamago_show() {
    return `<module ${this.id} in ${this._namespace.tamago_show()}>`;
  }
}

class Test {
  constructor(namespace, description, test) {
    this._namespace = namespace;
    this._description = description;
    this._test = test;
  }

  run() {
    const test = this._test;
    try {
      test();
      return { passed: true, error: null };
    } catch (error) {
      return { passed: false, error: error };
    }
  }

  describe() {
    return this._description;
  }
}

class Thunk {
  force() {
    throw new Panic(`not implemented`);
  }
}

class Alias extends Thunk {
  constructor(thunk) {
    super();
    this._thunk = thunk;
  }

  force() {
    const thunk = this._thunk;
    return thunk();
  }
}

class Lazy extends Thunk {
  constructor(thunk) {
    super();
    this._thunk = thunk;
    this._value = null;
    this._forced = false;
  }

  force() {
    if (this._forced) {
      return this._value;
    } else {
      const thunk = this._thunk;
      const value = thunk();
      this._forced = true;
      this._value = value;
      return value;
    }
  }
}

function make_record(ns, tag, fields) {
  const validKeys = new Set(fields.map(f => prop(f.name)));

  function panic(msg) {
    throw new Panic(msg);
  }

  function check_keys(data) {
    for (const key of Object.getOwnPropertyNames(data)) {
      if (!validKeys.has(key)) {
        throw new Panic(`${tag} does not accept ${key}`);
      }
    }
  }

  const record = class {
    get namespace() { return ns }
    get tag() { return tag }
    get fields() { return fields }

    constructor(data) {
      check_keys(data);
      for (const field of fields) {
        const key = prop(field.name);
        const value = key in data ? data[key]
                    : !field.required ? field.init()
                    : panic(`Field ${field.name} is required`);
        Object.defineProperty(this, key, { value });
      }
    }

    static tamago_has_instance(value) {
      return value instanceof record;
    }

    tamago_show() {
      const pairs = fields.map(f => `${f.name} = ${this[prop(f.name)]}`);
      return `${tag} {${pairs.join(", ")}}`
    }

    static tamago_update(data) {
      return new record(data);
    }

    static tamago_extract_record(value, fields) {
      if (!(value instanceof record)) {
        return null;
      }

      const result = Object.create(null);
      result[TAMAGO_TAG] = TagRecord;
      for (const field of fields) {
        if (!validKeys.has(field)) {
          return null;
        } else {
          result[field] = value[field];
        }
      }
      return result;
    }

    tamago_equals(that) {
      if (that === this) {
        return true;
      }

      if (!(that instanceof record)) {
        return false;
      }

      for (const key of validKeys) {
        if (!equals(this[key], that[key])) {
          return false;
        }
      }

      return true;
    }

    tamago_update(data) {
      check_keys(data);
      Object.setPrototypeOf(data, this);
      return data;
    }
  }

  return record;
}

function make_union(ns, tag, init) {
  const variants = [];
  const union = new class {
    get namespace() { return ns }
    get tag() { tag }
    get variants() { return variants }

    tamago_has_instance(value) {
      for (const variant of variants) {
        if (variant.tamago_has_instance(value)) {
          return true;
        }
      }
      return false;
    }

    tamago_show() {
      const vnames = variants.map(x => x.tag);
      return `<union ${tag}: ${vnames.join(" | ")}>`;
    }
  }

  init({
    define_record(rtag, fields) {
      const record = make_record(ns, `${tag}.${rtag}`, fields);
      variants.push(record);
      Object.defineProperty(union, prop(rtag), {
        value: record
      });
    }
  });

  return union;
}

function make_protocol(ns, name, types, init) {
  const methods = [];
  let implementations = [];
  const protocol = new class {
    get name() { return name }
    get types() { return types }
    get methods() { return methods }
    get namespace() { return ns }

    tamago_show() {
      return `<protocol ${name}(${types.join(", ")})>`;
    }

    tamago_add_implementation(rtTypes, init) {
      if (types.length !== rtTypes.length) {
        throw new Panic(`${name} requires ${types.length} types`);
      }

      const implementation = make_implementation(this, types, rtTypes, init);
      for (const method of methods) {
        if (method.required && !implementation[prop(method.method)]) {
          throw new Panic(`Missing ${method.method} in implementation of ${name}`);
        }
      }

      implementations.push(implementation);
      return implementation;
    }

    tamago_remove_implementation(implementation) {
      implementations = implementations.filter(x => x !== implementation);
    }

    tamago_has_instance(value) {
      for (const impl of implementations) {
        for (const type of Object.values(impl.types)) {
          if (type.tamago_has_instance(value)) {
            return true;
          }
        }
      }
      return false;
    }
  }

  function make_dispatcher(method, params, default_method, required) {
    methods.push({ method, params, default: default_method, required });
    const dispatch = types.map((t) => {
      return {
        type: t,
        index: params.indexOf(t)
      }
    }).filter(t => t.index !== -1);
    if (dispatch.length === 0) {
      throw new Panic(`No dispatch parameter in ${name}.${method}`);
    }
    Object.defineProperty(protocol, prop(method), {
      value: function(...args) {
        if (args.length !== params.length) {
          throw new Panic(`${name}.${method} expects ${params.length} arguments`);
        }

        select:
        for (const impl of implementations) {
          for (const d of dispatch) {
            if (!impl.types[d.type].tamago_has_instance(args[d.index])) {
              continue select;
            }
          }
          return impl[prop(method)](...args);
        }
        return default_method(...args);
      }
    });

    return protocol[prop(method)];
  }

  init({
    required_method(method, params) {
      return make_dispatcher(method, params, function(...args) {
        throw new Panic(`No implementation of ${name}.${method} for ${show(args)}`);
      }, true)
    },

    optional_method(method, params, default_method) {
      return make_dispatcher(method, params, default_method, false);
    },
  
    requires(proto, types) {
      // no-op for now
    }
  });

  return protocol;
}

function make_implementation(protocol, types, rtTypes, init) {
  const typeMap = Object.create(null);
  types.forEach((t, i) => {
    typeMap[t] = rtTypes[i];
  });

  const implementation = new class {
    get types() { return typeMap }
  }

  init({
    implement(name, fn) {
      Object.defineProperty(implementation, prop(name), { value: fn });
    }
  });

  return implementation;
}

class Perform {}

class PerformValue extends Perform {
  constructor(effect) {
    super();
    this.effect = effect;
  }
}

class PerformFun extends Perform {
  constructor(coroutine) {
    super();
    this.coroutine = coroutine;
  }
}

class PerformHandle extends Perform {
  constructor(handler, coroutine) {
    super();
    this.handler = handler;
    this.coroutine = coroutine;
  }
}

class EffectFrame {
  constructor(parent, handlers, coroutine) {
    this.parent = parent;
    this.handlers = handlers;
    this.coroutine = coroutine;
  }

  send(value) {
    return this.coroutine.next(value);
  }
}

class Handler {
  constructor(ns, name, handler) {
    this._namespace = ns;
    this._name = name;
    this._handler = handler;
  }

  handle(value, continuation) {
    const handler = this._handler;
    handler(value, continuation);
  }
}

function defer() {
  const result = {};
  const promise = new Promise((resolve, reject) => {
    result.resolve = resolve;
    result.reject = reject;
  });
  result.promise = promise;
  return result;
}

function run_effects(initialFrame) {
  let rejected = false;
  let frame = initialFrame;
  let promise = defer();

  function returnValue(value) {
    if (frame.parent == null) {
      promise.resolve(value);
    } else {
      frame = frame.parent;
      next(value);
    }
  }

  function send(value) {
    return frame.send(value);
  }

  function next(initialValue) {
    if (rejected) {
      throw new Panic(`Effect runner failed`);
    }

    let value = initialValue;
    let nextState = { done: true, value: null };
    try {
      while (true) {
        nextState = send(value);
        if (nextState.done) {
          returnValue(nextState.value);
          break;
        } else {
          const status = handle(nextState.value, once(next), fail);
          if (status.continue) {
            value = status.value;
          } else {
            break; // continuation was captured
          }
        }
      }
    } catch (error) {
      fail(error);
    }
  }

  function handle(effect, next, fail) {
    if (effect instanceof PerformValue) {
      return handleValue(effect.effect, next, fail);
    } else if (effect instanceof PerformFun) {
      const coroutine = effect.coroutine;
      frame = new EffectFrame(frame, frame.handlers, coroutine());
      return { continue: true, value: undefined };
    } else if (effect instanceof PerformHandle) {
      const coroutine = effect.coroutine;
      frame = new EffectFrame(frame, effect.handler, coroutine());
      return { continue: true, value: undefined };
    } else {
      reject(new Panic(`Not a valid perform value`));
    }
  }

  function handleValue(effect, next, fail) {
    const STOP = false;
    const CONTINUE = true;

    let sync = false;
    let current = frame;
    let returned = false;
    let result = null;
    let handleResult = CONTINUE;

    while (true) {
      sync = false;
      handleResult = STOP;
      current.handlers(effect, (handled, value) => {
        sync = true;
        if (handled === EFFECT_HANDLED) {
          if (returned) {
            handleResult = STOP;
            continuation(value);
          } else {
            handleResult = STOP;
            result = value;
          }
        } else {
          if (current.parent == null) {
            handleResult = STOP;
            fail(effect);
          } else {
            if (returned) {
              handleResult = STOP;
              handleValueAsync(current.parent, effect, next, fail);
            } else {
              handleResult = CONTINUE;
              current = current.parent;
            }
          }
        }
      });
      if (handleResult === STOP) {
        break;
      }
    }
    returned = true;
    return { continue: sync, value: result };
  }

  function handleValueAsync(frame, effect, next, fail) {
    frame.handlers(effect, (handled, value) => {
      if (handled === EFFECT_HANDLED) {
        next(value);
      } else {
        if (frame.parent == null) {
          fail(effect);
        } else {
          handleValueAsync(frame.parent, effect, next, fail);
        }
      }
    });
  }

  function fail(reason) {
    rejected = true;
    promise.reject(reason);
  }

  function once(fn) {
    let called = false;
    return (value) => {
      if (called) {
        throw new Panic(`Continuations are one-shot`);
      }
      if (rejected) {
        throw new Panic(`Effect runner failed`);
      }
      return fn(value);
    }
  }

  next(undefined);
  return promise.promise;
}

class ListCons {
  constructor(head, tail) {
    this.head = head;
    this.tail = tail;
  }
}

const list_empty = new class ListEmpty {};

module.exports = {
  Tamago,
  Namespace,
  Panic,
  Test,
  Thunk,
  Lazy,
  Alias,
  show,
  prop,
  EffectFrame,
  Perform,
  PerformFun,
  PerformHandle,
  PerformValue,
  run_effects,
  Handler,
  ListCons,
  list_empty
};