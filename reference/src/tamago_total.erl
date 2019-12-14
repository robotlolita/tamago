%% # The Total Calculus
%%
%% This module provides a reference implementation for the simplest calculus in Tamago:
%% the Total calculus. It describes its abstract syntax, static semantics, and
%% dynamic semantics through an operational reduction machine.
%%
%% The module is written in a sort-of literate style.
-module(tamago_total).

%% # Abstract syntax
%% 
%% The most primitive types in Total are the same primitive types used by the rest
%% of Tamago. These types are common among most programming languages.
%%
%% Tamago's `Text` is an opaque textual type; `Float` is a 64-bit IEEE752 floating
%% point value. `Integer` is an arbitrary-precision integer value. `Boolean` is
%% either `true` or `false`. And `nothing` is a special value to indicate that no
%% value can be produced, similar to what MLs call `unit`.
-type lit_value() ::
  {text, binary()}
| {float, float()}
| {integer, integer()}
| {boolean, boolean()}
| nothing.

%% We extend these primitive values with some complex types:
%%
%%   - Tuples: immutable and static sequences of values (mixed types).
%%   - List: the common linked list implementation (single type).
%%   - Record: anonymous records with scoped labels.
-type value() :: 
  lit_value()
| {tuple, list(value())}
| {cons, value(), value()}
| empty
| {record, list({atom(), value()})}.


%% Moving on to the expression part of the terms, Total provides the bare
%% minimum of features to transform the kind of values it provides.
%% Transformations are mostly based on pattern matching and function
%% application.
-type expr() ::
  {eprimitive, lit_value()}
%% Records support extension and direct projection.
| {eupdate, expr(), list(expr_pair())}
| {erecord, list(expr_pair())}
| {eproject, expr(), atom()}

| {etuple, list(expr())}
| {econs, expr(), expr()}
| eempty
%% Local let bindings allow naming values.
| {elet, atom(), expr(), expr()}
| {evariable, atom()}

| {eapply, expr(), list(expr())}
| {ematch, expr(), list(match_case())}.

-type expr_pair() :: {epair, atom(), expr()}.
-type match_case() :: {match_case, pattern(), expr(), expr()}.

-type pattern() ::
  pwildcard
| {pbind, atom()}
| {pouter_bind, pattern(), atom()}
| {pliteral, lit_value()}
| {pcons, pattern(), pattern()}
| pempty
| {ptuple, list(pattern())}
| {precord, list(pair_pattern())}.

-type pair_pattern() :: {pair_pattern, atom(), pattern()}.


%% # Runtime values
%%
%% While it's not possible to construct functions from Total, functions
%% can still be used. The set of runtime values then includes all of the
%% values we can construct from Total programs themselves, and the special
%% `Procedure` type.
%%
%% Now, since a `Procedure` is an external, host-provided value, we can't
%% describe its evaluation rules here. Rather, we only rely on the contract
%% that such value takes a list of Tamago runtime values, and produces a
%% new valid value of this set.
-type runtime_value() ::
  value()
| {procedure, fun((list(runtime_value())) -> runtime_value())}.


%% # Static semantics
%%
%% There are certain semantics in Total programs that should be checked
%% before they can be evaluated, which covers things that cannot be
%% captured by the abstract syntax, as it often exhibits context-sensitive
%% properties, and our specification of the abstract syntax through Erlang
%% types can only describe so much.
%%
%% Static semantics either hold or not. When they don't hold, the program
%% is considered invalid, and thus there are no dynamic semantics that
%% can be applied to it.

%% ## Unique labels
%%
%% A program in Total may describe anonymous records. Labels within a
%% single record should always be unique. That is, a record like:
%%
%%     { l1 = a, l1 = b }
%%
%% Is not valid, as the label `l1` is provided twice. This is true
%% even if both labels are associated with the same value.
unique_labels({eupdate, E, Ps}) ->
  assert_unique(elabels(Ps)) andalso unique_labels(E);

