const $rt = Tamago.make_runtime(__dirname, __filename, require, module);
const $pattern = $rt.pattern;
const $gte = $rt.builtin.gte;
const $gt = $rt.builtin.gt;
const $lte = $rt.builtin.lte;
const $lt = $rt.builtin.lt;
const $eq = $rt.builtin.eq;
const $neq = $rt.builtin.neq;
const $composer = $rt.builtin.composer;
const $composel = $rt.builtin.composel;
const $concat = $rt.builtin.concat;
const $plus = $rt.builtin.plus;
const $minus = $rt.builtin.minus;
const $times = $rt.builtin.times;
const $divide = $rt.builtin.divide;
const $and = $rt.builtin.and;
const $or = $rt.builtin.or;
const $not = $rt.builtin.not;

$rt.define_module("tamago.examples.fibonacci", function _($self) {
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
          return $plus(_fib($minus(_n, 1n)), _fib($minus(_n, 2n)));
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
