module Tamago.Compiler.Codegen.Generator

open Tamago.Language.Ast
open Tamago.Compiler
open Fable.Core

[<Emit("JSON.stringify($0)")>]
let toJsonString s : string = jsNative

let private toSafeId n =
  match n with
  | "_ >= _" -> "$gte"
  | "_ > _" -> "$gt"
  | "_ <= _" -> "$lte"
  | "_ < _" -> "$lt"
  | "_ === _" -> "$eq"
  | "_ =/= _" -> "$neq"
  | "_ >> _" -> "$composer"
  | "_ << _" -> "$composel"
  | "_ ++ _" -> "$concat"
  | "_ + _" -> "$plus"
  | "_ - _" -> "$minus"
  | "_ * _" -> "$times"
  | "_ ** _" -> "$pow"
  | "_ / _" -> "$divide"
  | "_ and _" -> "$and"
  | "_ or _" -> "$or"
  | "not _" -> "$not"
  | n ->
      "_" + (n.Replace("-", "_").Replace(":", "$").Replace(" ", "$"))

let private jsStr (str:string) = toJsonString str

let private jsKey (str:string) = "@t:" + str

let private list sep xs = String.concat sep xs

let private commaList xs = list ", " xs

let private stmtList xs = list ";\n" xs

let private join xs = list " " xs

let private makeArgs arity =
  List.replicate arity 0
    |> List.map (fun _ -> NFresh (fresh "arg"))

let private (+++) a b = Array.append a b

/// Sub-passes (yay single-pass compilers :'>)
let rec addReturns cc stmts =
  let addClauseReturns (e, stmts) =
    (e, addReturns cc stmts)

  let addCaseReturns kase =
    match kase with
    | MCGuarded (p, g, s) -> MCGuarded (p, g, addReturns cc s)
    | MCUnguarded (p, s) -> MCUnguarded (p, addReturns cc s)
    | MCDefault (s) -> MCDefault (addReturns cc s)

  let go stmt =
    match stmt with
    | SExpression e ->
        SReturn e
    | SMulti xs ->
        SMulti (addReturns cc xs)
    | SCond(clauses, otherwise) ->
        SCond(
          Array.map addClauseReturns clauses,
          addReturns cc otherwise
        )
    | SAssign(name, expr) ->
        SMulti([|
          SAssign(name, expr)
          SReturn(EVariable name)
        |])
    | SMatch (e, cases) ->
        SMatch(e, Array.map addCaseReturns cases)
    | _ -> stmt

  if Array.isEmpty stmts then
    [||]
  else
    match Array.splitAt (stmts.Length - 1) stmts with
    | (init, [|last|]) -> Array.append init [|go last|]
    | _ -> failwithf "internal error"


let isHole expr =
  match expr with
  | EHole -> true
  | _ -> false

let hasHoles expr =
  match expr with
  | EApply(c, args) ->
      (isHole c) || (Seq.exists isHole args)
  | _ ->
      false

let handleHole cc expr =
  match expr with
  | EHole ->
      let b = NFresh (fresh "hole")
      ([|b|], EVariable b)
  | _ ->
      ([||], expr)

