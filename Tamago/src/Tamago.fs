module Tamago.Compiler

open Tamago.Language
open Tamago.Compiler

let parse = Parsing.parse
let prettyPrint = PrettyPrint.printFile >> PrettyPrint.documentToString
let generate = Codegen.Generator.generateFile