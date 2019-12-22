const { assert } = require('./assert');
const define = Object.defineProperty;

class TamagoRecord {
  constructor(module, name, props) {
    this._name = name;
    this._module = module;
    this._properties = new Set(props);
  }

  $project(field) {
    assert(this._properties.has(field), `Undefined label ${field} in ${this}`);
    return (value) => {
      assert(value instanceof TamagoRecordInstance && value._record === this, `Not a ${this} instance`);
      return value._fields[field];
    }
  }

  $update(fields) {
    const givenKeys = Object.keys(fields);
    assert(givenKeys.length === this._properties.size, `Missing properties when constructing ${this}`);
    for (const key of givenKeys) {
      assert(this._properties.has(key), `Invalid label ${key} for ${this}`);
    }
    return new TamagoRecordInstance(this, fields);
  }

  $match(value, keys) {
    if (!(value instanceof TamagoRecordInstance)) {
      return null;
    }
    if (value._record != this) {
      return null;
    }

    const result = [];
    for (const key of keys) {
      assert(this._properties.has(key), `Invalid label ${key} in ${this}`);
      result.push(value._fields[key]);
    }
    return result;
  }

  toString() {
    return `TamagoRecord<${this._name} in ${this._module}>`;
  }
}

class TamagoRecordInstance {
  constructor(record, fields) {
    this._record = record;
    this._fields = fields;
  }

  $update(fields) {
    const newFields = Object.create(this._fields);
    for (const [key, value] of fields) {
      define(newFields, key, { value });
    }
    return new TamagoRecordInstance(this._record, newFields);
  }

  toString() {
    return `instance ${this._record}`;
  }
}

class TamagoUnion {
  constructor(module, name, cases) {
    this._module = module;
    this._name = name;
    this._cases = new Map();
    for (const [tag, record] of cases) {
      this._cases.set(tag, record);
    }
  }

  $project(name) {
    const record = this._cases.get(name);
    assert(record !== undefined, `No variant ${name} in ${this}`);
    return record;
  }

  toString() {
    return `TamagoUnion<${this._name} in ${this._module}>`;
  }
}

class TamagoAnonymousRecord {
  constructor(fields) {
    this._fields = Object.freeze(fields);
  }

  $project(name) {
    const value = this._fields[name];
    assert(value !== undefined, `Undefined label ${name}`);
    return value;
  }

  $update(fields) {
    const newFields = Object.create(this._fields);
    for (const [key, value] of Object.entries(fields)) {
      Object.defineProperty(newFields, key, { value });
    }
    return new TamagoAnonymousRecord(newFields);
  }

  toString() {
    return `{ ... }`;
  }
}

TamagoAnonymousRecord.$match = function(value) {
  if (!(value instanceof TamagoAnonymousRecord)) {
    return null;
  }

  const result = [];
  for (const key of keys) {
    assert(key in value._fields, `Invalid label ${key} in ${this}`);
    result.push(value._fields[key]);
  }
  return result;
};

function define_record(module, name, props) {
  return new TamagoRecord(module, name, props);
}

function define_variant(module, tag, props) {
  return [tag, new TamagoRecord(module, tag, props)];
}

function define_union(module, name, cases) {
  return new TamagoUnion(module, name, cases);
}

module.exports = {
  TamagoRecord,
  TamagoUnion,
  TamagoAnonymousRecord,
  define_record,
  define_union,
  define_variant
};