unique_labels({erecord, Ps}) ->
  assert_unique(elabels(Ps));

unique_labels({eproject, E, L}) ->
  unique_labels(E);

unique_labels({etuple, Es}) ->
  lists:all(fun unique_labels/1, Es);

unique_labels({econs, E1, E2}) ->
  unique_labels(E1) andalso unique_labels(E2);

unique_labels({eapply, E, Es}) ->
  unique_labels(E) andalso lists:all(fun unique_labels/1, Es);

unique_labels({ematch, E, Cs}) ->
  unique_labels(E) andalso unique_labels(Cs);

unique_labels({match_case, P, E1, E2}) ->
  unique_labels(P) andalso unique_labels(E1) andalso unique_labels(E2);

unique_labels({pouter_bind, P, X}) ->
  unique_labels(P);

unique_labels({pcons, P1, P2}) ->
  unique_labels(P1) andalso unique_labels(P2);

unique_labels({ptuple, Ps}) ->
  lists:all(fun unique_labels/1, Ps);

unique_labels({precord, Ps}) ->
  assert_unique(plabels(Ps))
    andalso lists:all(fun unique_labels/1, ppatterns(Ps));

unique_labels(_) -> true.

  
%% ## Unique pattern bindings
%%
%% In order to avoid having to unify variables produced during a match,
%% we require all bindings in a pattern to be unique.
unique_pattern_bindings(_) -> error(todo).


%% # Interpretation
%%
%% Total is a pure language, so an interpretation of Total programs
%% is straightforward with reductions. The `eval` function captures
%% most of the work in reducing a Total expression down to a
%% runtime value.

%% ## Primitive types
%% 
%% Primitive types are already valid runtime values. So any literal
%% requires no evaluation steps.
eval(C, {eprimitive, Value}) -> Value;

%% ## Records
%%
%% Tamago supports anonymous records with scoped labels. One way of
%% constructing such records is by providing a sequence of labelled
%% values (where each label has to be unique).
%%
%% For example:
%%
%%     { x = 1, y = 2 }
%%
%% Would be all needed to describe a 2d point.
%%
%% Pairs are evaluated left-to-right in source order. In the above
%% example, the expression associated with `x` would be evaluated
%% first, then the expression associated with `y`.
eval(C, {erecord, EPairs}) ->
  Pairs = map3(EPairs, fun(E) -> eval(C, E) end),
  {record, Pairs};

%% Records can also be extended. When doing so, a new version of
%% the record is generated, containing the same values as the
%% old record, with any new associations described in the update.
%%
%% Labels within the update have to be unique, but updates are not
%% restricted to just adding new labels to an existing record. They
%% can also provide a new value to an existing label. Consider:
%%
%%     let p1 = { x = 1, y = 2 } in
%%     p1 { x = 0, z = 3 }
%%
%% In this case, the resulting record shares the association for
%% `y` from `p1`, but will contain its own association for `x`, as
%% the value 0. It'll also contain a new association for `z`.
%% Nothing is changed in `p1`.
eval(C, {eupdate, ERecord, EPairs}) ->
  {record, OldPairs} = eval(C, ERecord),
  Pairs = map3(EPairs, fun(E) -> eval(C, E) end),
  {record, Pairs ++ OldPairs};

%% Finally, it's possible to project values from a record association
%% if its label is known. Consider:
%%
%%     let p1 = { x = 1, y = 2 } in
%%     p1.x
%%
%% This expression reduces to the value `1`, which is associated with
%% `x` in that record. As evaluation is only defined for projecting
%% existing association, trying to evaluate something like `p1.z`
%% would be an error.
eval(C, {eproject, ERecord, Label}) ->
  {record, Pairs} = eval(C, ERecord),
  {ok, Value} = assoc(Pairs, Label),
  Value;

%% # Tuples and lists
%%
%% A tuple is a static, immutable sequence of values. It's a sort of
%% iterative structure. Evaluation of its expressions proceeds from
%% left to right.
eval(C, {etuple, Exprs}) ->
  Values = map(Exprs, fun(E) -> eval(C, E) end),
  {tuple, Values};

