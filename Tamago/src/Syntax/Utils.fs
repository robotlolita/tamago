module Tamago.Language.Syntax.Utils

open Tamago.Language.Ast
open Fable.Core
open Fable.Core.JsInterop

let recordToVariant x =
  match x with
  | DRecord (name, props) -> unionCase name props
  | _ -> failwithf "Not a record %A" x

let list xs =
  Array.toList xs

let (+++) xs ys =
  List.append xs ys

let makeFunDeclaration (name, args) body maybeTest =
  match maybeTest with
  | None -> DFunction (name, args, body)
  | Some test -> 
      DMulti [
        DFunction (name, args, body)
        DTest (name, test)
      ]

let parseKws ps keywords =
  let prefix = if List.isEmpty ps then "" else "_:"
  let kws = List.map fst keywords
  let args = List.map snd keywords
  let fullName = prefix + (String.concat "" kws)
  (fullName, args)

let makeSig parameters keywords =
  let (name, args) = parseKws parameters keywords
  (name, parameters +++ args)

let makeLet pattern value body =
  match pattern with
  | PBind n -> ELet (n, value, body)
  | PAnything -> ELet (NPIgnore, value, body)
  | _ -> EMatch (value, [matchCase0 pattern body])

let projectPath path =
  match path with
  | [name] -> EVariable name
  | obj :: path ->
      List.fold
        (fun obj f -> EProject (obj, LELabel f))
        (EVariable obj)
        path
  | [] -> failwithf "Empty path in projection"

let makeApp args (ns, keywords) =
  let (name, args0) = parseKws args keywords
  let args = args +++ args0
  match ns with
  | None -> EApply ((EVariable name), args)
  | Some ns ->
      EApply (projectPath (ns +++ [name]), args)

let cons a b = ECons (a, b)

let listSpreadToCons hd tl =
  List.foldBack cons hd tl

let listToCons xs =
  List.foldBack cons xs EEmpty

let pcons a b = PCons (a, b)

let patternSpreadToCons hd tl =
  List.foldBack pcons hd tl

let patternListToCons xs =
  List.foldBack pcons xs PEmpty

[<Emit("Number($0)")>]
let parseNumber s : double = jsNative

[<Emit("JSON.parse($0)")>]
let parseJson s : string = jsNative

let parseString (s:string) = parseJson ((s.Replace("\r\n", "\\n")).Replace("\n", "\\n"))