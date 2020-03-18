# Tamago

Tamago is a safe, managed, and layered programming language that's meant to
run on some host platform. The current implementation targets JavaScript VMs
as the host.


## 0. Lambda Tamago

At the first layer, Lambda Tamago provides basic building blocks for functional
programming. This includes lambda abstractions, pattern matching, and inductive
data structures.

### Basic data types

  - **Arbitrary-precision integer**: `1_000_000_000_000_000_000_000_000`;
  - **64-bit IEEE-754 floats**: `1_000.000_001`;
  - **Unicode text**: `"私はOKです 😊"`;
  - **Logical/boolean**: `true`, `false`;
  - **Special unit/null value**: `nothing`;
  - **Tuples**: `(1, 2, 3)`;
  - **Linked lists**: `[1, 2, 3, ...tail]`;
  - **Anonymous records**: `{ label: value }`;
  - **Tagged records**: `Tag { label: value }`;

### Records

Tamago provides immutable and extensible records, both anonymous ones and
tagged ones. An anonymous record has no specific shape, and is completely
structural. A tagged record has a pre-defined shape, and equality requires
the tags to match as much as it requires the key/value pairs to.

One may construct a record through the literal syntax:

```
let p1 = { x: 1, y: 2 };
```

And they may also construct a record by extending an existing record:

```
let p2 = p1 { x: 0, z: 1 };
```

Values may be projected from the record by their respective labels:

```
p1.x; // => 1
p2.x; // => 0
p2.z; // => 1
```

It's not possible to project a label that does not exist in the record. So
code like the following will halt the process:

```
p1.z; // => panic: no label `z` in record
```

### Tagged records

A record may be *tagged*, which means that besides its key/value pairs, there's
also a shape or type that is unique to objects like that one. Tagged records
must be declared before they can be constructed:

```
data Point2d { x, y };
let p1 = Point2d { x: 1, y: 2 };
```

Extending a tagged record works similarly to extending a regular anonymous
record:

```
let p2 = p1 { x: 3 };
```

However, tagged records require all declared fields to be present, and allows
no extraneous fields, so all of the following will halt the process:

```
let p3 = Point2d { x: 1 }; // panic: missing `y` field
let p3 = Point2d { x: 1, y: 2, z: 3 }; // panic: extraneous `z` field
```

It's important to note that every tag definition yields a globally unique tag,
so tags are not compared by their name. For example, consider the following
piece of code:

```
define p1 = begin
  data Point2d { x, y };
  Point2d { x: 1, y: 1 };
end

define p2 = begin
  data Point2d { x, y };
  Point2d { x: 1, y: 1 };
end

p1 === p2; // false
```

### Unions

Tagged records can be grouped into unions to better indicate that there can
be one of several possibilities. For example:

```
data Maybe =
  | Some { value }
  | Nothing {};
```

Construction of these values follow a similar pattern, but one must project
the correct tag from the union:

```
let some1 = Maybe.Some { value: 1 };
let none = Maybe.Nothing {};
```


### Dynamic labels (symbols)

Record labels can be static, as seen before. In which case the values can be
accessed by anyone who knows the name of the label. But labels can also be
dynamic--in which case the value can only be accessed by those who have a
reference to the label:

```
let password = symbol: "password field";
let person = { (password): "some value", username: "user1" };
person.username; // => "username"
person.password; // => panic: field `password` not found in record
person.(password); // => "some value"
```

Dynamic labels exist only as a form of fine-grained, capability-based access
control, and otherwise work similarly to static labels.

### Pattern matching

Tamago uses pattern matching for dealing with inductive structures, and as a
more general form of control-flow. Indeed, `if...then...else ... end` is just a
more specialised form of pattern matching.

The syntax is very similar to what you'd see in a ML language:

```
match (a, b) with
| (_ is Integer, 1) when a > 1 => a;
| (_, _) => 1;
end
```

