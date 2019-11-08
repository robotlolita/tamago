## Design goals

There are four core design goals underlying all decisions in Tamago:


- **Small, incremental learning curve** — we want Tamago to be used for teaching programming for people who’ve never programmed before. Ideally, this means that it should be possible to introduce concepts gradually, without burdening students with unrelated concepts. It also means that the concepts used by Tamago must not require extensive study of areas that are far removed from most people’s everyday lives.

- **Observability** — we want people to explore computations in ways that make sense to them, and to be able to build their own mental models of what’s happening. This means that the language should try to help people visualise what is happening, why it’s happening, and how it’s happening. It should be possible to do this for every concept in the system, and with concrete examples that people can relate to their program’s context—this is largely why Tamago is not statically typed.

- **Can be implemented efficiently in JavaScript** — Tamago will always run on top of a web-browser (or other JavaScript runtime), so all features it provides need to be efficiently implementable in JavaScript. This doesn’t mean that there needs to be a 1:1 mapping, but it does mean that features which require extensive runtime changes (like deep coroutines or proper tail calls) are out of consideration.

- **Safety** — we want to give people a safe environment for exploration. They should be able to try all sorts of things without having to worry about whether it’ll render their computer unusable, or if they’re going to have their computer taken over by some random attacker.


## Language overview

If you look at a small example program in Tamago, you may be tempted to think of it as a functional language, if you know that paradigm:

```
union Tree is
  record Leaf { value }
  record Branch { left, right }
end

function (Tree.Leaf {value}) invert =
  Tree.Leaf { value = value }

function (Tree.Branch {left, right}) invert =
  Tree.Branch {
    left = right invert,
    right = left invert
  }
```

Though a bit unusual in its syntax, the features showcased by this example are found in most functional languages out there. At its very core, Tamago contains a small functional language with tagged records, first-class functions, and pattern matching. This core is one of the first layers of the Tamago language. The name of the layer is also "core".

A layer packages a set of features in order to solve specific problems. As a layered language, Tamago is made out of many such "layers". The Core layer provides a form of computing suitable for *transforming data*. Some of the other layers, and their respective goals are:

  - **Cooperative** -- a basis for cooperative concurrency with semi-coroutines;
  - **Effect** -- a basis for imperative programming with algebraic effects;
  - **Safe** -- a basis for safe programming with higher-order contracts;
  - **Module** -- a basis for modular programming with first-class modules and search-spaces;
  - **Distribute** -- a basis for safe distributed computing with actors;


## A Layered Language

We've mentioned before that Tamago is a *layered* programming language. Let's discuss what that means in more details.

One could say that Tamago is not a single programming language, but a collection of programming languages, each with well-defined dependencies and interoperability semantics. This view could be applied to other existing programming languages as well: for example, one could see Python's list comprehensions as a separate sub-language. Where Tamago differs from these is that these layers have *well-defined boundaries* and it's possible to choose which layers you want to make available to the programmer. The second characteristic depends on the first--one wouldn't be able to provide only layers A, B, and C if the boundaries between those and layer D weren't well-defined.

Primarily, the layered approach is a teaching mechanism. A teacher can choose only the layers that contain features relevant to the concepts being explained, avoiding problems with students stumbling into unrelated concepts by accident--though this by itself seems to open different problems, when students search for solutions on something like Google, as seen with Racket's teaching languages.

The second, but equally important aspect of having a layered approach, is to be able to provide very well-defined guarantees. This is important for security. Describing guarantees in a complex language with several features is difficult because it's not always straightforward to understand how these features interact with each other. But also because the addition of some features *requires* giving up on some guarantees--for example, adding unbounded recursion is important for a lot of practical problems, but it requires giving up on the guarantee of termination which can be very useful for some restricted use cases. A layered approach lets you choose which guarantees you need, and base your threat modelling on that. It avoids the need for ad-hoc restrictions of the language, which often miss edge cases that are used by attackers to take over the systems.

Again, the idea of layering is not novel. Tamago's implementation is inspired by Van Roy's et al approach with [Concepts, Techniques, and Models of Computer Programming](https://www.info.ucl.ac.be/~pvr/book.html) as much as it's inspired by security-focused languages like [Noether](https://www.infoq.com/presentations/noether/).