%% A list is an inductive, cons-cell-based structure. It has similar
%% semantics to this same structure in eager functional languages.
%% Evaluation again proceeds from left to right.
eval(C, {econs, E1, E2}) ->
  V1 = eval(C, E1),
  V2 = eval(C, E2),
  {cons, V1, V2};

eval(C, eempty) ->
  empty;

%% # Local bindings
%%
%% Total allows assigning names to values (but not computations!),
%% through the `let` construct. The scope of these names is static,
%% and they are valid over the region of the `let` body.
%%
%% When a `let` introoduces a name that already exists in the
%% evaluation context, the new association simply shadows the
%% existing value--that is, within the `let` body, the name
%% will refer to the newly associated value, whereas it'll
%% use the old value elsewhere.
eval(C, {elet, Name, Expr1, Expr2}) ->
  Value = eval(C, Expr1),
  C2 = extend(C, [{Name, Value}]),
  eval(C2, Expr2).

%% We can extract associations from the context by dereferencing
%% a name. This succeeds if the name has been introduced by either
%% the host or an user's `let` construct.
eval(C, {evariable, Name}) ->
  {ok, Value} = lookup(C, Name),
  Value;

%% # Procedures
%%
%% Applications in Total only work if the callee evaluates to
%% a host procedure. This host procedure makes the guarantee
%% that it will take a list of runtime values and produce a
%% new valid runtime value.
%%
%% Arguments provided to the application are evaluated left-to-right,
%% after the callee has been fully evaluated.
%%
%% Total cannot make any guarantees about any other behaviour
%% of the host procedure, thus in the presence of them it's
%% even possible for a Total program to not terminate, or to
%% be non-deterministic. We expect host procedures exposed
%% to Total to make the same guarantees Total does, but there's
%% no way to control that.
eval(C, {eapply, ECallee, EArgs}) ->
  {procedure, Fun} = eval(C, ECallee),
  Args = map(EArgs, fun(E) -> eval(C, E) end),
  Fun(Args);

%% # Pattern matching
%%
%% Finally, pattern matching is the general tool for both
%% extracting data and branching the evaluation. We evaluate
%% the match expression, and then try each provided pattern
%% in sequence.
eval(C, {ematch, E, Cases}) ->
  Value = eval(C, E),
  {match, Result} = match(C, Value, Cases),
  Result.


%% Match cases are tried one-by-one, from top to bottom,
%% until one succeeds. The successful match case than has
%% its body evaluated in a context that's extended by the
%% associations resulting from the matched pattern.
%% 
%% When a pattern matches, its unifications result in a set of
%% associations between names and portions of the value being matched.
%% A match case succeeds if, after its pattern matches, its guard
%% expression also holds if evaluated in a new context that includes
%% the matched associations from the pattern.
%%
%% If none of the patterns match, the computation diverges.
match(C, Value, []) ->
  {no_match, Value};

match(C, Value, [{match_case, Pattern, Guard, Body} | Cases]) ->
  case apply(C, Pattern, Value) of
    failed -> match(C, Value, Cases);
    Binds ->
      case eval_if_guard_holds(extend(C, Binds), Guard, Body) of
        {evaluated, Result} -> Result;
        failed -> match(C, Value, Cases)
      end
  end.


eval_if_guard_holds(C, Guard, Body) ->
  case eval(C, Guard) of
    true -> {evaluated, eval(C, Body)};
    false -> failed
  end.


%% Matching a pattern against a value uses a simplified unification
%% algorithm. It can be made simple because duplicated bindings
%% are not allowed in a Tamago pattern, thus all variables in
%% a pattern are guaranteed to be fresh, and will always unify
%% with any value.
%%
%% This means that, in order to match pattern, we recursively
%% apply simpler patterns against the value, and bubble up
%% unified variables. When we have more than one pattern to
%% match on a particular level, the `join` operation ensures
%% that we bubble up the unified variables if none of the 
%% patterns failed to match, and combine all of the sets of
%% individual matches.

