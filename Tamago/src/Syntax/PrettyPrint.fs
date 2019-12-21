module Tamago.Language.PrettyPrint

open Tamago.Language.Ast
open Tamago.FsharpUtils

type Document =
  | Text of string
  | Flow of Document list
  | Line of Document
  | Block of Document

let documentToString doc =
  let pad n = String.replicate (n * 2) " "
  let rec go indent doc =
    match doc with
    | Text s -> s
    | Flow xs -> String.concat "" (List.map (go indent) xs)
    | Line x -> "\n" + (pad indent) + (go indent x)
    | Block x -> go (indent + 1) x
  in go 0 doc

let text x = Text x
let flow xs = Flow xs
let line x = Line x
let stack xs = flow (List.map line xs)
let block xs = Block (stack xs)
let br = line (text "")
let skipBlock xs = block (intersperse br xs)
let spread xs = intersperse (text " ") xs
let between a b x = flow [a; x; b]
let braces = between (text "{") (text "}")
let parens = between (text "(") (text ")")
let brackets = between (text "[") (text "]")
let commaSep xs = (flow (intersperse (text ", ") xs))
let record xs = braces (commaSep xs)
let tuple xs = parens (commaSep xs)
let list xs = brackets (commaSep xs)

let (--) a b = flow [a; text " "; b]
let (|-) a b = flow [a; b]
let (@-) a b = flow [a; line b]
let (@@) a bs = flow [a; block bs]
let (!) a = text a

let rec printFile (f:File) =
  !"module" -- printNamespace f.``namespace``
    |- br
    |- flow (intersperse br (List.map (printDeclaration >> line) f.definitions))


and printDeclaration decl =
  match decl with
  | DImport (ns, a) ->
      !"use" -- printNamespace ns -- !"as" -- printAlias a
  | DRecord (n, ps) ->
      !"record" -- text n -- record (List.map printPropDef ps)
  | DUnion (n, cs) ->
      !"union" -- text n -- !"is"
        |- block (List.map printCase cs)
      @- !"end"
  | DDefine (n, e) ->
      !"define" -- text n -- !"="
        |- block [printExpr e]
  | DModule (n, ds) ->
      !"module" -- text n -- !"is" 
        |- skipBlock (List.map printDeclaration ds)
      @- !"end"
  | DTest (d, e) ->
      !"test" -- text d -- !"is"
        |- block [printExpr e]
      @- !"end"
  | DFunction (n, ps, e) ->
      !"function" -- text n -- tuple (List.map printParam ps) -- !"=" 
        |- block [printExpr e]
  | DMulti (ds) ->
      stack (List.map printDeclaration ds)

and printNamespace ns =
  text (String.concat "." ns)

and printAlias alias =
  match alias with
  | AName n -> text n
  | AFresh b -> text ("$" + b.Name)

and printPropDef prop =
  text prop.field

and printCase c =
  !"record" -- text c.tag -- record (List.map printPropDef c.properties)

and printParam p =
  printNamePattern p.name

and printNamePattern np =
  match np with
  | NPName s -> text s
  | NPIgnore -> text "_"

and printExpr e =
  match e with
  | ESequence (a, b) ->
      printExpr a |- !";" |- line (printExpr b)
  | ELet (n, v, b) ->
      !"let" -- printNamePattern n -- !"=" -- printExpr v -- !"in"
      @- (printExpr b)
  | EMatch (e, cs) ->
      !"match" -- printExpr e -- !"with"
        |- block (List.map printMatchCase cs)
      @- line !"end"
  | EIf (t, c, a) ->
      !"if" -- printExpr t -- !"then"
        |- block [printExpr c]
      @- !"else"
        |- block [printExpr a]
  | EApply (c, a) ->
      match c with
      | EVariable n when isBinaryOperator n ->
          let [a1; a2] = a
          parens (maybeParens a1 -- text n -- maybeParens a2)
      | EVariable n when isUnaryOperator n ->
          let [a] = a
          parens (text n -- maybeParens a)
      | _ ->
          maybeParens c |- tuple (List.map printExpr a)
  | EAssert e ->
      !"assert" -- printExpr e
  | EAssertMatch (a, b) ->
      maybeParens a -- !"==>" -- maybeParens b
  | ERecord ps ->
      record (List.map printProp ps)
  | EUpdate (r, ps) ->
      maybeParens r -- record (List.map printProp ps)
  | EProject (r, l) ->
      maybeParens r |- !"." |- printLabel l
  | ELambda (ps, e) ->
      tuple (List.map printParam ps) -- !"=>" -- maybeParens e
  | EVariable n ->
      text n
  | EHole ->
      text "_"
  | ECons (hd, tl) ->
      printCons (List.map printExpr hd) (printExpr tl)
  | EList xs ->
      list (List.map printExpr xs)
  | ETuple xs ->
      tuple (List.map printExpr xs)
  | ELiteral l ->
      printLiteral l

and printLiteral l =
  match l with
  | LFloat f -> text (sprintf "%f" f)
  | LInteger i -> text i
  | LString s -> text (sprintf "\"%s\"" s)
  | LBoolean b -> text (sprintf "%b" b)
  | LNothing -> text "nothing"

and printMatchCase mc =
  !"|" -- printPattern mc.pattern -- !"when" -- printExpr mc.guard -- !"=>"
    |- block [(printExpr mc.body)]

and printPattern p =
  match p with
  | PBind n -> printNamePattern n
  | POuterBind (p, n) -> printPattern p -- !"as" -- printNamePattern n
  | PContract (p, e) -> printPattern p -- !"::" -- maybeParens e
  | PLiteral l -> printLiteral l
  | PCons (hd, tl) -> printCons (List.map printPattern hd) (printPattern tl)
  | PList ps -> list (List.map printPattern ps)
  | PTuple ps -> tuple (List.map printPattern ps)
  | PRecord ps -> record (List.map printPropPattern ps)
  | PExtractor (o, ps) -> maybeParens o -- record (List.map printPropPattern ps)
  | PAnything -> !"_"

and printCons hd tl =
  brackets (flow [commaSep hd; !" ..."; tl])

and printPropPattern p =
  printLabel p.field -- !"=" -- printPattern p.pattern

and printProp p =
  printLabel p.field -- !"=" -- printExpr p.value

and printLabel l =
  match l with
  | LELabel s -> text s
  | LEDynamic e -> parens (printExpr e)

and maybeParens expr =
  match expr with
  | EHole
  | EVariable _
  | ERecord _
  | ELiteral _
  | EList _
  | ECons _ 
  | EProject _ ->
      printExpr expr
  | _ ->
      parens (printExpr expr)