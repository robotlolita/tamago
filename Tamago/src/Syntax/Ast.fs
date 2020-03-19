module Tamago.Language.Ast

type File = {
  ``namespace``: Namespace
  definitions: Statement[]
}

and Statement =
  | SUse of Namespace * alias:Name
  | SOpen of Expression * Binding[]
  | SDefine of Name * Expression
  | SFunction of Name * Parameter[] * Statement[]
  | SData of Name * DataDefinition
  | SUnion of Name * DataDefinition[]
  | STest of description:string * Statement[]
  | SHandler of name:string * MatchCase[]
  | SLet of Name * Expression
  | SLetMutable of Name * Expression
  | SCond of CondClause[] * Statement[]
  | SAssign of Name * Expression
  | SExpression of Expression
  | SAssert of Expression * message:string
  | SMatch of Expression * MatchCase[]
  | SFor of Name * Expression * Statement[]
  | SRepeat of Statement[]
  | SRepeatWith of RepeatBinding[] * Statement[]
  | SContinueWith of RepeatBinding[]
  | SBreak
  | SResumeWith of Expression
  | SProtocol of name:string * types:string[] * ProtocolDefinition[]
  | SImplement of Expression * types:Expression[] * ImplementStatement[]
  | SUnreachable of message:string
  | SMulti of Statement[]
  | SReturn of Expression

and DataDefinition = 
  | DDRecord of Tag * Field[]

and Expression =
  | EApply of callee:Expression * arguments:Expression[]
  | ELambda of Parameter[] * Statement[]
  | EVariable of Name
  | ETuple of Expression[]
  | ECons of Expression * Expression
  | EEmpty
  | EBlock of Statement[]
  | ERecord of PairExpression[]
  | EUpdate of Expression * PairExpression[]
  | EProject of Expression * Label
  | ECheck of contract:Expression * Expression
  | EType of Expression
  | EProcess of Statement[]
  | EDo of Statement[]
  | EHandle of Statement[] * MatchCase[]
  | EPerform of Expression
  | EYield of Expression
  | EYieldAll of Expression
  | ELiteral of Literal
  | EHole

and Literal =
  | LFloat of double
  | LInteger of string
  | LText of string
  | LBoolean of bool
  | LNothing

and CondClause = Expression * Statement[]
  
and Parameter = Name
and Namespace = string
and Tag = string

and Field =
  | FRequired of string
  | FOptional of string * _default:Expression

and Name =
  | NStatic of string
  | NFresh of FreshBind
  | NIgnore

and FreshBind(name: string) =
  let mutable generated : string option = None
  member __.Name = name
  member __.Get(generate : unit -> string) =
    match generated with
    | Some value -> value
    | None ->
        let value = generate()
        generated <- Some value
        value


and Label =
  | LStatic of string
  | LDynamic of Expression

and PairExpression = Label * Expression

and MatchCase =
  | MCGuarded of Pattern * guard:Expression * Statement[]
  | MCUnguarded of Pattern * Statement[]
  | MCDefault of Statement[]

and Pattern =
  | PAnything
  | PBind of Name
  | PBindAs of Pattern * Name
  | PLiteral of Literal
  | PEqual of Expression
  | PTuple of Pattern[]
  | PCons of Pattern * Pattern
  | PEmpty
  | PRecord of PairPattern[]
  | PCheck of Pattern * Expression
  | PExtractRecord of Expression * PairPattern[]

and PairPattern = Label * Pattern

and Binding =
  | BFunction of arity:int * name:string
  | BVariable of string

and RepeatBinding = Name * Expression

and ProtocolDefinition =
  | PDRequiredMethod of string * Parameter[]
  | PDOptionalMethod of string * Parameter[] * Statement[]
  | PDRequires of Expression * types:string[]

and ImplementStatement =
  | ISMethod of string * Parameter[] * Statement[]
  | ISStatement of Statement

let file ns defs : File =
  {
    ``namespace`` = ns
    definitions = defs
  }

let makeFun (name, parameters) statements =
  match statements with
  | [|SExpression (EBlock x)|] -> SFunction (name, parameters, x)
  | _ -> SFunction (name, parameters, statements)

let prefixName op = NStatic (op + " _")
let unaryName op = NStatic ("_ " + op)

let unarySig op self =
  (unaryName op, [|self|])


let binaryName op = NStatic ("_ " + op + " _")

let binarySig op l r =
  (binaryName op, [|l; r|])

let selfToPrefix self =
  match self with
  | Some _ -> "_:"
  | None -> ""

let parseKeywords self keywords =
  let prefix = selfToPrefix self
  let kws = Seq.map fst keywords
  let args = Array.map snd keywords
  let fullName = prefix + (String.concat "" kws)
  (fullName, Array.append (Option.toArray self) args)

let keywordSig keywords self =
  let (name, args) = parseKeywords self keywords
  (NStatic name, args)

let parameter n = n

let keywordApply self keywords =
  let (name, args) = parseKeywords self keywords
  EApply ((EVariable (NStatic name)), args)

let fresh name =
  FreshBind name

let caseName c =
  match c with
  | DDRecord (n, _) -> n

let sigToBinding (name, args) =
  match name with
  | NStatic name -> (Array.length args, name)
  | _ -> failwithf "internal: fresh name in binding"

let sigToName (name, _) =
  match name with
  | NStatic name -> name
  | _ -> failwithf "internal: fresh name in static signature"

let requiredProtoDef (NStatic name, parameters) =
  PDRequiredMethod (name, parameters)

let optionalProtoDef (NStatic name, parameters) body =
  PDOptionalMethod (name, parameters, body)

let implDef (NStatic name, parameters) body =
  ISMethod (name, parameters, body)

let makeUse ns alias bindings =
  match bindings with
  | [||] -> SUse (ns, alias)
  | _ -> SMulti [|
           SUse (ns, alias)
           SOpen (EVariable alias, bindings)
         |]

let assertMatch expr pattern =
  let bind = NFresh (fresh "assert")
  EBlock [|
    SLet (bind, expr)
    SMatch (EVariable bind, [|
      MCUnguarded (pattern, [|SExpression (ELiteral (LBoolean true))|])
    |])
  |]

let rec replaceHoles expr =
  match expr with
  | EApply(c, args) ->
      EApply(replaceHoles c, Array.map replaceHoles args)
  | EHole ->
      EVariable (NFresh (fresh "hole"))
  | _ ->
      expr

let (+++) a b = Seq.append a b

let rec collectHoles expr =
  match expr with
  | EApply(c, args) ->
      collectHoles c +++ (Seq.collect collectHoles args)
  | EVariable (NFresh b) when b.Name = "hole" ->
      Seq.ofList [(NFresh b)]
  | _ ->
      Seq.empty
  
let makeLambda expr =
  let expr' = replaceHoles expr
  let ps = collectHoles expr'
  ELambda (Array.ofSeq ps, [|SExpression expr'|])

let makeListPattern hd tl =
  Seq.foldBack (fun a b -> PCons(a, b))
               hd
               tl

let makeList hd tl =
  Seq.foldBack (fun a b -> ECons (a, b))
               hd
               tl

