# Tamago

Tamago is a practical, safe, and observable programming language designed to
allow efficient implementation on top of a JavaScript VM.

This introductory section goes through some of the philosophy behind Tamago's
design, and the set of design goals that guide the design process. Subsequent
sections go into details on each "layer" that makes up the Tamago language.


## A Language-Oriented View of Computing

The Purr project is guided by a view of what computing should *feel* like, and
as its kernel language, such view affects Tamago's design as well. And for Purr,
this view is language-oriented.

But what does it mean for computing to be "language-oriented" anyway? Well, as
humans, language shapes a lot of our communication. And I don't mean just things
like English or Japanese here; we say a lot in different mathematical notations,
in body language, symbolism, etc. Some of these languages were developed over
millenia, some are only a couple of hundred years old, and some are intelligible
only among some small niche groups. However, it's undeniable that all of these
ways of communication are very important for how we navigate, experience, and
affect the world around us.

Another interesting observation here is that we don't just pick one form of
communication and use it all the time for the rest of our lives—communication is
rather contextual. Even when we're talking about something like English, the way
we communicate on a social network like Twitter is very different from how we
choose to communicate with our friends in a BBQ, or with our co-workers at the
office, or with close partners in private moments.

And even when you look at things like academic works, the way they communicate
an idea varies a lot depending on the context. The way you explain your work
changes depending on whether you're talking with peers in your field or people
who're not very familiar with it; depending on whether you're presenting at a
conference or participating in a discussion panel; and even depending on whether
you're publicating a short paper on a journal, a long thesis, or compiling a
book.

We switch between notations, analogies, and graphical visualisations of the same
thing, over and over again, in order to get it through to our target audience.
Yet, when you look at programming, none of this flexibility is present. We pick
exactly one notation to describe everything in—and whenever we feel generous, we
may throw some external documentation to explain it in different ways, although
those end up invariably outdated in a year or two.

Now, this simplified form of communication, where we pick just one notation and
roll with it for everything, makes a lot of sense if we view computing as the
act of getting the computer to do something. In fact, looking at the history of
"coding", this was not a bad idea in the 1950s—programs were not really designed
and communicated through a "programming language"; rather, "coding" was the act
of serialising those ideas into something the computer could understand. And
this serialisation was rather small, and not touched or reused directly.

Programming in 2020 is in a completely different category—programs are not
merely a serialisation format anymore. They're the way we communicate ideas with
our peers; they're the way we share these ideas so other people can build upon
them. And they must be *understandable* by those other people!

We've seen the rise of higher-level languages, and the idea of using small,
embedded DSLs. These approaches certainly bring programs into a more human-like
realm—as programs get written by larger and larger teams, and maintained over
several decades, it's only natural that this pressure would make us focus on
making programs at least more understandable by our peers.

But while higher-level languages and DSLs represent some progress, these
notations are still fairly static. You still pick one notation and use it for
everything. You may choose the level of details you use in parts of a program;
but the context in communication is a dynamic one, and the choices in
programming are not. One cannot just take a portion of a program and *choose* to
view it in a different notation than what it was written on. That is manual work
that needs to be repeated over and over again, every time there's a change in
context.

Every time we change the question we need to ask the program's source, the
context in which the program is to be read on changes with it. And modern
programming is virtually asking questions *all the time*.

So, if we are to take lessons from other fields that have been dealing with
these problems for thousands of years, we need to accept that a static view of
programming notations is not going to cut it. But the next logical question here
is: how do we make notations flexible enough to allow these kinds of contextual
adaptation?


### A Tool-Guided Process of Programming

In order to be able to get contextual visualisation of a program without putting
in the manual effort, the first thing we need to accept is that we'll not be
programming in a text editor anymore.

Note that this does not mean moving towards a visual notation—there are many
visual programming languages out there, and they suffer from the same problem;
the only difference is that they pick a static visual notation instead of a
static textual one. Hardly an improvement as far as Purr's goals go.

Abandoning a text editor isn't exactly a controversial statement. To some
extent, modern programming has already done that. Modern IDEs overlay contextual
information and provide contextual manipulation of the underlying program, in
ways that are not strictly textual.

What Purr proposes is moving a bit more in that direction. To this end, we
separate programs into:

  - The **surface syntaxes**: which are the notation users are seeing on the
    screen, and by which they describe their programs;
    
  - The **structured syntaxes**: which is the set of underlying notations used
    by tools, these include Concrete Syntax Trees (CST), Abstract Syntax Trees
    (AST), Control Flow Graphs (CFG), Data Flow Graphs (DFG), etc.
    
  - The **static semantics**: which are the set of meanings assigned to each of
    the syntaxes, without a need of running the program;
    
  - The **dynamic semantics**: which are the set of meanings assigned to each of
    the syntax when the program *runs*.
    
