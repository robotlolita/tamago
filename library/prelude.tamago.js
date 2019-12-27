const $rt = Tamago.make_runtime(__dirname, __filename, require, module);
const $pattern = $rt.pattern;

$rt.define_module("tamago.prelude", function _($self) {
  const _P = $rt.import_external("./prelude.js");
  const _Float = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Float",
      $rt.force(_P).$project("isFloat")
    );
  });
  $self.expose("Float", _Float);
  const _Integer = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Integer",
      $rt.force(_P).$project("isInteger")
    );
  });
  $self.expose("Integer", _Integer);
  const _Text = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Text",
      $rt.force(_P).$project("isString")
    );
  });
  $self.expose("Text", _Text);
  const _Nothing = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Nothing",
      $rt.force(_P).$project("isNothing")
    );
  });
  $self.expose("Nothing", _Nothing);
  const _Record = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Record",
      $rt.force(_P).$project("isRecord")
    );
  });
  $self.expose("Record", _Record);
  const _Function = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Function",
      $rt.force(_P).$project("isFunction")
    );
  });
  $self.expose("Function", _Function);
  const _Symbol = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Symbol",
      $rt.force(_P).$project("isSymbol")
    );
  });
  $self.expose("Symbol", _Symbol);
  const _Tuple = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Tuple",
      $rt.force(_P).$project("isTuple")
    );
  });
  $self.expose("Tuple", _Tuple);
  const _Object = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Object",
      $rt.check_arity(0.0, function _() {
        return $eq($rt.force(_P).$project("typeOf")(_x), "object");
      })
    );
  });
  $self.expose("Object", _Object);
  const _Any = $rt.thunk(function _() {
    return $rt.force(_P).$project("makeType")(
      "Any",
      $rt.check_arity(0.0, function _() {
        return true;
      })
    );
  });
  $self.expose("Any", _Any);
  const $eq = function $eq(_a, _b) {
    return $rt.match(
      [_a, _b],
      [
        [
          $pattern.tuple([$pattern.empty(), $pattern.empty()]),
          function _({}) {
            return true;
          },
          function _({}) {
            return true;
          }
        ],
        [
          $pattern.tuple([
            $pattern.cons($pattern.bind("x"), $pattern.bind("xs")),
            $pattern.cons($pattern.bind("y"), $pattern.bind("ys"))
          ]),
          function _({ ["x"]: _x, ["xs"]: _xs, ["y"]: _y, ["ys"]: _ys }) {
            return true;
          },
          function _({ ["x"]: _x, ["xs"]: _xs, ["y"]: _y, ["ys"]: _ys }) {
            return $and($eq(_x, _y), $eq(_xs, _ys));
          }
        ],
        [
          $pattern.tuple([$pattern.bind("$_1"), $pattern.bind("$_2")]),
          function _({}) {
            return true;
          },
          function _({}) {
            return $rt.force(_P).$project("opEq")(_a, _b);
          }
        ]
      ]
    );
  };
  $self.expose("===", $eq);
  const $neq = function $neq(_a, _b) {
    return $rt.match(
      [_a, _b],
      [
        [
          $pattern.tuple([
            $pattern.check($pattern.bind("$_3"), $rt.force(_Float)),
            $pattern.check($pattern.bind("$_4"), $rt.force(_Float))
          ]),
          function _({}) {
            return true;
          },
          function _({}) {
            return $rt.force(_P).$project("opNeq")(_a, _b);
          }
        ],
        [
          $pattern.tuple([$pattern.bind("$_5"), $pattern.bind("$_6")]),
          function _({}) {
            return true;
          },
          function _({}) {
            return $not($eq(_a, _b));
          }
        ]
      ]
    );
  };
  $self.expose("=/=", $neq);
  const $gte = function $gte(_a, _b) {
    return $rt.force(_P).$project("opGte")(_a, _b);
  };
  $self.expose(">=", $gte);
  const $gt = function $gt(_a, _b) {
    return $rt.force(_P).$project("opGt")(_a, _b);
  };
  $self.expose(">", $gt);
  const $lte = function $lte(_a, _b) {
    return $rt.force(_P).$project("opLte")(_a, _b);
  };
  $self.expose("<=", $lte);
  const $lt = function $lt(_a, _b) {
    return $rt.force(_P).$project("opLt")(_a, _b);
  };
  $self.expose("<", $lt);
  const $plus = function $plus(_a, _b) {
    return $rt.force(_P).$project("opPlus")(_a, _b);
  };
  $self.expose("+", $plus);
  const $minus = function $minus(_a, _b) {
    return $rt.force(_P).$project("opMinus")(_a, _b);
  };
  $self.expose("-", $minus);
  const $divide = function $divide(_a, _b) {
    return $rt.force(_P).$project("opDivide")(_a, _b);
  };
  $self.expose("/", $divide);
  const $times = function $times(_a, _b) {
    return $rt.force(_P).$project("opTimes")(_a, _b);
  };
  $self.expose("*", $times);
  const $and = function $and(_a, _b) {
    return $rt.force(_P).$project("opAnd")(_a, _b);
  };
  $self.expose("and", $and);
  const $or = function $or(_a, _b) {
    return $rt.force(_P).$project("opOr")(_a, _b);
  };
  $self.expose("or", $or);
  const $not = function $not(_a) {
    return $rt.force(_P).$project("opNot")(_a);
  };
  $self.expose("not", $not);
  const _to_float = function _to_float(_self) {
    return $rt.match(_self, [
      [
        $pattern.check($pattern.bind("$_7"), $rt.force(_Float)),
        function _({}) {
          return true;
        },
        function _({}) {
          return _self;
        }
      ],
      [
        $pattern.check($pattern.bind("$_8"), $rt.force(_Integer)),
        function _({}) {
          return true;
        },
        function _({}) {
          return $rt.force(_P).$project("integerToFloat")(_self);
        }
      ]
    ]);
  };
  $self.expose("to-float", _to_float);
  const _symbol$ = function _symbol$(_description) {
    return $rt.force(_P).$project("makeSymbol")(_description);
  };
  $self.expose("symbol:", _symbol$);
  const __$at$ = function __$at$(_tuple, _index) {
    return $rt.force(_P).$project("tupleAt")(_tuple, _index);
  };
  $self.expose("_:at:", __$at$);
  const _size = function _size(_tuple) {
    return $rt.force(_P).$project("tupleSize")(_tuple);
  };
  $self.expose("size", _size);
  const _first = function _first(_tuple) {
    return __$at$(_tuple, 1.0);
  };
  $self.expose("first", _first);
  const _second = function _second(_tuple) {
    return __$at$(_tuple, 2.0);
  };
  $self.expose("second", _second);
  const _last = function _last(_tuple) {
    return __$at$(_tuple, _size(_tuple));
  };
  $self.expose("last", _last);
  const _head = function _head(_list) {
    return $rt.match(_list, [
      [
        $pattern.cons($pattern.bind("x"), $pattern.bind("$_9")),
        function _({ ["x"]: _x }) {
          return true;
        },
        function _({ ["x"]: _x }) {
          return _x;
        }
      ]
    ]);
  };
  $self.expose("head", _head);
  const _tail = function _tail(_list) {
    return $rt.match(_list, [
      [
        $pattern.cons($pattern.bind("$_10"), $pattern.bind("xs")),
        function _({ ["xs"]: _xs }) {
          return true;
        },
        function _({ ["xs"]: _xs }) {
          return _xs;
        }
      ],
      [
        $pattern.empty(),
        function _({}) {
          return true;
        },
        function _({}) {
          return $rt.empty();
        }
      ]
    ]);
  };
  $self.expose("tail", _tail);
  const __$at$ = function __$at$(_list, _index) {
    return $rt.match(_list, [
      [
        $pattern.cons($pattern.bind("x"), $pattern.bind("$_11")),
        function _({ ["x"]: _x }) {
          return $eq(_index, 1n);
        },
        function _({ ["x"]: _x }) {
          return _x;
        }
      ],
      [
        $pattern.cons($pattern.bind("$_12"), $pattern.bind("xs")),
        function _({ ["xs"]: _xs }) {
          return true;
        },
        function _({ ["xs"]: _xs }) {
          return __$at$(_xs, $minus(_index, 1n));
        }
      ]
    ]);
  };
  $self.expose("_:at:", __$at$);
  const __$zip$with$ = function __$zip$with$(_left, _right, _combine) {
    return $rt.match(
      [_left, _right],
      [
        [
          $pattern.tuple([
            $pattern.cons($pattern.bind("x"), $pattern.bind("xs")),
            $pattern.cons($pattern.bind("y"), $pattern.bind("ys"))
          ]),
          function _({ ["x"]: _x, ["xs"]: _xs, ["y"]: _y, ["ys"]: _ys }) {
            return true;
          },
          function _({ ["x"]: _x, ["xs"]: _xs, ["y"]: _y, ["ys"]: _ys }) {
            return $rt.cons(_combine(_x, _y), __$zip$with$(_xs, _ys, _combine));
          }
        ],
        [
          $pattern.tuple([$pattern.empty(), $pattern.empty()]),
          function _({}) {
            return true;
          },
          function _({}) {
            return $rt.empty();
          }
        ]
      ]
    );
  };
  $self.expose("_:zip:with:", __$zip$with$);
});
