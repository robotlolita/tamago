function prop(name) {
  return `@t:${name}`;
}

class Panic extends Error {
  get name() { return "TamagoPanic" }
}

function show(value) {
  if (value === null) {
    return "nothing";
  } else if (typeof value === "function") {
    return `<lambda ${value.name || '(anonymous)'}/${value.length}>`;
  } else if (Array.isArray(value)) {
    return `[${value.map(show).join(", ")}]`;
  } else if (typeof value === "string") {
    return JSON.stringify(value);
  } else if (typeof value === "bigint") {
    return value.toLocaleString();
  } else if (typeof value === "number") {
    if (value === Math.floor(value)) {
      return value.toFixed(0);
    } else {
      return String(value);
    }
  } else if (typeof value === "boolean") {
    return String(value);
  } else if (typeof value === "symbol") {
    return `<symbol: ${value}>`;
  } else if (value && value.tamago_show) {
    return value.tamago_show();
  } else {
    return `<host-value>`;
  } 
}

function equals(a, b) {
  if (a === null && b === null) {
    return a === b;
  } else if (Array.isArray(a) && Array.isArray(b)) {
    return a.length === b.length && a.every((x, i) => equals(x, b[i]));
  } else if (typeof a === "object" && typeof a.tamago_equals === "function") {
    return a.tamago_equals(b);
  } else {
    return a === b;
  }
}

function make_module(name, obj) {
  const result = Object.create(null);
  for (const key of Object.getOwnPropertyNames(obj)) {
    const descriptor = Object.getOwnPropertyDescriptor(obj, key);
    Object.defineProperty(result, prop(key), {
      ...descriptor,
      writable: false,
      configurable: false,
      enumerable: false
    });
  }
  return {
    _id: name,
    _tests: [],
    load() {
      return result;
    }
  }
}

const unsafeInteger = make_module("tamago::native::unsafe::integer", {
  add(a, b) {
    return a + b;
  },
  sub(a, b) {
    return a - b;
  },
  mul(a, b) {
    return a * b;
  },
  div(a, b) {
    if (b === 0n) {
      throw new Panic(`internal: division by 0`);
    }
    return a / b;
  },
  pow(a, b) {
    return a ** b;
  },
  negate(a) {
    return -a;
  },
  gte(a, b) {
    return a >= b;
  },
  gt(a, b) {
    return a > b;
  },
  lte(a, b) {
    return a <= b;
  },
  lt(a, b) {
    return a < b;
  },
  eq(a, b) {
    return a === b;
  },
  "not-eq"(a, b) {
    return a !== b;
  },
  "bit-not"(a) {
    return ~a;
  },
  "bit-and"(a, b) {
    return a & b;
  },
  "bit-or"(a, b) {
    return a | b;
  },
  "bit-xor"(a, b) {
    return a ^ b;
  },
  "bit-shr"(a, b) {
    return a >> b;
  },
  "bit-ushr"(a, b) {
    return a >>> b;
  },
  "bit-shl"(a, b) {
    return a << b;
  },
  "to-float"(a) {
    return Number(a);
  }
});

const unsafeFloat = make_module("tamago::native::unsafe::float64", {
  add(a, b) {
    return a + b;
  },
  sub(a, b) {
    return a - b;
  },
  mul(a, b) {
    return a * b;
  },
  div(a, b) {
    return a / b;
  },
  pow(a, b) {
    return a ** b;
  },
  negate(a) {
    return -a;
  },
  gte(a, b) {
    return a >= b;
  },
  gt(a, b) {
    return a > b;
  },
  lte(a, b) {
    return a <= b;
  },
  lt(a, b) {
    return a < b;
  },
  eq(a, b) {
    return a === b;
  },
  "not-eq"(a, b) {
    return a !== b;
  },
  "bit-not"(a) {
    return ~a;
  },
  "bit-and"(a, b) {
    return a & b;
  },
  "bit-or"(a, b) {
    return a | b;
  },
  "bit-xor"(a, b) {
    return a ^ b;
  },
  "bit-shr"(a, b) {
    return a >> b;
  },
  "bit-ushr"(a, b) {
    return a >>> b;
  },
  "bit-shl"(a, b) {
    return a << b;
  }
});

const unsafeBool = make_module("tamago::native::unsafe::boolean", {
  and(a, b) {
    return a && b;
  },
  or(a, b) {
    return a || b;
  },
  not(a) {
    return !a;
  }
});

const unsafeText = make_module("tamago::native::unsafe::text", {
  concat(a, b) {
    return a + b;
  },
  eq(a, b) {
    return a === b;
  }
});

const unsafeTuple = make_module("tamago::native::unsafe::tuple", {
  size(a) {
    return BigInt(a.length);
  },
  at(a, index) {
    return a[Number(index)];
  },
  concat(a, b) {
    return a.concat(b);
  },
  slice(a, start, end) {
    return a.slice(Number(start), Number(end));
  }
});

const unsafeType = make_module("tamago::native::unsafe::type", {
  Integer: {
    tamago_has_instance(value) {
      return typeof value === "bigint";
    },
    tamago_show() {
      return `<type: Integer>`;
    }
  },

  Float64: {
    tamago_has_instance(value) {
      return typeof value === "number";
    }
  },

  Boolean: {
    tamago_has_instance(value) {
      return typeof value === "boolean";
    }
  },

  Symbol: {
    tamago_has_instance(value) {
      return typeof value === "symbol";
    }
  },

  Text: {
    tamago_has_instance(value) {
      return typeof value === "string";
    },
    tamago_show() {
      return `<type: Text>`;
    }
  },

  Tuple: {
    tamago_has_instance(value) {
      return Array.isArray(value);
    }
  },

  Lambda: {
    tamago_has_instance(value) {
      return typeof value === "function";
    }
  },

  Nothing: {
    tamago_has_instance(value) {
      return value === null;
    }
  },

  Any: {
    tamago_has_instance(value) {
      return value !== undefined;
    }
  }
});

const unsafeEquality = make_module("tamago::native::unsafe::equality", {
  eq(a, b) {
    return a === b;
  },
  "not-eq"(a, b) {
    return a !== b;
  }
});

const modules = [
  unsafeInteger,
  unsafeFloat,
  unsafeBool,
  unsafeText,
  unsafeTuple,
  unsafeType,
  unsafeEquality
];

module.exports = {
  modules,
  load_natives(runtime) {
    for (m of modules) {
      runtime._namespaces.set(m._id, m);
    }
  },
  prop,
  Panic,
  show,
  equals
};