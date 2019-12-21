module Tamago.FsharpUtils

let rec intersperse sep xs =
  match xs with
  | [] -> []
  | [x] -> [x]
  | x0 :: x1 :: xs -> x0 :: sep :: x1 :: intersperse sep xs