Tamago supports the following patterns:

  - `pattern as name` -- matches if `pattern` matches, binds the outer value to `name`;
  - `name` -- matches anything, binds it to `name`;
  - `1`, `"foo"`, `true`, `nothing` -- matches if the value equals the literal;
  - `(p1, p2, p3)` -- matches a tuple of the given arity, if all subpatterns match;
  - `[p1, p2, ...p3]` -- matches a list of the given shape, if all subpatterns match;
  - `{ l: p1 }` -- matches a record if it contains at least the given labels, and the values match the provided patterns;
  - `Tag { l: p1 }` -- matches a tagged record if it contains at least the given labels, and the values match the provided patterns;
  - `^expr` -- matches if the value equals what the given expression evaluates to;
  - `_` -- matches anything but binds nothing;

### Lambda abstractions

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

Do note that unlike some dynamic languages, Tamago has no concept of a
"variadic" lambda. All lambdas take a fixed amount of arguments, and it's
an error to provide a different number of arguments than the expected ones.

### Bindings

Tamago provides both early bindings, through `let`, and lazy bindings, 
through `define`:

```
let a = 1 + 1; // always evaluated here
define b = 1 + 1; // only evaluated if and when `b` is used, at most once
```

### Function definition and application

Tamago also provides a convenient form of defining functions piggy-backing
on the `define` construct. Functions in Tamago may be:

  - **infix**: `_ + _`, `_ === _`, only defined for a static set of operators;
  - **prefix**: `not _`, the only operator currently supported;
  - **parenthesised prefix**: `f(_, _, _)`, the common application form;
  - **postfix**: `_ seconds`;
  - **mixfix**: `_ between: _ and: _`;

Function application and definition follows more or less the same syntax:

```
define a + b = to-do;
1 + 2;

define not a = to-do;
not 1;

define f(a, b, c) = to-do;
f(1, 2, 3);

define x seconds = to-do;
2 seconds;

define x between: min and: max = to-do;
1 between: 0 and: 3;
```

Functions can also be partially applied with the `_` (hole) operator:

```
let add-one = _ + 1;
add-one(2); // equivalent to (2 + 1)
```

Note that function definitions and lambdas are equivalent (they're the
same at runtime!), but operators are special syntax that can't be applied
to regular variable names:

```
define a + b = to-do;
define plus = fun(a, b) -> to-do;
```

#### Operators and precedence

The main precedence defined in Tamago is, from weakest to strongest:

  - **mixfix**: `a some: b function: c`;
  - **infix**: `a + b`;
  - **prefix/postfix**: `not a`, `a seconds`;
  - **prefix parenthesised**: `f(a, b, c)`;

All infix operators have the same precedence, and combining different infix
operators *requires* parenthesis for specifying precedence. This applies even
to mathematical operators:

```
1 + 2 * 3;  // this is a parsing error

(1 + 2) * 3; // this is OK
1 + (2 * 3); // this is also OK
```

For sequences of the same infix operator, they always associate to the left:

```
1 + 2 + 3 + 4;        // this is OK
(((1 + 2) + 3) + 4);  // and always means this
```

Although the precedence and associativity is static, Tamago does not allow
user-define sequences of symbols for infix operators. This is both because
our tools are not great for searching sequences of symbols, and because
there's no agreed upon pronunciation or meaning for them.

Instead of making the infix syntax more flexible, Tamago expects library
authors to build fully-fledged first-class DSLs for embedded languages,
including their own parser and semantics.

The following tables describe the operators Tamago supports:

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


### Blocks

Tamago has a statement/expression separation. Things like `define`, `let`,
`data`, and such are statements, and may only appear in the statement-level
of the grammar. Things like function application are expressions, and may not
include statements directly in any of its sub-expressions.

To allow embedding statements where expressions are expected, Tamago has
a concept of *statement blocks*, an expression that allows one to provide
a sequence of statements, where the value of the last statement is taken as
the result of the block, if a value is produced (otherwise the result is
`nothing`).