The terminology is pretty standard in the Programming Languages field. Now, when
working in a program, we're usually limited to describing and reading the
surface syntaxes, and we may get tools that provide us information derived from
static semantics (like inferred types).

What we don't generally have is some way of querying and manipulating the
structured syntax and the dynamic semantics. Although the latter sometimes is
present, in some restricted form, in tools like REPLs and stepping debuggers.

And thus Purr idea is to introduce "modes" of programming. Some of these modes
already exist when we distinguish between the static and dynamic meaning of
programs anyway. However, an important aspect here is that "modes" in Purr are
never *exclusive*. A programmer may have a portion of code opened in static
manipulation mode, at the same time they have such portion of code open in
dynamic visualisation mode, such that they can visualise the execution of that
portion of the program at the same time they change it, and get immediate
feedback on those changes.

Modes can be divided into a few categories:

  - **Writing**: allows one to describe concepts in any particular notation
    directly, most of this is not much different from writing programs in modern
    IDEs, probably with the exception that **writing** can be restricted to any
    portion of code, rather than strictly require some file boundary.
    
  - **Manipulating**: allows one to modify existing concepts in any particular
    notation, as restricted by that notation. For example, if one chooses to
    manipulate tabular data they would get the options of adding new columns,
    reordering rows, or replacing the contents of a particular cell; if one
    chooses to manipulate a functional expression they would get the options of
    wrapping things into a function call, reordering expressions, or renaming
    variables.
    
    The important thing to remember is that all manipulations are
    structure-preserving in regards to the notation and underlying static
    semantics. That is, while one may change the underlying semantics through
    manipulation, they may not put it into an invalid state with respect to its
    static semantics; it's not possible to manipulate programs in ways that
    result in invalid uses of the notation.

  - **Querying**: allows one to search, summarise, and compute information about
    any known aspect of the program (either by static or dynamic analysis—and
    this implies that dynamic analysis has to be recorded), and choose how to
    present that information, with the possibility of storing such queries for
    reuse in the future, or parameterising them.
    
  - **Observing**: allows one to explore how computations **run** and affect
    computing resources and information. This helps programmers build their own
    mental models about how the program works by the process of scientific
    observation, and to validate their own conclusions. Of course, this implies
    that it must always be possible to safely execute a program and observe its
    effects—that is, one must be able to run something like `rm -rf *` without
    having to worry about whether it'll wreck their computer or not.
    
The environment has to, of course, be able to combine these modes in any way,
and use multiple modes for the same portion of the program. This process of
piecing different modes of the same information together is a common human
process.


### Flexible Notations

Once we have a process that allows presenting information in different ways
depending on what questions we're trying to answer at the moment, the next big
challenge is actually presenting this information in a suitable notation.

Of course, once you have a graphical interface, presenting arbitrary information
isn't *exactly* the problem. The challenge is the step before that one: figuring
out which information to present, and figuring out how to present that
information in a way that supports the programmer's analysis of whatever
problem they're trying to solve.

I've mentioned the separation between surface syntax and everything else
before—the surface syntax is the notation we're aiming to present the user. And
it's also the way through which users interact with the system—and here one must
understand that direct manipulation of things, like dragging-and-dropping syntax
nodes, is also a particular notation for a "manipulation language" of sorts.

While this takes care of syntax (or notation, in a more general sense) being a
flexible thing in Purr, it fails to answer an important question: who *defines*
these notations? And how does the system know what its rules are, and on which
contexts they're suitable to be used?

