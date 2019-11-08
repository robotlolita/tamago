module Tamago.Language.Parsing

open Fohm.Generated

let private tryParse source options = Tamago.parse "File" source options

let parse source =
  match tryParse source { filename = None } with
  | Ok v -> v
  | Error e -> failwith e