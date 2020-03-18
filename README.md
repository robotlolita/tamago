# Tamago

Tamago is a practical, safe, and observable programming language designed to
allow efficient implementation on top of a JavaScript VM. One of the goals of
Tamago is to be usable as a tool for teaching programming, and lowering the
barriers to teaching programming, so it makes sense to support running programs
in a web-browser, rather than requiring students to configure complicated
environments in their machines.

There's nothing really novel in Tamago—it's a practical language, rather than a
research one. But the language does bring together features that have been found
to work well in both existing languages and more recent research papers. It then
lays out these features into layers; this allows different parts of programs to
choose different sets of guarantees, as well as allowing a more incremental way
of learning.


## What does Tamago look like?

Part of Tamago feels like your run-off-the-mill functional language, with
algebraic data types and pattern matching. For example, to invert a binary tree:

```
data Tree =
  | Leaf { value }
  | Branch { left, right };
  
define tree invert =
begin
  match tree with
  | Tree.Leaf { value } =>
      Tree.Leaf { value };
  | Tree.Branch { left, right } =>
      Tree.Branch {
        left = right invert,
        right = left invert
      };
  end
end
```

But part of Tamago also feels like a regular imperative language, with local
mutation and restricted looping constructs:

```
define n fib =
begin
  let mutable result = nothing;

  repeat with
    a = 0,
    b = 1,
    current = 0,
  begin
    if current === n then
      result <- a;
      break;
    else
      continue with
        a = b,
        b = a + b,
        current = current + 1;
    end
  end
  
  result;
end
```

Or even a modern incantation of Haskell's imperative facilities, with algebraic
effect and handlers:

```
define guess-game =
do
  let answer = !random.integer-between: 1 and: 100;
  repeat
    !io.write: "Guess a number (1..100): ";
    let guess = !io.read;
    if guess < answer then
      !io.write: "Too small!";
    else if guess > answer then
      !io.write: "Too big!";
    else
      !io.write: "You got it!";
      break;
    end
  end
end

define main: _ = guess-game;

define pure-main: _ =
begin
  let mutable stdin = [];
  let mutable stdout = [];

  handle
    !guess-game;
  with
  | Random.Integer { min, max } =>
      resume with (max - min) / 2;
  | IO.Read {} =>
      let reply = stdin first;
      stdin <- stdin rest;
      resume with reply;
  | IO.Write { text } =>
      stdout <- stdout append: text;
  end
end
```

For a more detailed introduction to Tamago, see the
[Language Overview](./docs/overview.md).

The [examples](./examples/) folder includes several other small examples of
Tamago in action. You can run them by providing the namespace to the tamago VM:

```shell
$ cd examples
$ tamago compile
$ tamago run tamago::examples::hello-world;
```

## Compiling from source

There are no pre-built versions of Tamago currently, as the compiler is pretty
much a work-in-progress. If you want to try it out, you can compile it directly
from the source in this repository.

The prototype compiler is implemented in Fable (a dialect of F# that compiles to
JavaScript). To compile it you'll need:

  - [Node 12.x+](https://nodejs.org/en/)
  - [Dotnet Core SDK 3.1+](https://dotnet.microsoft.com/download)
  
Once you've got those tools installed, run these commands:

```
cd Tamago
npm install
npm run build
```

You can either symlink `Tamago/tamago.js` somewhere in your `$PATH`, or invoke
it by its file system path.


## Roadmap

Tamago is still a work-in-progress. Below is the current plan for the near
future:

  - [X] A bootstrapping compiler;
  - [ ] A self-hosting, incremental, multi-pass compiler;
  - [ ] A small standard library;
    - [ ] Numerics (Integer, Float64);
    - [ ] Text (codepoints/units, graphemes, slices);
    - [ ] Collections (linked list, vector, set, map, range, stream -- mutable & persistent);
    - [ ] Base (Bool, Maybe, Result, Ordering, base protocols);
    - [ ] Transcript;
    - [ ] PEG-based parsing;
  - [ ] Interactive shell;
  - [ ] Observer-based debugging;
  - [ ] First-class contracts layer;
  - [ ] Property-based testing layer;


## Licence

Tamago is copyright (c) Quildreen Motta 2019-2020, and released under the MIT
licence. See the LICENCE file in this repository for detailed information.
