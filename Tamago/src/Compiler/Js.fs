module Tamago.Compiler.Codegen.Js

open System.Text.RegularExpressions
open Fable.Core

[<Emit("JSON.stringify($0)")>]
let toJsonString s : string = jsNative


type JsStmt =
  | JsConst of JsExpr * JsExpr
  | JsReturn of JsExpr
  | JsExpr of JsExpr
  | JsMulti of JsStmt list

and JsExpr =
  | JsComma of JsExpr * JsExpr
  | JsApply of JsExpr * JsExpr list
  | JsId of string
  | JsMember of JsExpr * string
  | JsCond of JsExpr * JsExpr * JsExpr
  | JsFun of string * JsExpr list * JsStmt list
  | JsRecord of (JsExpr * JsExpr) list
  | JsArray of JsExpr list
  | JsBigInt of string
  | JsFloat of double
  | JsBool of bool
  | JsString of string
  | JsNull


let private toSafeId n =
  match n with
  | ">=" -> "$gte"
  | ">" -> "$gt"
  | "<=" -> "$lte"
  | "<" -> "$lt"
  | "===" -> "$eq"
  | "=/=" -> "$neq"
  | ">>" -> "$composer"
  | "<<" -> "$composel"
  | "++" -> "$concat"
  | "+" -> "$plus"
  | "-" -> "$minus"
  | "*" -> "$times"
  | "/" -> "$divide"
  | "and" -> "$and"
  | "or" -> "$or"
  | "not" -> "$not"
  | n ->
      "_" + (n.Replace("-", "_").Replace(":", "$"))


let rec generate stmts =
  String.concat "\n" (Seq.map generateStmt stmts)

and generateStmt stmt =
  match stmt with
  | JsConst (name, value) ->
      sprintf "const %s = %s;" (generateExpr name) (generateExpr value)
  | JsReturn e ->
      sprintf "return %s;" (generateExpr e)
  | JsExpr e ->
      sprintf "%s;" (generateExpr e)
  | JsMulti xs ->
      generate xs

and generateExpr e =
  match e with
  | JsComma (l, r) ->
      sprintf "((%s), (%s))"
        (generateExpr l)
        (generateExpr r)

  | JsApply (callee, args) ->
      sprintf "(%s)(%s)"
        (generateExpr callee)
        (generateExprList args)
  
  | JsId n ->
      n
  
  | JsMember (o, n) ->
      sprintf "(%s).%s" (generateExpr o) n
  
  | JsCond (t, c, a) ->
      sprintf "((%s) ? (%s) : (%s))"
        (generateExpr t)
        (generateExpr c)
        (generateExpr a)

  | JsFun (n, ps, b) ->
      sprintf "(function %s(%s) { %s })"
        n
        (generateExprList ps)
        (generate b)

  | JsRecord (ps) ->
      sprintf "({ %s })"
        (String.concat ", " (Seq.map generatePropPair ps))

  | JsArray xs ->
      sprintf "[%s]" (generateExprList xs)
  
  | JsBigInt n ->
      sprintf "%sn" n

  | JsFloat n ->
      sprintf "%f" n
  
  | JsBool b ->
      sprintf "%b" b

  | JsString s ->
      toJsonString s

  | JsNull ->
      "null"

and generateExprList es =
  String.concat ", " (Seq.map generateExpr es)

and generatePropPair (k, v) =
  sprintf "[%s]: %s"
    (generateExpr k)
    (generateExpr v)


let jsMulti xs = JsMulti xs
let jsConst n e = JsConst (n, e)
let jsReturn e = JsReturn e
let jsExpr e = JsExpr e
let jsApp c xs = JsApply (c, xs)
let jsId n = JsId (toSafeId n)
let jsRawId n = JsId n
let jsMember o l = JsMember (o, l)
let jsCond t c a = JsCond (t, c, a)
let jsFun n ps b = JsFun (n, ps, b)
let jsAnonFun ps b = jsFun "" ps b
let jsRecord ps = JsRecord ps
let jsArray xs = JsArray xs
let jsBigInt n = JsBigInt n
let jsFloat n = JsFloat n
let jsBool b = JsBool b
let jsStr s = JsString s
let jsNull = JsNull
let jsComma a b = JsComma (a, b)
let jsExprConst n v b = jsApp (jsAnonFun [n] [jsReturn b]) [v]
let jsThunk e = jsAnonFun [] [jsReturn e]