let handleHoles cc expr =
  match expr with
  | EApply(c, args) when hasHoles expr ->
      let (cBind, c') = handleHole cc c
      let (argsBind, args') = Array.map (handleHole cc) args
                              |> Array.unzip
      let binds = cBind +++ (Array.concat argsBind)
      ELambda(binds, [|SExpression (EApply(c', args'))|])
  | _ ->
      expr


let analyseLazy cc stmts =
  let getThunkInBind bind =
    match bind with
    | BVariable n -> [|n|]
    | BFunction _ -> [||]

  let rec getThunk stmt =
    match stmt with
    | SDefine (NStatic n, _) -> [|n|]
    | SMulti stmts -> Array.collect getThunk stmts
    | SOpen (_, binds) -> Array.collect getThunkInBind binds
    | _ -> [||]
  let thunks = Array.collect getThunk stmts
  Context.extendLazy thunks cc


/// Generation
let compileName cc name =
  match name with
  | NStatic n -> toSafeId n
  | NFresh b -> toSafeId (Context.fresh cc b)
  | NIgnore -> Context.freshIgnore cc

let compileParameter = compileName

let compileExport name =
  match name with
  | NStatic n ->
      sprintf "$self.expose(%s, %s);" (jsStr (jsKey n)) (toSafeId n)
  | _ -> ""

let compileExportAs cc name ex =
  sprintf "$self.expose(%s, %s);" (jsStr ex) (compileName cc name)

let maybeGetName name =
  match name with
  | NStatic n -> n
  | _ -> "_"

let compileLiteral lit =
  match lit with
  | LFloat n -> sprintf "%f" n
  | LInteger s -> sprintf "%sn" s
  | LText s -> jsStr s
  | LBoolean b -> sprintf "%b" b
  | LNothing -> "null"

let rec compileStatement cc stmt =
  match stmt with
  | SUse(ns, alias) ->
      sprintf "
        const %s = $tamago.use_namespace(%s);
        "
        (compileName cc alias)
        (jsStr ns)

  | SOpen(EVariable alias, bindings) ->
      (stmtList (Seq.map (unpackBinding cc alias) bindings))

  | SOpen(m, bindings) ->
      let alias = NFresh (fresh "open")
      sprintf "
        const %s = %s;
        %s
        "
        (compileName cc alias)
        (compileExpr cc m)
        (stmtList (Seq.map (unpackBinding cc alias) bindings))

  | SDefine(name, expr) ->
      sprintf "
        const %s = $tamago.thunk(() => (%s));
        %s
        "
        (compileName cc name)
        (compileExpr cc expr)
        (compileExport name)

  | SFunction(name, parameters, body) ->
      let cc = analyseLazy cc body
      let body = addReturns cc body
      sprintf "
        function %s(%s) {
          $tamago.check_arity(%s, %d, arguments.length);
          %s
        }
        %s;"
        (compileName cc name)
        (commaList (Seq.map (compileParameter cc) parameters))
        (jsStr (maybeGetName name))
        (Array.length parameters)
        (stmtList (Seq.map (compileStatement cc) body))
        (compileExport name)

  | SData(name, def) ->
      sprintf "const %s = %s; %s;"
        (compileName cc name)
        (compileData cc def)
        (compileExport name)

  | SUnion(name, defs) ->
      sprintf "const %s = $self.define_union(%s, ($self) => { %s }); %s"
        (compileName cc name)
        (jsStr (maybeGetName name))
        (stmtList (Seq.map (compileData cc) defs))
        (compileExport name)

  | STest(desc, stmts) ->
      let cc = analyseLazy cc stmts
      let body = addReturns cc stmts
      sprintf "$self.define_test(%s, function() { %s })"
        (jsStr desc)
        (stmtList (Seq.map (compileStatement cc) body))
  
  | SModule(name, body) ->
      let cc = analyseLazy cc body
      let body = addReturns cc body
      sprintf "
        const %s = $self.define_module(%s, ($self) => { %s });
        %s;
        "
        (compileName cc name)
        (jsStr (maybeGetName name))
        (stmtList (Seq.map (compileStatement cc) body))
        (compileExport name)

  | SLet(name, expr) ->
      sprintf "const %s = %s;"
        (compileName cc name)
        (compileExpr cc expr)

  | SLetMutable(name, expr) ->
      sprintf "let %s = %s;"
        (compileName cc name)
        (compileExpr cc expr)

  | SCond(clauses, otherwise) ->
      let compileClause cc (cond, stmts) =
        let cc = analyseLazy cc stmts
        sprintf "if (%s) { %s } else"
          (compileExpr cc cond)
          (stmtList (Seq.map (compileStatement cc) stmts))

      sprintf "%s { %s }"
        (join (Seq.map (compileClause cc) clauses))
        (stmtList (Seq.map (compileStatement cc) otherwise))

  | SAssign(name, expr) ->
      sprintf "%s = %s;"
        (compileName cc name)
        (compileExpr cc expr)

  | SExpression(expr) ->
      sprintf "%s;" (compileExpr cc expr)

  | SAssert(expr, message) ->
      sprintf "if (!(%s)) { throw $tamago.assertion_failed(%s); }"
        (compileExpr cc expr)
        (jsStr message)

  | SUnreachable(message) ->
      sprintf "throw $tamago.unreachable(%s);"
        (jsStr message)

  | SMatch (expr, cases) ->
      let bind = fresh "match"
      sprintf "{
          const %s = %s;
          switch (true) {
            %s
            default: {
              throw $tamago.unmatched(%s);
            }
          }
        }"
        (compileName cc (NFresh bind))
        (compileExpr cc expr)
        (stmtList (Seq.map (compileCase cc (NFresh bind)) cases))
        (compileName cc (NFresh bind))
  
  | SResumeWith (expr) ->
      sprintf "
        $continue($tamago.handled, %s);
        "
        (compileExpr cc expr)

  | SHandler (name, cases) ->
      let hbind = fresh "handler";
      let bind = fresh "handle";
      sprintf "
        const %s = $self.define_handler(%s, function*(%s, $continue) {
          switch (true) {
            %s
            default: {
              $continue($tamago.not_handled, null);
            }
          }
        });
        %s"
        (compileName cc (NFresh hbind))
        (jsStr name)
        (compileName cc (NFresh bind))
        (stmtList (Seq.map (compileCase cc (NFresh bind)) cases))
        (compileExportAs cc (NFresh hbind) name)

  | SInterface(name, types, defs) ->
      sprintf "
        const %s = $self.define_protocol(%s, [%s], ($protocol) => {
          %s
        });
        "
        (compileName cc (NStatic name))
        (jsStr name)
        (commaList (Seq.map jsStr types))
        (stmtList (Seq.map (compileProtocolDefinition cc) defs))

  | SImplement(proto, types, defs) ->
      sprintf "
        $self.implement_protocol(%s, [%s], ($protocol) => {
          %s
        })
        "
        (compileExpr cc proto)
        (commaList (Seq.map (compileExpr cc) types))
        (stmtList (Seq.map (compileImplStmt cc) defs))

  | SFor (name, expr, stmts) ->
      let cc = analyseLazy cc stmts
      sprintf "for (const %s of %s) { %s }"
        (compileName cc name)
        (compileExpr cc expr)
        (stmtList (Seq.map (compileStatement cc) stmts))

  | SRepeat (stmts) ->
      let cc = analyseLazy cc stmts
      sprintf "while (true) { %s }"
        (stmtList (Seq.map (compileStatement cc) stmts))

  | SRepeatWith (binds, stmts) ->
      let cc = analyseLazy cc stmts
      sprintf "{ let %s; %s; while (true) { %s } }"
        (commaList (Seq.map (fst >> compileName cc) binds))
        (unpackRepeatBindings cc binds)
        (stmtList (Seq.map (compileStatement cc) stmts))

  | SContinueWith (binds) ->
      sprintf "%s; continue;"
        (unpackRepeatBindings cc binds)

  | SBreak ->
      "break;"

  | SMulti(stmts) ->
      stmtList (Seq.map (compileStatement cc) stmts)

  | SReturn(expr) ->
      sprintf "return (%s);"
        (compileExpr cc expr)

and compileData cc def =
  match def with
  | DDRecord(tag, fields) ->
      sprintf "$self.define_record(%s, [%s])"
        (jsStr tag)
        (commaList (Seq.map (compileField cc) fields))

and compileField cc field =
  match field with
  | FRequired(name) ->
      sprintf "{ name: %s, init: null, required: true }"
        (jsStr name)
  | FOptional(name, expr) ->
      sprintf "{ name: %s, init: () => (%s), required: false }"
        (jsStr name)
        (compileExpr cc expr)

and compileExpr cc expr =
  match handleHoles cc expr with
  | EApply(callee, args) ->
      sprintf "(%s(%s))"
        (compileExpr cc callee)
        (commaList (Seq.map (compileExpr cc) args))

  | ELambda(parameters, body) ->
      let cc = analyseLazy cc body
      let body = addReturns cc body
      sprintf "(
        function(%s) {
          $tamago.check_arity('(anonymous)', %d, arguments.length);
          %s
        }
      )"
        (commaList (Seq.map (compileParameter cc) parameters))
        (parameters.Length)
        (stmtList (Seq.map (compileStatement cc) body))

  | EVariable(NStatic name) ->
      if Context.isLazy name cc then
        sprintf "$tamago.force(%s)" (compileName cc (NStatic name))
      else
        sprintf "%s" (compileName cc (NStatic name))

  | EVariable(name) ->
      sprintf "%s" (compileName cc name)

  | ETuple(items) ->
      sprintf "[%s]"
        (commaList (Seq.map (compileExpr cc) items))

  | ECons (hd, tl) ->
      sprintf "$tamago.list_cons(%s, %s)"
        (compileExpr cc hd)
        (compileExpr cc tl)
  
  | EEmpty ->
      "$tamago.list_empty()"

  | EBlock(stmts) ->
      let cc = analyseLazy cc stmts
      let body = addReturns cc stmts
      sprintf "(() => { %s })()"
        (stmtList (Seq.map (compileStatement cc) body))

  | EProcess(stmts) ->
      let cc = analyseLazy cc stmts
      let body = addReturns cc stmts
      sprintf "(function*(){ %s })()"
        (stmtList (Seq.map (compileStatement cc) body))

  | ECheck(typ, value) ->
      sprintf "$tamago.instance_of(%s, %s)"
        (compileExpr cc typ)
        (compileExpr cc value)

  | ECastAsType(typ) ->
      sprintf "$tamago.as_type(%s)"
        (compileExpr cc typ)

  | EDo(stmts) ->
      let cc = analyseLazy cc stmts
      let body = addReturns cc stmts
      sprintf "$tamago.effect(function*(){ %s })"
        (stmtList (Seq.map (compileStatement cc) body))

  | EHandle (stmts, cases) ->
      let cc = analyseLazy cc stmts
      let body = addReturns cc stmts
      let bind = fresh "handle";
      sprintf "(
        yield $self.handle(
          function*(){ %s },
          function(%s, $continue) {
            switch (true) {
              %s
              default: {
                $continue($tamago.not_handled, null);
              }
            }
          }
        ))
        "
        (stmtList (Seq.map (compileStatement cc) body))
        (compileName cc (NFresh bind))
        (stmtList (Seq.map (compileCase cc (NFresh bind)) cases))


  | EYield(expr) ->
      sprintf "(yield %s)"
        (compileExpr cc expr)
  
  | EYieldAll(expr) ->
      sprintf "(yield* %s)"
        (compileExpr cc expr)
        
  | EPerform(expr) ->
      sprintf "(yield $tamago.perform(%s))"
        (compileExpr cc expr)

  | ELiteral(lit) ->
      compileLiteral lit

  | ERecord(pairs) ->
      sprintf "$tamago.record({ %s })"
        (commaList (Seq.map (compileExprPair cc) pairs))
  
  | EUpdate(object, pairs) ->
      sprintf "$tamago.update(%s, { %s })"
        (compileExpr cc object)
        (commaList (Seq.map (compileExprPair cc) pairs))

  | EProject(object, label) ->
      sprintf "(%s)[%s]"
        (compileExpr cc object)
        (compileKey cc label)
       
  | ETodo ->
      "$tamago.unimplemented()"

  | EHole ->
      failwithf "internal: Found a hole during codegen"