%% The `wildcard` pattern always matches a value, but produces
%% no association.
apply(C, pwildcard, Value) -> 
  [];

%% The `bind` pattern always matches a value, and produces a
%% fresh variable association for the given name.
apply(C, {pbind, Name}, Value) ->
  [{Name, Value}];

%% The `outer_bind` pattern matches if its subpattern matches,
%% in which case it produces a fresh variable association like
%% `bind` does, but combines both match results.
apply(C, {pouter_bind, Pattern, Name}, Value) ->
  join([{Name, Value}], apply(C, Pattern, Value));

%% Literals match if the values being matched are the same,
%% producing no associations.
apply(C, {pliteral, Value}, Value) ->
  [];

%% Lists match if its subpatterns match, and bubbles up the
%% union of these matches.
apply(C, {pcons, P1, P2}, {cons, V1, V2}) ->
  join(apply(C, P1, V1), apply(C, P2, V2));

apply(C, pempty, empty) ->
  [];

%% Tuples match if they have the same length, and all subpatterns
%% match, bubbling up the union of these matches.
apply(C, {ptuple, Ps}, {tuple, Vs}) when length(Ps) == length(Vs) ->
  Results = zip_with(Ps, Vs, fun(P, V) -> apply(C, P, V) end), 
  join_all(Results);

%% Records match if the subpatterns provided match. In order to
%% match these patterns, we expect the associations in the patterns
%% to have a subset of the record's labels. We match each labelled
%% pattern against the same labelled value in the record, but we
%% don't require all labels to be present.
apply(C, {precord, Ps}, {record, Pairs}) ->
  Labels = map(Ps, fun({pair_pattern, Label, _}) -> Label end),
  Patterns = map(Ps, fun({pair_pattern, _, Pattern}) -> Pattern end),
  Values = extract(Pairs, Labels),
  Results = zip_with(Patterns, Values, fun(P, V) -> apply(C, P, V) end),
  join_all(Results).

%% Any other case not covered so far means that the pattern does not
%% have a chance of matching the value.
apply(C, _Pattern, _Value) ->
  failed.


%%% # Helpers
join(failed, _) -> failed;
join(_, failed) -> failed;
join(Xs, Ys) -> Xs ++ Ys.

join_all(Results) -> lists:foldl(fun join/2, [], Results).


extract(Pairs, []) -> [];
extract(Pairs, [Label | Rest]) ->
  {ok, Value} = assoc(Pairs, Label),
  [Value | extract(Pairs, Rest)].


assert_unique(Xs) ->
  lists:usort(Xs) =:= lists:sort(Xs).


labels(Pairs) ->
  lists:map(fun({L, _}) -> L end, Pairs).

values(Pairs) ->
  lists:map(fun({_, V}) -> V end, Pairs).

elabels(EPairs) ->
  lists:map(fun({epair, L, _}) -> L end, EPairs).

plabels(PPairs) ->
  lists:map(fun({pair_pattern, L, _}) -> L end, PPairs).

ppatterns(PPairs) ->
  lists:map(fun({pair_pattern, _, P}) -> P end, PPairs).


zip_with(Xs, Ys, Fun) ->
  lists:zip_with(Fun, Xs, Ys).

zip(Xs, Ys) ->
  lists:zip(Xs, Ys).


extend(Context, Pairs) ->
  Pairs ++ Context.

lookup(Context, Name) ->
  assoc(Context, Name).

assoc(AssocList, Label) ->
  case proplists:get_value(Label, AssocList, {not_found, Label}) of
    {not_found, X} -> {not_found, X};
    X -> {ok, X}
  end.


map(List, Fun) ->
  lists:map(Fun, List).

map3(EPairs, Fun) ->
  lists:map(
    fun({T1, T2, X}) -> {T1, T2, Fun(X)} end,
    List
  ).