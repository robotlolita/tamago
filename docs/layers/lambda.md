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


### Records

Records are immutable compositions of labelled values. For example, one
could describe a coordinate in a 2d space using records:

```
{ x: 1, y: 2 }
```

This represents the coordinate where the `x` axis is one, and the `y` axis
is 2.

Values may be projected from records through their names:

```
let p1 = { x: 1, y: 2 };
p1.x; // => 1
p1.y; // => 2
```

Finally, one can create new records that share some of an existing record's
associations:

```
let p1 = { x: 1, y: 2 };
let p2 = p1 { y: 3 };

p1.x; // => 1
p1.y; // => 2

p2.x; // => 1  (same as p1.x)
p2.y; // => 3  (updated association)
```


### Lists

Lists are an inductive data structure that allows us to express sequences
of values. The implementation Tamago uses is commonly referred to as a
"linked list" in computer science literature.

A list can be one of two possibilities:

  - The empty list: `[]`; or
  - A link of a value followed by another list: `[value, ...list]`;

For example, to express a sequence of natural numbers from 1 to 4, one could
write:

```
[1, ...[2, ...[3, ...[4, ...[]]]]]
```

But since lists are such a common data structure, Tamago allows one to express
the same list in the following way:

```
[1, 2, 3, 4]
```

Linked lists are immutable, thus one may only create new lists that shares
values of existing lists. For example, if we wanted to add `0` to the
the beginning of the list, we could write:

```
let naturals = [1, 2, 3, 4];
let naturals2 = [0, ...naturals];
```

Now `naturals2` represents the sequence of naturals from 0 to 4.


### Pattern matching

We've seen how to construct values in Tamago, but what about *using* these
values? Records still have a projection operator, but how do we go about
using parts of a list? The answer in Tamago is **pattern matching**.

Now, Tamago uses pattern matching both for dealing with inductive data
structures, and as a more general form of control flow. Indeed, common
constructs in other languages, like `if ... then ... else ...` are really
a special application of pattern matching in Tamago.

The pattern matching construct Tamago uses is very similar to other functional
languages, particularly ML dialects:

```
match [1, 2, 3, 4] with
| [a, b, 3, 4] => [a, b];
end
```

The following patterns are supported by Tamago:

  - `some-name`: binds the value at that position to `some-name`;
  - `p as some-name`: binds the value to `some-name`, but only if the pattern
    `p` successfully matches;
  - `1000`, `"foo"`, `true`: matches if the value equals the literal;
  - `{ key: p }`: matches a record containing an association with key `key`,
    but only if the value associated by that matches the pattern `p`;
  - `[a, b, c, ...d]`, `[]`: matches lists;
  - `_`: matches anything, but doesn't bind it to any name;
  - `^expr`: matches anything that equals the value yielded by evaluating the
    expression `expr`;

Tamago also supports guards in pattern matching. For example:

```
match [1, 2] with
| [a, b] when a + b === 2 => 2;
| [a, b] => 3;
end
```

The example above will return 3, as the first pattern only matches if the
guard `a + b === 2` holds.


### Lambda abstractions and application

In order to abstract over expressions, Lambda Tamago provides lambda
abstractions. A lambda in this context is an anonymous closure---a function
with its accompanying environment record.

Lambdas may be expressed through the `fun` syntax:

```
fun(X, Y, Z) -> X + Y + Z
```

And they can be applied through the parenthesised invocation syntax:

```
let inc = fun(X) -> X + 1;
inc(1); // => 2
```

Tamago's named functions, as described in the next section, are just a
convenient form of using lambdas.


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


#### Eager, local bindings

Besides `define`, Tamago also provides an eager binding form that works
*inside* function bodies. The `let` form associates names with values,
and thus evaluates the expression on the right before proceeding.

```
let two = 1 + 1;
let four = two + two;
```

Will have the following reduction semantics:

```
let two = 1 + 1;
let four = two + two;
--------------------------------------- (1) simplifying two
let two = 2;
let four = two + two;
--------------------------------------- (2) substituting two in four
let two = 2;
let four = 2 + 2;
--------------------------------------- (3) simplifying four
let two = 2;
let four = 4;
```


## Abstract machine

### Abstract syntax

```
I in integers
F in floats
T in texts
B in booleans
X in names
L in labels

Statement (S):
  let X = E;                      -- eager binding
  E;                              -- expression

Expression (E):
  fun(X, ...) -> E                -- lambda abstraction
  E(E, ...)                       -- lambda application
  X                               -- variable dereference

  delay E                         -- thunks an expression
  force E                         -- forces an expression

  []                              -- empty list
  [E, E]                          -- cons cell

  { L: E, ... }                   -- record
  E { L: E, ... }                 -- record extension
  E.L                             -- record projection

  nothing                         -- unit value
  I | F | T | B                   -- literal

  match E with MC ... end         -- pattern matching

Match case (MC):
  case P when E => S ...

Pattern (P):
  I | F | T | B | nothing         -- literal
  X                               -- bind match
  []                              -- empty list
  [X, X]                          -- cons cell
  { L: X, ... }                   -- record
```

### Static semantics

TODO

### Dynamic semantics

TODO


## Threat model

Lambda Tamago is a simple, pure functional language. As a result the language
is managed, uses only immutable data, and guarantees that all programs written
in it will be deterministic.

There are still many interesting attacks that one may perform against a
Lambda Tamago program. This section describes the common threats. Consider
that all threats described here are accepted, and when the threat is mitigated
by an additional layer, this is noted here.

Note that Lambda Tamago does not consider many forms of external attacks. For
example, if external parties can scan and modify a Lambda Tamago's program's
memory; or if a process may modify a Lambda Tamago program's source when stored
on disk. While those threats exist, Lambda Tamago does nothing to mitigate
them.


### Resource attacks

While Lambda Tamago programs are pure from a strict mathematical perspective,
from a computational one it still requires resources to run. A Lambda Tamago
program will allocate memory, requiring both RAM and disk space (if swap area
is configured on the disk); and instructions take time to execute.

Because there's no way of limiting or placing guarantees on the amount of
resources used by a Lambda Tamago program, it's easy for an attacker to
write a Lambda Tamago program that causes a denial of service, either by
hogging the CPU or by hogging the memory.

In both cases this attack affects not only the Lambda Tamago program itself,
but also other processes sharing the same resources. If a Lambda Tamago program
recursively allocates many linked list nodes, it could starve the machine of
memory, and might cause other processes to crash by not having any available
memory page to store their data on.

A Lambda Tamago program that hogs too many resources might not cause other
processes to *crash*, but it still has a very high probability of making them
run slower. This means that the latency on all processes may increase, and if
any of the processes expects some deadlines to be met, it'll experience hiccups
that may well be catastrophic from that process' point of view.

The unbounded-resources aspect of Lambda Tamago makes it a poor fit for using
as a scripting language inside applications that may receive programs written
by uncooperative users. Resource bounds are added in Bounded Tamago, and can
be used to mitigate the problem---a Bounded Tamago program does not make any
static guarantee about resources, so programs that require more resources than
specified will simply diverge.


### Privacy attacks

Lambda Tamago provides almost no controls for privacy. Static scoping means
that regions may hide some bindings, but top-level bindings are visible for
the entire program.

Furthermore, all data structures in Lambda Tamago are fully reflexive. Which
means that their values may be inspected by any subprogram. While no data
may be *modified*, this still means that in a Lambda Tamago program with mixed
levels of trust, sensitive data may be leaked to untrusted sub-programs.

Privacy controls are introduced in Sensitive Tamago and Modular Tamago. They
solve some privacy around sharing data among components with mixed trust levels,
but provide no static guarantees on sensitive data flow. Because these controls
are dynamic, indirect leaks of secrets is not accounted for.


## Appendix A: Surface Syntax

TODO