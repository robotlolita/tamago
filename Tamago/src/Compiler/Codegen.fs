module Tamago.Compiler.Codegen.Generator

open Tamago.Language.Ast
open Tamago.Compiler
open Tamago.Compiler.Codegen.Js


//== Runtime primitives
let send o m args =
  jsApp (jsMember o m) args

let rt n args =
  send (jsRawId "$rt") n args

let mrt n args =
  send (jsRawId "$self") n args

let prt n args =
  send (jsRawId "$pattern") n args

let defMod id defs =
  rt "define_module" [
    jsStr id
    jsAnonFun [jsRawId "$self"] defs
  ]

let useMod id =
  rt "import_module" [jsStr id]

let useExternal id =
  rt "import_external" [jsStr id]

let expose name expr =
  mrt "expose" [jsStr name; expr]

let propsToStrList props =
  (props |> List.map (fun (p:Property) -> jsStr p.field))

let defRecord name props =
  mrt "define_record" [
    jsStr name
    jsArray props
  ]

let defCase tag props =
  mrt "define_variant" [
    jsStr tag
    jsArray props
  ]

let defUnion name cases =
  mrt "define_union" [
    jsStr name
    jsArray cases
  ]

let defTest desc expr =
  mrt "define_test" [desc; jsAnonFun [] [jsReturn expr]]

let defSubMod name defs =
  mrt "define_module" [
    name
    jsAnonFun [jsRawId "$self"] defs
  ]

let defFun name ps body =
  jsFun name ps [
    jsReturn body
  ]

let rtOpen object =
  mrt "open" [object]

let thunk e =
  rt "thunk" [jsAnonFun [] [jsReturn e]]

let exAssert e =
  rt "assert" [e]

let exAssertMatch l r =
  rt "assert_match" [l; r]

let exRecord ps =
  rt "record" [jsRecord ps]

let exRecordUpdate r ps =
  send r "$update" [jsRecord ps]

let exProject r l =
  send r "$project" [l]

let exArityCheck arity f =
  rt "check_arity" [arity; f]

let exForce thunk =
  rt "force" [thunk]

let exCons hd tl =
  rt "cons" [hd; tl]

let exEmpty =
  rt "empty" [];

let exMatch v cs =
  rt "match" [v; jsArray cs]

let exScopeSend n =
  mrt "$project_scoped" [n]

let pBind n =
  prt "bind" [n]

let pOuterBind p n =
  prt "bind_as" [p; n]

let pContract p e =
  prt "check" [p; e]

let pEqual l =
  prt "equal" [l]

let pCons hd tl =
  prt "cons" [hd; tl]

let pEmpty =
  prt "empty" []

let pTuple xs =
  prt "tuple" [jsArray xs]

let pRecord xs =
  prt "record" [jsArray xs]

let pExtractor o xs =
  prt "extractor" [o; jsArray xs]

let pAnything =
  prt "any" []

//== Extracting data
let rec defLocals def =
  match def with
  | DImport (_, AName n) -> [n]
  | DImport _ -> []
  | DFFI (_, AName n) -> [n]
  | DFFI _ -> []
  | DOpen _ -> []
  | DRecord (n, _) -> [n]
  | DUnion (n, _) -> [n]
  | DDefine (n, _) -> [n]
  | DModule (n, _) -> [n]
  | DTest _ -> []
  | DFunction (n, _, _) -> [n]
  | DMulti xs -> List.collect defLocals xs

let rec defLazyLocals def =
  match def with
  | DImport (_, AName n) -> [n]
  | DImport _ -> []
  | DFFI (_, AName n) -> [n]
  | DFFI _ -> []
  | DOpen _ -> []
  | DRecord (n, _) -> []
  | DUnion (n, _) -> []
  | DDefine (n, _) -> [n]
  | DModule (n, _) -> []
  | DTest _ -> []
  | DFunction (n, _, _) -> []
  | DMulti xs -> List.collect defLazyLocals xs

let namePatternLocals np =
  match np with
  | NPName n -> [n]
  | NPIgnore -> []

let paramLocals (p:Parameter) =
  namePatternLocals p.name