For example, consider the idea of summarising programs into an idea of "types".
Many IDEs for static languages of the ML family (like Haskell, F#, and OCaml)
will happily overlay the inferred types over the source for you, so you can
quickly see what kind of properties the function expects from its inputs and
outputs. And it feels like this would be a notation whose need is common enough
that Purr, as a system with flexible notations, should provide for users, right?

Now, consider the following cases:

  - In a dynamically typed codebase, a programmer is interested in knowing which
    functions read or write to the database, either directly or indirectly. They
    would like to see the "effect types" inferred from a combination of static
    and dynamic analysis passes shown on top of each function's signature to
    quickly see if the function is safe to use anywhere or not.
    
  - In a statically typed codebase, a programmer is interested in seeing what
    types have been covered by previous dynamic runs of their test code. In
    particular, they would like to enrich the type overlays such that it shows
    the expected type along with the types that were actually provided during
    test runs. This way they can extend the tests to verify more of the
    program's expected behaviour.
    
Even though all three situations deal with a concept of "types" and presenting
that idea of types as an overlay somehow, both the source and actual
presentation of that information completely differs in each one of them. But
even if we consider only the first case, we may want to move between a
"richness" spectrum of the type system depending on which details we're
interested on at the moment. Maybe we want to ignore refinements in a type
language that supports that for the current question we're asking; or maybe we
actually care about the details of how the `Equality<A>` type is implemented,
and would like to have that information close to its current use-site.

And types are a *common* summary of information. As we look into the day-to-day
of different programmers, we quickly find things that have **nothing** to do
with seeing types or source code. For example, a game developer tweaking
animations may want to look at an onion-skin rendering of some object in the
current scene, and then tweak values and watch how it affects the animation in
real-time. An interactive-fiction developer may be interested in seeing how all
routes are connected, and be able to select nodes to figure out the possible
paths from each point, in order to check for non-intended dead-ends. A front-end
developer may want to tweak colours and spacing by direct manipulation, choosing
colours in an HSV wheel rather than figuring out an arbitrary set of four
byte-sized numbers.

Under Purr's view of computing, **all** of the tasks above should be achievable
directly inside of the programming environment, at any point in time. With as
many notations as programmers feel the need for.

You can see that there's no way for Purr to provide all of these tools out of
the box—they need to be written by users themselves, according to their
contexts. So, the analysis and presentation of data has to be handled in an
extensible manner, where external systems may provide such feature.

Further, it's completely unreasonable to expect every programmer to build their
own IDE while they go about implementing whatever program they must—the latter
is already work enough for a team of people. And thus it should be possible for
people to build small notational components and share them with the broader
community. In this sense, Purr works as a container for all these user-defined
notational and analysis components.

But this doesn't take care of all problems. In fact, it creates some. If a
programming environment such as Purr is made out of several components, written
by people who are not the Purr developers, how do people manage which components
they should trust or not? How do we make it safe for people to install a random
component from Alice without having to worry that she'll steal some SSH keys?
Or, in an even more common problem, how do we make sure that Alice, Bob, and
Max's components all play well together if users decide to use them together?

While I'm still searching for a well-grounded theory for answering these
questions, it's important to note that [capability-based
security](https://en.wikipedia.org/wiki/Capability-based_security) tries to
address the first problem—and Purr uses a coarse-grained form of it—, whereas
systems like [GToolkit](https://gtoolkit.com/) address the second by isolating
visualisations.


## Design Goals

The view of computing detailed above works as an overarching goal of the entire
Purr system. The core design goals of Tamago are far more modest, in comparison.
These goals are used to decide how to think about Tamago as a programming
language on its own, and which features get to make it into the language, or
which features should be kept out of the language—as much as one may love such
feature, and argue for its usefulness.

The four core design goals are:

  - **Small, incremental learning curve** — One of the use cases for Tamago is
    to use it for teaching programming concepts, in particular to absolute
    beginners. Ideally, this means that it should be possible to introduce
    concepts gradually, without burdening students with unrelated concepts. And
    this also implies that the concepts used by Tamago must not require
    extensive study of areas that are far removed from most people's everyday
    lives.
    
  - **Observability** — People should be able to *explore* computations in ways
    that make sense to them. They should be able to build their own mental
    models of computation by observing what is happening—by observing how things
    work. The "what", "why", and "how" should be questions that get clear
    answers from these observations. And it should be possible to apply this to
    every concept in the system, with concrete examples that can relate to the
    program people are working on—this is also a huge part of why Tamago is not
    statically typed.
    
  - **Can be implemented efficiently in JavaScript** — Tamago is a hosted
    language, and it should be possible to run all of its components and
    programs in a web-browser. Supporting the browser as a platform for
    programming, Tamago can be used for teaching programming without students
    and teachers having to worry about how to configure a productive environment
    in 30 different operating systems. Of course, being "able to implement
    efficiently" does not mean that features need a 1:1 mapping to the host
    language, but it does mean that implementing a feature should not require
    extensive runtime changes (e.g.: as proper tail calls would).
    
  - **Safety** — Tamago should provide people with an environment that is safe
    for exploration, both because components will be in a mixed trust mode, and
    because the core idea is that people can understand programs by observing
    them. This quickly becomes useless if they're not able to observe something
    like `rm -rf *` multiple times to build a mental model of it. Therefore,
    Tamago should let people try all sorts of things without worrying about
    whether that'll render their computer unusable, or if they're going to have
    their computer taken over by random attackers.
    

## A Layered Language

To deliver on these goals Tamago uses an idea of "layered language". One could
say that Tamago is not a single programing language, but a collection of
programming languages, each with well-defined dependencies and interoperability
semantics. This view could, in fact, be applied to other existing programming
languages as well: for example, one could see Python's list comprehensions as a
separate sub-language.

Where Tamago differs from existing languages with respect to layers is in that
they have *well-defined boundaries*. It's possible to make just a subset of the
layers available to the programmer, in which case the programmer will know
exactly which features they can use, and which guarantees they can expect from
the system.

The layered approach was first devised as a teaching mechanism. A teacher could
choose only the layers that contain the features relevant to the concepts being
explained, avoiding problems with students stumbling upon unrelated concepts by
accident—although this by itself creates different problems when students can
search for solutions in something like Google, which is already seen with
Racket's teaching languages.

But another important aspect of having a layered approach is that it makes it
*possible* to provide a very well-defined set of guarantees people can opt for.
This is important for security. Describing guarantees in a complex language,
with several features, is difficult because it's not always straightforward to
understand how these features interact and affect each other. But also because
once you add certain features you must *give up on* some guarantees. For
example, by adding unbounded recursion you can abstract over several problems in
a very practical manner, but this requires giving up on the guarantee that
programs will always terminate. The layered approach lets you choose which
guarantees you need, and base your threat modelling on that. It avoids the need
for ad-hoc restrictions on the language, which often misses edge cases that are
used by attackers to take over the system.

As everything in Purr, the idea of stratifying the language in layers is hardly
novel. Tamago's implementation takes inspiration on Van Roy et al's approach
with [Concepts, Techniques, and Models of Computer
Programming](https://www.info.ucl.ac.be/~pvr/book.html), as much as it takes
inspiration in security-focused languages like
[Noether](https://www.infoq.com/presentations/noether/).


### The Layers

  - [**Lambda Tamago**](./layers/lambda.md) - Provides ways of abstracting
    and transforming data. Guarantees that all computations are deterministic
    and pure, but makes no guarantees about termination or resource usage.
    Programs may still non-deterministically diverge if the machine running
    them cannot always provides the required resources, but terminating
    programs will not compute different values.

<!--
  - **Total** — Provides ways of abstracting data and transforming it, but
    provides no way to abstract over transformations themselves. Guarantees that
    all computations written in the layer will terminate, but makes no
    guarantees about resource usage. Does not include any form of recursion,
    although co-recursive operations are supported if they operate on finite
    data.
    
  - **Core** — Depends on **Total**. Provides ways of abstracting over data
    transformations. Guarantees that all abstractions will be deterministic, but
    does not guarantee that they'll always converge. Core does not include any
    effects or mutable state, and thus can be considered pure.
    
  - **Cooperative** — Depends on **Core**. Provides ways of cooperatively
    interleaving computational processes. Guarantees that all interleavings will
    be deterministic, but not that they'll always converge. Again, it does not
    include any effects or mutable state.
    
  - **Effect** — Depends on **Cooperative**. Provides ways of interacting with
    the outside world through algebraic effects and handlers. Guarantees that
    all effects are controllable, but makes no guarantees about external
    handlers. Execution may be non-deterministic or diverge if handlers talk to
    external systems, but if all effects are handled by pure functions,
    execution is guaranteed to be deterministic.
    
  - **Module** — Depends on **Core**. Provides ways of describing boundaries,
    scopes, and trust. Guarantees that subprograms are not able to do anything
    that its parent program does not allow it to, and guarantees that each
    subprogram is never allowed to do anything more than what its trust boundary
    allows. However, it does not allow placing bounds on time and memory usage,
    so it's not possible to protect against denial-of-service kinds of attacks.
    
  - **Safe-data** — Depends on **Total**. Provides first-order contracts for
    data sructures, but no abstractions over contracts. Contracts always
    terminate, but there are no guarantees about their resource usage, or impact
    over runtime.
    
  - **Safe** — Depends on **Safe-data** and **Core**. Provides higher-order
    contracts and the ability of abstracting over contracts. Inherits the
    problems of contracts possibly diverging from **Core**, and of resource
    usage not being bounded from everything else.
    
  - **Meta** — Depends on **Total**. Provides ways of attaching meta-data to
    computational descriptions, and makes such descriptions available to the
    system. There's no computation involved, and thus no effects on runtime.
-->