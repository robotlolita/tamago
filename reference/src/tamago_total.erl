-module(tamago_total).

%%% # Abstract syntax
-type lit_value() ::
  {string, string()}
| {float, float()}
| {integer, integer()}
| {boolean, boolean()}
| nothing.

-type value() :: 
  lit_value()
| {tuple, list(value())}
| {cons, value(), value()}
| empty
| {record, list({atom(), value()})}

-type expr() ::
  {eprimitive, lit_value()}
| {eupdate, expr(), list(expr_pair())}
| {erecord, list(expr_pair())}
| {eproject, expr(), atom()}
| {etuple, list(expr())}
| {econs, expr(), expr()}
| eempty
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


%%% # Runtime values
-type runtime_value() ::
  value()
| {lambda, list(atom()), expr()}.


%%% # Static semantics
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

  


%%% # Interpretation
eval(C, {eprimitive, Value}) -> Value;

eval(C, {eupdate, ERecord, EPairs}) ->
  {record, OldPairs} = eval(C, ERecord),
  Pairs = map3(EPairs, fun(E) -> eval(C, E) end),
  {record, Pairs ++ OldPairs};

eval(C, {erecord, EPairs}) ->
  Pairs = map3(EPairs, fun(E) -> eval(C, E) end),
  {record, Pairs};

eval(C, {eproject, ERecord, Label}) ->
  {record, Pairs} = eval(C, ERecord),
  {ok, Value} = assoc(Pairs, Label),
  Value;

eval(C, {etuple, Exprs}) ->
  Values = map(Exprs, fun(E) -> eval(C, E) end),
  {tuple, Values};

eval(C, {econs, E1, E2}) ->
  V1 = eval(C, E1),
  V2 = eval(C, E2),
  {cons, V1, V2};

eval(C, eempty) ->
  empty;

eval(C, {evariable, Name}) ->
  {ok, Value} = lookup(C, Name),
  Value;

eval(C, {eapply, ECallee, EArgs}) ->
  {lambda, Params, Body} = eval(C, ECallee),
  Args = map(EArgs, fun(E) -> eval(C, E) end),
  eval(extend(C, zip(Params, Args)), Body);

eval(C, {ematch, E, Cases}) ->
  Value = eval(C, E),
  {match, Result} = match(C, Value, Cases),
  Result.


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


apply(C, pwildcard, Value) -> 
  [];

apply(C, {pbind, Name}, Value) ->
  [{Name, Value}];

apply(C, {pouter_bind, Pattern, Name}, Value) ->
  join([{Name, Value}], apply(C, Pattern, Value));

apply(C, {pliteral, Value}, Value) ->
  [];

apply(C, {pcons, P1, P2}, {cons, V1, V2}) ->
  join(apply(C, P1, V1), apply(C, P2, V2));

apply(C, pempty, empty) ->
  [];

apply(C, {ptuple, Ps}, {tuple, Vs}) when length(Ps) == length(Vs) ->
  Results = zip_with(Ps, Vs, fun(P, V) -> apply(C, P, V) end), 
  join_all(Results);

apply(C, {precord, Ps}, {record, Pairs}) ->
  Labels = map(Ps, fun({pair_pattern, Label, _}) -> Label end),
  Patterns = map(Ps, fun({pair_pattern, _, Pattern}) -> Pattern end),
  Values = extract(Pairs, Labels),
  Results = zip_with(Patterns, Values, fun(P, V) -> apply(C, P, V) end),
  join_all(Results).


join(failed, _) -> failed;
join(_, failed) -> failed;
join(Xs, Ys) -> Xs ++ Ys.

join_all(Results) -> lists:foldl(fun join/2, [], Results).


extract(Pairs, []) -> [];
extract(Pairs, [Label | Rest]) ->
  {ok, Value} = assoc(Pairs, Label),
  [Value | extract(Pairs, Rest)].



%%% # Helpers
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