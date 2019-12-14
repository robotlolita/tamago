## Total

The Total layer provides the basis for describing and transforming data, but nothing besides that. For example, programs in the Total layer may invoke functions, but they have no way of *defining* functions. As a result of these limitations, the Total layer can make two important guarantees:

  - All Total programs will terminate;
  - All Total programs will always compute the same output, if given the same input;

The Total layer cannot make guarantees about computational resources, though. While we have the guarantee that all programs will terminate, we don't really have any guarantee about *when* they will terminate. We also make no guarantees about how much memory a program requires. A program that requires more memory than available in the system will never compute an answer, but will likely be interrupted by the operating system, or cause the entire computer to freeze. Still, because Total does have any state, it's possible to bound programs on the computational resource used externally--in which case programs requiring more resources will simply diverge.

For example, let us consider [Abigail's prime-testing regular expression](http://abigail.be/):

    "^1?$|^(11+?)\1+$"

This regular expression is a valid program in Total. Given an arbitrary string of "1"s it'll either succeed, if the number of "1"s is not a prime number, or fail otherwise. But in the process it'll backtrack a lot. That means that, for large numbers, it may take minutes to compute an answer--but one will be computed, if there's memory for it.

A Total program diverging is not an attack opportunity, from the point of view of Tamago. But it can be an attack opportunity externally. Depending on how the operating system deals with the increasing resource usage, depending on how the JavaScript machine deals with running out of memory, depending on how restarting the program is handled, depending on how applicable timing attacks are from the outside, etc. A recurring theme when discussing the security properties of Tamago is that it only applies to *Tamago programs*. Anything external is outside of these guarantees.


### The Total calculus

See [tamago_total.erl](./reference/src/tamago_total.erl) for an annotated reference implementation of the Total calculus, covering abstract syntax, static semantics, and dynamic semantics.