and compileExprPair cc (key, value) =
  sprintf "[%s]: %s"
    (compileKey cc key)
    (compileExpr cc value)

and compileKey cc key =
  match key with
  | LStatic name ->
      jsStr (jsKey name)
  | LDynamic expr ->
      sprintf "$tamago.symbol(%s)"
        (compileExpr cc expr)

and compileCase cc bind kase =
  let rec unpackBind cc n (idx, bind, _) =
    sprintf "const %s = %s[%s]"
      (compileName cc bind)
      (compileName cc n)
      idx

  and compileCase' cc n p body =
    match p with
    | PTuple ps ->
        let project =
          Array.mapi (fun i p -> (string i, NFresh (fresh "match"), p)) ps
        let binds =
          Seq.map (fun (_,b,p) -> (b, p)) project
        sprintf "
          if ($tamago.is_tuple(%s, %d)) {
            %s;
            %s;
          }"
          (compileName cc n)
          (ps.Length)
          (stmtList (Seq.map (unpackBind cc n) project))
          (foldCase cc binds body)

    | PCons (hd, tl) ->
        let hdBind = NFresh (fresh "match")
        let tlBind = NFresh (fresh "match")
        sprintf "
          if ($tamago.is_list_cons(%s)) {
            const %s = (%s).head;
            const %s = (%s).tail;
            %s;
          }
          "
          (compileName cc n)
          (compileName cc hdBind)
          (compileName cc n)
          (compileName cc tlBind)
          (compileName cc n)
          (foldCase cc [(hdBind, hd); (tlBind, tl)] body)

    | PEmpty ->
        sprintf "
          if ($tamago.is_list_empty(%s)) {
            %s;
          }
          "
          (compileName cc n)
          body

    | PRecord ps ->
        let project =
          Array.map (fun(l,p) -> (compileKey cc l, NFresh (fresh "match"), p)) ps
        let binds = Seq.map (fun(_,b,p) -> (b, p)) project
        sprintf "
          if ($tamago.is_record(%s)) {
            %s;
            %s;
          }
          "
          (compileName cc n)
          (stmtList (Seq.map (unpackBind cc n) project))
          (foldCase cc binds body)

    | PAnything ->
        body

    | PBind name ->
        sprintf "const %s = %s; %s"
          (compileName cc name)
          (compileName cc n)
          body

    | PBindAs (patt, name) ->
        sprintf "const %s = %s; %s"
          (compileName cc name)
          (compileName cc n)
          (compileCase' cc n patt body)

    | PLiteral lit ->
        sprintf "if (%s === %s) { %s }"
          (compileName cc n)
          (compileLiteral lit)
          body

    | PEqual expr ->
        sprintf "if ($tamago.equals(%s, %s)) { %s }"
          (compileName cc n)
          (compileExpr cc expr)
          body

    | PCheck (pattern, expr) ->
        sprintf "
          if ($tamago.instance_of(%s, %s)) {
            %s
          }
          "
          (compileExpr cc expr)
          (compileName cc n)
          (compileCase' cc n pattern body)

    | PExtractRecord (o, ps) ->
        let bind = NFresh (fresh "match")
        let fields = Seq.map (fst >> compileKey cc) ps
        sprintf "
          const %s = $tamago.extract_record(%s, %s, [%s]);
          %s
          "
          (compileName cc bind)
          (compileExpr cc o)
          (compileName cc n)
          (commaList fields)
          (compileCase' cc bind (PRecord ps) body)

  and foldCase cc binds body =
    let doFold s (b, p) =
      compileCase' cc b p s
    Seq.fold doFold body binds
  
  let stmts s =
    let cc = analyseLazy cc s
    sprintf "%s; break;"
      (stmtList (Seq.map (compileStatement cc) s))

  match kase with
  | MCGuarded(p, g, s) ->
      let body = sprintf "if (%s) { %s }"
                   (compileExpr cc g)
                   (stmts s)
      sprintf "case true: { %s }"
        (compileCase' cc bind p body)
  | MCUnguarded(p, s) ->
      sprintf "case true: { %s }"
        (compileCase' cc bind p (stmts s))
  | MCDefault(s) ->
      sprintf "case true: { %s }"
        (stmts s)

and unpackBinding cc alias binding =
  match binding with
  | BVariable name ->
      sprintf "const %s = $tamago.alias(() => (%s[%s]));"
        (compileName cc (NStatic name))
        (compileName cc alias)
        (jsStr (jsKey name))
  | BFunction (arity, name) ->
      let args = commaList (Seq.map (compileName cc) (makeArgs arity))
      sprintf "function %s(%s) { return %s[%s](%s) };"
        (compileName cc (NStatic name))
        args
        (compileName cc alias)
        (jsStr (jsKey name))
        args

and unpackRepeatBindings cc binds =
  let unpackToFresh (_, fresh, expr) =
    sprintf "const %s = %s;"
      (compileName cc fresh)
      (compileExpr cc expr)

  let freshToReal (real, fresh, _) =
    sprintf "%s = %s;"
      (compileName cc real)
      (compileName cc fresh)

  let binds' = Array.map (fun(n,e) -> (n,NFresh (fresh "repeat"),e)) binds
  sprintf "%s; %s"
    (stmtList (Seq.map unpackToFresh binds'))
    (stmtList (Seq.map freshToReal binds'))

and compileProtocolDefinition cc def =
  match def with
  | PDOptionalMethod (name, parameters, body) ->
      let body = addReturns cc body
      let types = Seq.map (maybeGetName >> jsStr) parameters
      sprintf "
        const %s = $protocol.optional_method(%s, [%s], function %s(%s) { %s });
        "
        (toSafeId name)
        (jsStr name)
        (commaList types)
        (toSafeId name)
        (commaList (Seq.map (compileParameter cc) parameters))
        (stmtList (Seq.map (compileStatement cc) body))

  | PDRequiredMethod (name, parameters) ->
      let types = Seq.map (maybeGetName >> jsStr) parameters
      sprintf "const %s = $protocol.required_method(%s, [%s])"
        (toSafeId name)
        (jsStr name)
        (commaList types)

  | PDRequires (proto, types) ->
      sprintf "$protocol.requires(%s, [%s])"
        (compileExpr cc proto)
        (commaList (Seq.map jsStr types))

and compileImplStmt cc stmt =
  match stmt with
  | ISMethod(name, parameters, body) ->
      let body = addReturns cc body
      let ps = Array.map (compileParameter cc) parameters
      sprintf "
        $protocol.implement(%s,
          function %s(%s) { %s }
        );
        "
        (jsStr name)
        (toSafeId name)
        (commaList ps)
        (stmtList (Seq.map (compileStatement cc) body))


  | ISStatement stmt ->
      compileStatement cc stmt


let compileFile cc (file:File) =
  let body = file.definitions
  let cc = analyseLazy cc body
  sprintf
    "module.exports = ($tamago) => {
      $tamago.define_namespace(%s, ($self) => {
        %s
      })
    }"
    (jsStr file.``namespace``)
    (stmtList (Seq.map (compileStatement cc) body))

//== Generator entry-point
let generateFile file =
  let cc = Context.empty
  compileFile cc file
