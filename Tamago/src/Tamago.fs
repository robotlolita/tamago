module Tamago.Compiler

open Tamago.Language
open Tamago.Compiler

let parse = Parsing.parse
let generate = Codegen.Generator.generateFile