```
let a = begin
  define f(x) = x;
  f(1);
end

a; // => 1
```

## 1. Contract Tamago

Tamago supports both first-order and higher-order contracts. Contract Tamago
also adds features for defining other forms of dynamic verification, like
tests (and in the future, property-based tests).

> NOTE: this layer is still a work-in-progress. Types and contract abstraction
>       are mostly lacking right now.


### Assert statements

Assertions are the most basic form of first-order contracts in Tamago. An
assertion describes an invariant that must be true at runtime, using regular
expressions:

```
define a / b =
begin
  assert a is Integer;
  assert b is Integer;
  assert b =/= 0;
  integer-divide(a, b);
end
```

Here we make it explicit that the function only works for integer values, and
requires the divisor to be different from 0.

Tamago also provides a special form of assertion in the form of the statement
`unreachable`. This allows one to be explicit about paths that ought not to
be taken at runtime, but which could not be described in any other way.

```
define a / b =
begin
  if b =/= 0 then
    integer-divide(a, b);
  else
    unreachable "b should never be 0!"
  end
end
```

Because Tamago may use assertions to guide other processes and do
optimisations, it's preferred to use pattern matching and regular assertions
where possible, and only use `unreachable` statements as a last resort.


### Tests

This layer also supports defining tests on functions and larger blocks of code.
Currently only basic unit testing is supported, but in the future
property-based testing and other forms of dynamic verification will also be
supported.

```
define not value = begin
  match value with
  | true => false;
  | false => true;
  end
where
  assert not true ==> false;
  assert not false ==> true;
end
```

Tests don't need to always be attached to a function. They can also stand on
their own in any block of code:

```
test "This is some test" = begin
  assert 1 + 1 ==> 2;
  assert 4 / 2 ==> 2;
end
```

Tests can be executed through `tamago test`.


### Special holes

When one is iterating over a program design, they may not yet have a fully
functional program for every part of the program. Tamago allows the special
`to-do` expression to stand for any sub-expression in a program, signaling
that such part has yet to be done.

For example:

```
define a between: min and: max = to-do;
do-something();
```

Will compile and execute normally, as `_ between: _ and: _` is never called.

Such holes can be used for iterating over the program's design, and in the
future will be used to guide live-programming tools, and sketching tools
by allowing programs to be synthesised automatically.

For example:

```
define list sort = 
begin
  to-do;
where
  assert [] sort ==> [];
  assert [0, 2, 1] sort ==> [0, 1, 2];
end
```

Could be fed to the sketching tool to automatically suggest the following
implementation for sort:

```
define list sort =
begin
  match list with
  | [] => [];
  | [x, ...rest] =>
      let less = rest filter: _ < x;
      let more = rest filter: _ >= x;
      less ++ [x] ++ more;
  end
where
  assert [] sort ==> [];
  assert [0, 2, 1] sort ==> [0, 1, 2];
end
```

This form of example-based and tool-assisted development is something Tamago
aims to fully support in the future.


## 2. Method Tamago

Method Tamago introduces multi-methods, grouped into interfaces. You can see
this as a form of first-class type classes, or as a less restricted form of
Clojure protocols.


### Interfaces and Implementations

Multi-methods are grouped into interfaces. And then these interfaces are
implemented for many kinds of values.

```
interface Equality(a, b) with
  method a === b;
  method a =/= b = not (a === b);
end
```

The interface is parameterised by some variables, and these variables
are used in the method to indicate which parameters are to be used
for dispatching.

Methods may be either required or optional. An optional method is one that
provides a default implementation, but allows that implementation to be
overriden by specialised implementations for performance or more restricted
behaviour.

The `implement` construct provides an implementation of the protocol, and
must provide the roles to substitute for each parameter that the interface
expects:

```
implement Equality(Float64, Float64) with
  method left === right = float-equals(left, right);
  method left =/= right = float-not-equals(left, right);
end

implement Equality(Integer, Integer) with
  method left === right = integer-equals(left, right);
  // =/= implementation inherited from the Equality interface
end

implement Equality(Float64, Integer) with
  method left === right = left === integer-to-float(right);
end

implement Equality(Integer, Float64) with
  method left === right = integer-to-float(left) === right;
end
```

### Dispatch

Note here that dispatch does not work on contracts, but rather on a more
concrete concept of "role". A role can be:

  - A primitive type (`Integer`, `Text`, ...);
  - A record tag;
  - An union;
  - An interface;
  - The special type `Any`;
  - The special role `itself`, e.g.: `Integer.itself`

Dispatch takes into account the *distance* from a concrete type to order the
dispatch. And the sorting proceeds left-to-right.

For example, let's say that an interface is parameterised by two variables.
Then it has the following implementations:

```
implement Interface(Any, Any) with ... end
implement Interface(Integer, Integer) with ... end
implement Interface(Maybe.Some, Maybe) with ... end
implement Interface(Maybe, Maybe.Some) with ... end
```

Regardless of the order in which these appear in the source code, the following
order is used for dispatch:

```
(Integer, Integer)   -- all concrete
(Maybe.Some, Maybe)  -- first concrete
(Maybe, Maybe.Some)  -- second concrete
(Any, Any)           -- always last
```

### Inheritance

Interfaces may extend other interfaces, requiring implementations to exist
for them whenever someone implements the interface.

For example, when defining a `Monad` interface, the objects also need to
implement `Applicative`, and this dependency can be made explicit in the
code:

```
interface Applicative(typ, instance) with
  method typ of: value;
  method instance apply: value;
end

interface Monad(typ, instance) with
  requires Applicative(typ, instance);
  method instance chain: transform;
end
```

Now when we implement these interfaces, Tamago will only allow implementations
of `Monad` where an implementation for `Applicative` also exists:

```
// this is okay
implement Applicative(Maybe.itself, Maybe) with
  method _ of: value = Maybe.Some { value };
  method m apply: f = begin
    match (f, m) with
    | (Maybe.Some { value: f }, Maybe.Some { value: v }) =>
        Maybe.Some { value: f(v) };
    | (Maybe.Nothing {}, _) =>
        Maybe.Nothing {};
    | (_, Maybe.Nothing {}) =>
        Maybe.Nothing {};
    end
  end
end

implement Monad(Maybe.itself, Maybe) with
  method m chain: f = begin
    match m with
    | Maybe.Some { value } => f(value);
    | Maybe.Nothing {} => Maybe.Nothing {}
    end
  end
end

// this is not okay, as there's no Applicative implementation for List
implement Monad(List.itself, List) with
  method m chain: f = begin
  | [] => [];
  | [x, ...xs] => f(x) ++ (xs chain: f);
  end
end
```

## 3. Cooperative Tamago

Cooperative Tamago introduces co-routines, loops, and local mutation. These
primitives work as a building block for the cooperative concurrency used by
most of single-node Tamago programs.

### Co-routines

A co-routine (cooperative) block is an expression described by the `process`
keyword. It defines a block of code that can be suspended and resumed at any
point in time, but suspensions are only allowed for the same stack-frame. That
is, Tamago's coroutines are stackless---they only capture the current frame,
and thus do not allow functions called from the process to suspend the process
itself. The restriction is necessary to allow efficient implementation in a
JavaScript VM. It also makes suspensions explicit.

So a co-routine is defined with `process`, and it's suspended through `yield`.

```
let naturals = process
  yield 1;
  yield 2;
  yield 3;
end
```

A co-routine may be interleaved into another through the `yield all`
expression, which suspends the current process until the interleaved process
completes:

```
// this yields 1, 2, 3 from naturals before it yields 4
let naturals2 = process
  yield all naturals;
  yield 4;
  yield 5;
  yield 6;
end
```

Processes don't begin executing once they're constructed. They need to be
explicitly executed. The standard library provides the `_ run` function to
run processes:

