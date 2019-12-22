const { assert } = require('./assert');
const { Cons, empty } = require('./list');
const { TamagoRecord, TamagoAnonymousRecord } = require('./record');

function match(value, cases) {
  for (const [pattern, guard, body] of cases) {
    const binds = pattern(value);
    if (binds == null || guard(binds)) {
      continue;
    }
    return body(binds);
  }
  throw new Error(`Unmatched ${value}`);
}

function join_binds(binds) {
  return binds.reduce(
    (a, b) => {
      if (a == null || b == null) {
        return null;
      } else {
        return { ...a, ...b };
      }
    },
    {}
  );
}

const patterns = new class Patterns {
  bind(name) {
    return value => ({ [name]: value });
  }

  bind_as(pattern, name) {
    return value => join_binds(pattern(value), { [name]: value });
  }

  check(pattern, type) {
    return value => {
      if (type.$has_instance(value)) {
        return pattern(value)
      } else {
        return null;
      }
    }
  }

  equal(literal) {
    return value => {
      if (value === literal) {
        return {};
      } else {
        return null;
      }
    }
  }

  cons(head, tail) {
    return value => {
      if (value instanceof Cons) {
        return join_binds([
          head(value._value),
          tail(value._rest)
        ])
      } else {
        return null;
      }
    }
  }

  empty() {
    return this.equal(empty);
  }

  tuple(patterns) {
    return value => {
      if (Array.isArray(value) && value.length === patterns.length) {
        return join_binds(
          patterns.map((p, i) => p(value[i]))
        );
      } else {
        return null;
      }
    }
  }

  record(keys, patterns) {
    return value => {
      const values = TamagoAnonymousRecord.$match(value, keys);
      if (values != null) {
        return this.tuple(patterns)(values);
      } else {
        return null;
      }
    }
  }

  extractor(record, keys, patterns) {
    assert(record instanceof TamagoRecord, `Expected a record view`);
    return value => {
      const values = record.$match(value, keys);
      if (values != null) {
        return this.tuple(patterns)(values);
      } else {
        return null;
      }
    }
  }
}


module.exports = {
  match,
  patterns
};