let rec patternLocals p =
  match p with
  | PBind n ->
      namePatternLocals n

  | POuterBind (p, n) ->
      List.append (patternLocals p) (namePatternLocals n)
  
  | PContract (p, _) ->
      patternLocals p

  | PLiteral l ->
      []

  | PCons (hd, tl) ->
      List.append (patternLocals hd) (patternLocals tl)

  | PEmpty ->
      []

  | PTuple ps ->
      List.collect patternLocals ps

  | PRecord ps ->
      List.collect propPatternLocals ps

  | PExtractor (_, ps) ->
      List.collect propPatternLocals ps

  | PAnything ->
      []

and propPatternLocals (prop:PropertyPattern) =
  patternLocals prop.pattern


let includeModuleLocals cc defs =
  let locals = List.collect defLocals defs
  let lazyLocals = List.collect defLazyLocals defs
  cc |> Context.extendLocals (Set.ofList locals)
     |> Context.extendLazy (Set.ofList lazyLocals)


//== Helpers
let moduleId xs = String.concat "." xs

let detectHole cc expr =
  match expr with
  | EHole ->
      let b = Context.fresh cc (fresh "$")
      ([b], EVariable b)
  | _ ->
      ([], expr)

let detectHoles cc exprs =
  let pairs = List.map (detectHole cc) exprs
  let (holes, exprs) = List.unzip pairs
  (List.concat holes, exprs)

let holeToParam hole =
  parameter (NPName hole)

let holesToLambda cc expr =
  match expr with
  | EApply (c, args) ->
      let (cHoles, c) = detectHole cc c
      let (argsHoles, args) = detectHoles cc args
      let holes = List.append cHoles argsHoles
      if List.isEmpty holes then
        EApply (c, args)
      else
        let ps = List.map holeToParam holes
        ELambda (ps, (EApply (c, args)))
  
  | _ -> expr


//== Generator primitives
let prelude =
  """
 const $rt = Tamago.make_runtime(__dirname, __filename, require, module);
 const $pattern = $rt.pattern;
  """

let rec compileFile cc (file:File) =
  let cc = includeModuleLocals cc file.definitions
  defMod (moduleId file.``namespace``)
    (List.map (compileDeclaration cc) file.definitions)

and compileDeclaration cc decl =
  match decl with
  | DImport (ns, a) ->
      jsConst (compileAlias cc a) (moduleId ns |> useMod |> thunk)

  | DFFI (s, n) ->
      jsConst (compileAlias cc n) (useExternal s)

  | DOpen (e) ->
      jsExpr <| rtOpen (compileExpr cc e)

  | DRecord (n, ps) ->
      jsMulti [
        jsConst (jsId n) (defRecord n (List.map compilePropDef ps))
        jsExpr <| expose n (jsId n)
      ]

  | DUnion (n, cs) ->
      jsMulti [
        jsConst (jsId n) (defUnion n (List.map compileCase cs))
        jsExpr <| expose n (jsId n)
      ]

  | DDefine (n, e) ->
      jsMulti [
        jsConst (jsId n) (thunk (compileExpr cc e))
        jsExpr <| expose n (jsId n)
      ]

  | DModule (n, ds) ->
      let cc = includeModuleLocals cc ds
      jsMulti [
        jsConst (jsId n) (
          defSubMod (jsStr n)
            (List.map (compileDeclaration cc) ds)
        )
        jsExpr <| expose n (jsId n)
      ]

  | DTest (s, e) ->
      jsExpr <| defTest (jsStr s) (compileExpr cc e)

  | DFunction (n, ps, e) ->
      let locals = List.collect paramLocals ps
      let cc = Context.extendLocals (Set.ofList locals) cc
      jsMulti [
        jsConst (jsId n) (
          defFun n 
            (List.map (compileParam cc) ps)
            (compileExpr cc e)
        )
        jsExpr <| expose n (jsId n)
      ]

  | DMulti xs ->
      jsMulti (List.map (compileDeclaration cc) xs)

