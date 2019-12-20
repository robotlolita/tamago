module Tamago.Language.Ast

type File = {
  ``namespace``: Namespace
  definitions: Declaration list
}

and Declaration =
  | DImport of Namespace * Alias
  | DRecord of Name * Property list
  | DUnion of Name * UnionCase list
  | DDefine of Name * Expression
  | DModule of Name * Declaration list
  | DTest of description:string * Expression
  | DFunction of Name * Parameter list * Expression
  | DMulti of Declaration list

and Expression =
  | ESequence of Expression * Expression
  | ELet of NamePattern * value:Expression * body:Expression
  | EMatch of Expression * MatchCase list
  | EIf of test:Expression * consequent:Expression * alternate:Expression
  | EApply of callee:Expression * arguments:Expression list
  | EAssert of Expression
  | EAssertMatch of Expression * Expression
  | ERecord of PropertyExpr list
  | EUpdate of object:Expression * PropertyExpr list
  | EProject of object:Expression * LabelExpr
  | ELambda of Parameter list * Expression
  | EVariable of Name
  | EHole
  | ECons of head:Expression * tail:Expression
  | EEmpty
  | ETuple of Expression list
  | ELiteral of Literal

and Literal =
  | LFloat of double
  | LInteger of string
  | LString of string
  | LBoolean of bool
  | LNothing

and Alias =
  | AName of string
  | AFresh of FreshBind

and NamePattern =
  | NPName of string
  | NPIgnore

and Property = {
  field: Name
}

and UnionCase = {
  tag: Name
  properties: Property list
}

and Parameter = {
  name: NamePattern
}

and MatchCase = {
  pattern: Pattern
  guard: Expression
  body: Expression
}

and Pattern =
  | PBind of NamePattern
  | POuterBind of Pattern * NamePattern
  | PContract of Pattern * Expression
  | PLiteral of Literal
  | PCons of head:Pattern * tail:Pattern
  | PTuple of Pattern list
  | PRecord of PropertyPattern list
  | PExtractor of object:Expression * PropertyPattern list
  | PAnything

and PropertyPattern = {
  field: LabelExpr
  pattern: Pattern
}

and PropertyExpr = {
  field: LabelExpr
  value: Expression
}

and LabelExpr =
  | LELabel of Name
  | LEDynamic of Expression  // must evaluate to a symbol

and Namespace = Name list
and Name = string
and Id = string

and FreshBind(name: string) =
  member __.Name = name


let file ns defs : File =
  {
    ``namespace`` = ns
    definitions = defs
  }

let property name : Property =
  {
    field = name
  }

let unionCase tag props : UnionCase =
  {
    tag = tag
    properties = props
  }

let parameter name : Parameter =
  {
    name = name
  }

let matchCase pattern guard body : MatchCase =
  {
    pattern = pattern
    guard = guard
    body = body
  }

let matchCase0 pattern body =
  let guard = ELiteral (LBoolean true)
  matchCase pattern guard body

let propertyPattern field pattern : PropertyPattern =
  {
    field = field
    pattern = pattern
  }

let propertyExpr field value : PropertyExpr =
  {
    field = field
    value = value
  }

let fresh name =
  FreshBind name