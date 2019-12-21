module Tamago.Compiler

open Tamago.Language

let parse = Parsing.parse
let prettyPrint = PrettyPrint.printFile >> PrettyPrint.documentToString