const { Cons, empty } = require('../Tamago/runtime/list');
const { TamagoRecordInstance, TamagoAnonymousRecord } = require('../Tamago/runtime/record');

class Prelude {
  opGte(a, b) {
    return a >= b;
  }

  opGt(a, b) {
    return a > b;
  }

  opLte(a, b) {
    return a <= b;
  }

  opLt(a, b) {
    return a < b;
  }

  opEq(a, b) {
    return a === b;
  }

  opNeq(a, b) {
    return a !== b;
  }

  opPlus(a, b) {
    return a + b;
  }

  opTimes(a, b) {
    return a * b;
  }

  opMinus(a, b) {
    return a - b;
  }

  opDivide(a, b) {
    return a / b;
  }

  opAnd(a, b) {
    return a && b;
  }

  opOr(a, b) {
    return a || b;
  }

  opNot(a) {
    return !a;
  }

  opRemainder(a, b) {
    return a % b;
  }

  opPower(a, b) {
    return a ** b;
  }

  opBitAnd(a, b) {
    return a & b;
  }

  opBitOr(a, b) {
    return a | b;
  }

  opBitXor(a, b) {
    return a ^ b;
  }

  opBitNot(a) {
    return ~a;
  }

  opLeftShift(a, b) {
    return a << b;
  }

  opRightShift(a, b) {
    return a >> b;
  }

  opZeroFillRightShift(a, b) {
    return a >>> b;
  }

  typeOf(a) {
    switch (typeof a) {
      case "bigint": return "integer";
      case "boolean": return "boolean";
      case "function": return "function";
      case "number": return "float";
      case "string": return "text";
      case "symbol": return "symbol";
      case "undefined": return "nothing";
      case "object": {
        if (a === null) {
          return "nothing";
        }
        if (Array.isArray(a)) {
          return "tuple";
        }
        if (a instanceof TamagoAnonymousRecord || a instanceof TamagoRecordInstance) {
          return "record";
        }
        if (a instanceof Cons || a === empty) {
          return "list";
        }
        return "object";
      }
    }
  }

  isFloat(a) {
    return typeof a === "number";
  }

  isInteger(a) {
    return typeof a === "bigint";
  }

  isBoolean(a) {
    return typeof a === "boolean";
  }

  isFunction(a) {
    return typeof a === "function";
  }

  isString(a) {
    return typeof a === "string";
  }

  isSymbol(a) {
    return typeof a === "symbol";
  }

  isTuple(a) {
    return Array.isArray(a);
  }

  isRecord(a) {
    return a instanceof TamagoRecordInstance || a instanceof TamagoAnonymousRecord;
  }

  isList(a) {
    return a instanceof Cons || a === empty;
  }

  isNothing(a) {
    return a == null;
  }

  integerToFloat(a) {
    return Number(a);
  }

  floatToInteger(a) {
    try {
      return BigInt(a);
    } catch (e) {
      return null;
    }
  }

  tupleAt(tuple, index) {
    if (index < 1 || index > tuple.length) {
      throw new RangeError("Index out of range: " + index);
    }
    return tuple[index - 1];
  }

  listFold(list, init, fn) {
    let node = list;
    let result = init;
    while (node !== empty) {
      if (!(node instanceof Cons)) {
        throw new Error("Invalid list");
      }
      result = fn(result, node._value);
      node = node._rest;
    }
    return result;
  }

  tupleSize(tuple) {
    return tuple.length;
  }

  makeType(name, check) {
    return {
      name,
      $has_instance: check
    };
  }

  $project(field) {
    if (Prelude.prototype.hasOwnProperty(field)) {
      return Prelude.prototype[field];
    } else {
      throw new Error("No such property " + field);
    }
  }
}

module.exports = Prelude.prototype;