# Lambda Tamago

The Lambda layer provides a basis for computation through functional
programming concepts: inductive data structures, recursive processes,
and pattern matching.

Programs written in Lambda Tamago are pure, but not necessarily total.
There are also no bounds on resource usage. Because of this it is possible
for a program's termination to be non-deterministic---if the system cannot
always provide the needed resources for running the program, it will diverge
some times. However, while programs may diverge, no terminating program will
compute *different values* when given the same input.


## Concepts

Lambda Tamago is comprised of few core concepts:

  - Lambda abstractions
  - Function application
  - Records, lists, and primitive values
  - Pattern matching
  - Binding definition

Although the surface syntax includes convenience structures for some common
operations that is derived from the five concepts above.


### Primitives

Lambda Tamago introduces a small set of primitive values that are common in
functional languages:

  - **Arbitrary-precision integer**: `1_000_000_000_000_000_000_000_000`;
  - **64-bit IEEE-754 floats**: `1_000.000_001`;
  - **Unicode text**: `"私はOKです 😊"`;
  - **Logical/boolean**: `true`, `false`;
  - **Special unit/null value**: `nothing`;


### Tuples

Tuples are immutable, fixed sequences of values. Tamago has 

### Lambda abstractions and application




### Bindings

A binding is an association between a name and an expression. Such associations
are created by the `define` construct. In its simplest form, `define` looks
like the following:

```
define one = 1;
```

In this example the name `one` and the value `1` may be used interchangeably
in any code where `one` is valid. Bindings are lexically scoped, which means
that the portions of code in which the name has a particular meaning are
defined entirely by the structure of the code, and are independent of how
the program executes at runtime.

#### Evaluation semantics of bindings

It's important to note that no evaluation of the expression happens during
a `define`. The only thing `define` does is to express the equivalence between
the given name, and the expression on the right of the equals. While the
distinction isn't important with pure programs, it becomes more relevant when
effects and modules are added on top of Lambda Tamago. And it's also relevant
when discussing runtime costs.

For example, given a program such as:

```
define two = 1 + 1;
define four = two + two;
```

A `define` construct that associates names with values would have to execute
the expression on the right. This guarantees that `1 + 1` is only evaluated
once, giving us the value `2`. That is then what is used by the `four`
definition.

In essence, we use the following reduction rules:

```
define two = 1 + 1;
define four = two + two;
--------------------------------------- (1) simplifying `1 + 1`
define two = 2;
define four = two + two;
--------------------------------------- (2) substituting `two`
define two = 2;
define four = 2 + 2;
--------------------------------------- (3) simplifying `2 + 2`
define two = 2;
define four = 4;
```

In a language that associates names with expressions, instead, we only start
simplifying from the entry-point, not from the order in which expressions
appear in the source code. That is, such a language gives rise to the following
reduction semantics when `four` is evaluated:

```
define two = 1 + 1;
define four = two + two;
--------------------------------------- (1) substituting `two`
define two = 1 + 1;
define four = (1 + 1) + (1 + 1);
--------------------------------------- (2) simplify
define two = 1 + 1;
define four = 2 + (1 + 1);
--------------------------------------- (3) simplify
define two = 1 + 1;
define four = 2 + 2;
--------------------------------------- (4) simplify
define two = 1 + 1;
define four = 4;
```

The entire evaluation now takes 4 steps, rather than 3. And we are forced
to evaluate `1 + 1` twice, even though we know it's going to give us the
exact same result. As an optimisation (and this becomes more important once
we layer effects on top), Lambda Tamago makes bindings **lazy**. That is,
while evaluation is deferred, it is also performed *at most* once. This gives
rise to the following reduction semantics when evaluating `four`:

```
define two = <1 + 1>@1;
define four = <two + two>@2;
--------------------------------------- (1) substituting `two`
define two = <1 + 1>@1;
define four = <<1 + 1>@1 + <1 + 1>@1>@2;
--------------------------------------- (2) simplifying the graph node `1`
define two = <2>@1;
define four = <<2>@1 + <2>@1>@2;
--------------------------------------- (3) simplifying the graph node `2`
define two = <2>@1;
define four = <4>@2;
```

This optimisation gives us 3 evaluation steps again, but it introduces a notion
of state (albeit a runtime-controlled one, rather than user-controlled), where
every node is tagged, and its evaluation state is saved to be reused in the
future. In the abstract semantics described in this document, we introduce an
explicit notion of *delaying* and *forcing* computations, but we'll ignore the
syntactic overhead when explaining the concepts here.


#### Function bindings

Besides associating simple names with an expression, bindings also associate
names to functions. Because functions are always deferred, the optimisation
above does not take place, and the function definition is eagerly evaluated
to a closure---a function definition with its accompanying environment record,
which contains the bindings that are visible in that function.

Functions in Lambda Tamago may be one of the following kind:

  - An unary function, written in postfix notation e.g.: `_ successor`;
  - An unary function, written in prefix notation e.g.: `not _`;
  - A binary function, written in infix notation e.g.: `_ + _`;
  - A N-ary function, written in mixfix notation e.g.: `_ between: _ and: _`;
  - A N-ary function, written in prefix notation e.g.: `max(_, _)`;

Functions always have a fixed arity, and while the notation may change, they
always have the same semantics. That is, a function defined as `max(_, _)`
will work in the same way as one defined as `max-between: _ and: _`, if they
have the same body, but the way you refer to those functions by name differs.

Here are some examples of function definitions:

```
define value id = value;
// called as `1 id`, yielding `1`

define not value = false;
// called as `not 1`, yielding `false`

define a ++ b = [a, b];
// called as `1 ++ 2`, yielding `[1, 2]`

define a between: min and: max = true;
// called as `1 between: 0 and: 3`, yielding `true`

define first(a, b) = a;
// called as `first(1, 2)`, yielding `1`;
```

And in all of these cases, they're roughly equivalent to the following:

```
define id = fun(value) -> value;
define negate = fun(value) -> false;
define concatenate = fun(a, b) -> [a, b];
define between = fun(a, min, max) -> true;
define first = fun(a, b) -> a;
```

Unary prefix functions and binary infix functions only exist for a set of
special symbols. All other functions must be postfix, mixfix, or use the
parenthesised prefix notation. This decision was made on the basis that
symbolic bindings are difficult to search for in existing search tools,
difficult to pronounce (as there's no agreed-upon pronunciation), and
invites a complicated discussion on associativity and precedence, which
all users are burdened with even if they choose not to use particular
classes of symbols.

Instead of trying to make syntax infinitely flexible, Tamago invites library
authors to provide first-class DSLs instead, with specialised notation, when
such choice makes sense.


##### Logical operators

| Signature | Description                    |
+-----------+--------------------------------+
| `not _`   | Logical negation               |
| `_ and _` | Logical conjunction            |
| `_ or _`  | Logical disjunction            |


##### Arithmetic operators

| Signature | Description                    |
+-----------+--------------------------------+
| `_ + _`   | arithmetic addition            |
| `_ - _`   | arithmetic subtraction         |
| `_ * _`   | arithmetic multiplication      |
| `_ / _`   | arithmetic division            |
| `_ ** _`  | arithmetic exponentiation      |


##### Relational operators

| Signature | Description                    |
+-----------+--------------------------------+
| `_ > _`   | relational greater than        |
| `_ >= _`  | relational greater or equal to |
| `_ < _`   | relational less than           |
| `_ <= _`  | relational less or equal to    |


##### Equality operators

| Signature | Description                    |
+-----------+--------------------------------+
| `_ === _` | structural equality            |
| `_ =/= _` | structural inequality          |


##### Collection operators

| Signature | Description                    |
+-----------+--------------------------------+
| `_ ++ _`  | Concatenation                  |


