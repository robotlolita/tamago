module Tamago.Compiler.Context

open System.Collections.Generic
open Tamago.Language.Ast

type CompilationContext = {
  freshGenerator: FreshGenerator
  boundFresh: Dictionary<FreshBind, Name>
  lazyLocals: Set<Name>
  locals: Set<Name>
}

and FreshGenerator() =
  let mutable counter = 1
  member __.Next prefix =
    let suffix = counter
    counter <- counter + 1
    sprintf "$%s_%d" prefix suffix

let empty : CompilationContext =
  {
    freshGenerator = FreshGenerator()
    boundFresh = Dictionary()
    lazyLocals = Set.empty
    locals = Set.empty
  }

let freshIgnore cc =
  cc.freshGenerator.Next ""

let fresh cc (bind:FreshBind) =
  let name = cc.freshGenerator.Next bind.Name
  cc.boundFresh.Add (bind, name)
  name

let isLazy name cc =
  Set.contains name cc.lazyLocals

let isLocal name cc =
  Set.contains name cc.locals

let extendLocals locals cc =
  let newLocals = Set.union cc.locals (Set.ofSeq locals)
  { cc with locals = newLocals }

let extendLazy names cc =
  let newLazy = Set.union cc.lazyLocals (Set.ofSeq names)
  { cc with lazyLocals = newLazy }