```
let naturals-process = naturals run;
```

`_ run` returns a suspended process. The process can be resumed up to the next
`yield` through the `_ resume` and `_ resume-with: value` functions. The former
is equivalent to `_ resume-with: nothing`, but may only be called if the
process has not yielded before.

```
naturals-process resume;
// => Yield { value: 1 }
naturals-process resume-with: nothing;
// => Yield { value: 2 }
naturals-process resume-with: nothing;
// => Yield { value: 3 }
naturals-process resume-with: nothing;
// => Done { value: nothing }
```

A resumption may return `Yield { value }` if the process has suspended with
an intermediate value, or `Done { value }` if the process has finished
all its instructions---in this case `value` is whatever the process returned.

Yielding and Resuming are used for building cooperative concurrent processes.
When a process yields, it's relaying some message to its executor. When the
executor resumes the process with a value, it's replying to the previous
message. The resumption value essentially replaces the entire `yield` 
expression as its evaluation result, allowing cooperation like this:

```
let sum = process yield 1 + yield 2 end;

define perform-sum: proc value: value =
begin
  match proc resume-with: value with
  | Yield { value: 1 } => perform-sum: proc value: 3;
  | Yield { value: 2 } => perform-sum: proc value: 6;
  | Done { value } => value
  end
end

perform-sum: sum run value: nothing;
// => 9 (equivalent to `3 + 6`)
```

### Local mutability

Processes by themselves introduce a form of mutable state to Tamago, such that
resuming the same process may yield different values, as each time one does so
the state of the process changes.

Tamago also introduces another form of local mutable bindings for directing
this state. This is introduced through the `let mutable` construct and the
`name <- value` operator.

```
let mutable x = 1;
x <- x + 1;
x; // => 2
```

Proper semantic restrictions for local mutability are still to be defined. As
a pass-by-value language, however, it's not possible to pass the mutable
binding to another function to be mutated there.


### Iteration

Processes can be treated as streams, in which values are produced over time.
The `for ... in ... begin ... end` construct allows performing some action
for each yielded value, under the assumption that the process expects no
particular replies from the executor:

```
let mutable sum = 0;
for n in naturals begin
  sum <- sum + n;
end
sum; // => 1 + 2 + 3
```

Note that the expression provided needs to resolve to an Iterable value,
but not necessarily a process instance. We'll discuss Iterables more when
we discuss interfaces later.


### Repetition

Local mutation and control flow allow repetition to be used as a form of
directing processes, as is common in imperative programming. Because Tamago
does not support tail-call optimisation, and since processes only capture
the current stack frame, repetition constructs are the way to handle unbounded
iteration/recursion.

```
let fib = process
  yield 0;
  yield 1;
  repeat with n = 0, m = 1 begin
    yield n + m;
    continue with
      n = m,
      m = n + m;
  end
end;

let fibp = fib run;
fibp resume; // => Yield { 0 }
fibp resume-with: nothing; // => Yield { 1 }
fibp resume-with: nothing; // => Yield { 1 }
fibp resume-with: nothing; // => Yield { 2 }
fibp resume-with: nothing; // => Yield { 3 }
fibp resume-with: nothing; // => Yield { 5 }
```

Tamago also provides a `repeat` block without bindings, which can be
controlled through local mutable bindings. This construct does not
allow `continue with` statements:

```
let fib2 = process
  let mutable n = 0;
  let mutable m = 1;
  yield n;
  yield m;
  repeat
    yield n + m;
    let tmp = n;
    n <- m;
    m <- tmp + m;
  end
end
```

Repetitions can be stopped through the `break` statement. The `break` statement
only allows stopping the current repetition, so stopping nested ones requires
additional bookkeeping (this might change in the future).

```
let one-to-ten = process
  repeat with n = 1 begin
    yield n;
    if n < 10 then
      continue with n = n + 1;
    else
      break;
    end
  end
end
```

## 4. Effectful Tamago

