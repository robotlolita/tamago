const $rt = Tamago.make_runtime(__dirname, __filename, require, module);
const $pattern = $rt.pattern;

$rt.define_module("tamago.examples.fibonacci", function _($self) {
  const _Prelude = $rt.thunk(function _() {
    return $rt.import_module("tamago.prelude");
  });
  $self.open($rt.force(_Prelude));
  const _fib = function _fib(_n) {
    return $rt.match(_n, [
      [
        $pattern.equal(0n),
        function _({}) {
          return true;
        },
        function _({}) {
          return 0n;
        }
      ],
      [
        $pattern.equal(1n),
        function _({}) {
          return true;
        },
        function _({}) {
          return 1n;
        }
      ],
      [
        $pattern.bind("n"),
        function _({ ["n"]: _n }) {
          return true;
        },
        function _({ ["n"]: _n }) {
          return $self.$project_scoped("+")(
            _fib($self.$project_scoped("-")(_n, 1n)),
            _fib($self.$project_scoped("-")(_n, 2n))
          );
        }
      ]
    ]);
  };
  $self.expose("fib", _fib);
  $self.define_test("fib", function _() {
    return (
      $rt.assert_match(_fib(0n), 0n),
      ($rt.assert_match(_fib(1n), 1n),
      ($rt.assert_match(_fib(2n), 1n),
      ($rt.assert_match(_fib(3n), 2n),
      ($rt.assert_match(_fib(4n), 3n),
      ($rt.assert_match(_fib(5n), 5n),
      ($rt.assert_match(_fib(6n), 8n),
      ($rt.assert_match(_fib(7n), 13n),
      ($rt.assert_match(_fib(8n), 21n),
      ($rt.assert_match(_fib(9n), 34n),
      $rt.assert_match(_fib(10n), 55n))))))))))
    );
  });
  const _main$ = function _main$(_$_1) {
    return _fib(10n);
  };
  $self.expose("main:", _main$);
});
