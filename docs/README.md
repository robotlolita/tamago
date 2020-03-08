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


### Notations and Context