and compileExpr cc e =
  match holesToLambda cc e with // TODO: move to a separate pass
  | ESequence (l, r) ->
      jsComma (compileExpr cc l) (compileExpr cc r)

  | ELet (NPIgnore, value, body) ->
      jsComma (compileExpr cc value) (compileExpr cc body)

  | ELet (NPName n, value, body) ->
      let locals = Set.ofList [n]
      let cc = Context.extendLocals locals cc
      jsExprConst (jsId n) (compileExpr cc value) (compileExpr cc body)

  | EMatch (value, cases) ->
      exMatch (compileExpr cc value)
        (List.map (compileMatchCase cc) cases)
  
  | EIf (t, c, a) ->
      jsCond (compileExpr cc t)
        (compileExpr cc c)
        (compileExpr cc a)

  | EApply (c, args) ->
      jsApp (compileExpr cc c) (List.map (compileExpr cc) args)

  | EAssert e ->
      exAssert (compileExpr cc e)

  | EAssertMatch (l, r) ->
      exAssertMatch (compileExpr cc l) (compileExpr cc r)

  | ERecord ps ->
      exRecord (List.map (compileProp cc) ps)
  
  | EUpdate (r, ps) ->
      exRecordUpdate (compileExpr cc r)
        (List.map (compileProp cc) ps)

  | EProject (r, l) ->
      exProject (compileExpr cc r) (compileLabel cc l)

  | ELambda (ps, b) ->
      let locals = List.collect paramLocals ps
      let cc = Context.extendLocals (Set.ofList locals) cc
      exArityCheck (jsFloat (double (List.length ps)))
        (jsAnonFun
          (List.map (compileParam cc) ps)
          [jsReturn (compileExpr cc b)])

  | EVariable n ->
      if Context.isLazy n cc then
        exForce (jsId n)
      else if Context.isLocal n cc then
        jsId n
      else
        exScopeSend (jsStr n)

  | EHole ->
      failwithf "Hole outside of application"

  | ECons (hd, tl) ->
      exCons
        (compileExpr cc hd)
        (compileExpr cc tl)
  
  | EEmpty ->
      exEmpty

  | ETuple xs ->
      jsArray (List.map (compileExpr cc) xs)

  | ELiteral l ->
      compileLiteral l

and compileLiteral l =
  match l with
  | LFloat d -> jsFloat d
  | LInteger s -> jsBigInt s
  | LString s -> jsStr s
  | LBoolean b -> jsBool b
  | LNothing -> jsNull
  
and compileAlias cc alias =
  match alias with
  | AName n -> jsId n
  | AFresh b -> jsRawId (Context.fresh cc b)

and compilePropDef (prop:Property) =
  jsStr prop.field

and compileCase (c:UnionCase) =
  defCase c.tag (List.map compilePropDef c.properties)

and compileParam cc (c:Parameter) =
  jsId (compileNamePattern cc c.name)

and compileNamePattern cc n =
  match n with
  | NPName n -> n
  | NPIgnore -> Context.freshIgnore cc

and compileProp cc (prop:PropertyExpr) =
  (
    compileLabel cc prop.field,
    compileExpr cc prop.value
  )

and compileLabel cc label =
  match label with
  | LELabel n -> jsStr n
  | LEDynamic e -> compileExpr cc e

and compileMatchCase cc (c:MatchCase) =
  let locals = patternLocals c.pattern
  let cc1 = Context.extendLocals (Set.ofList locals) cc
  let ps = jsRecord (List.map compileMatchBind locals)
  jsArray [
    compilePattern cc c.pattern
    jsAnonFun [ps] [jsReturn (compileExpr cc1 c.guard)]
    jsAnonFun [ps] [jsReturn (compileExpr cc1 c.body)]
  ]

and compileMatchBind name =
  (jsStr name), (jsId name)


and compilePattern cc p =
  match p with
  | PBind n -> 
      pBind (jsStr (compileNamePattern cc n))

  | POuterBind (p, n) ->
      pOuterBind (compilePattern cc p) (jsStr (compileNamePattern cc n))
  
  | PContract (p, e) ->
      pContract (compilePattern cc p) (compileExpr cc e)
   
  | PLiteral l ->
      pEqual (compileLiteral l)

  | PCons (hd, tl) ->
      pCons
        (compilePattern cc hd)
        (compilePattern cc tl)

  | PEmpty ->
      pEmpty

  | PTuple xs ->
      pTuple (List.map (compilePattern cc) xs)

  | PRecord ps ->
      pRecord (List.map (compilePropPattern cc) ps)

  | PExtractor (r, ps) ->
      pExtractor (compileExpr cc r)
        (List.map (compilePropPattern cc) ps)

  | PAnything ->
      pAnything

and compilePropPattern cc (prop:PropertyPattern) =
  jsArray [
    compileLabel cc prop.field
    compilePattern cc prop.pattern
  ]


//== Generator entry-point
let generateFile file =
  let cc = Context.empty
  let m = compileFile cc file
  prelude + "\n" + (Js.generateExpr m)