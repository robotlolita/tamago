// This code was automatically generated from a grammar definition by Fohm.
module Fohm.Generated.Tamago

type Offset = 
  { line: int; column: int }

type OffsetRecord<'a> =
  { start: 'a; ``end``: 'a }

type Position = 
  {
    offset: unit -> OffsetRecord<int>
    position: unit -> OffsetRecord<Offset>
    sourceSlice: string
    sourceString: string
    filename: string option
  }

type Meta = 
  { source: Position; children: Position[] }

type ParseOptions =
  { filename: string option }


open Tamago.Language.Ast
open Tamago.Language.Syntax.Utils


open Fable.Core
open Fable.Core.JsInterop

[<Import("makeParser", from="./fohm-runtime.js")>]
let private makeParser (source: string, visitor: obj): obj = jsNative

let private visitor = 
  createObj [
    "File_alt0" ==> fun (meta:Meta) _0 _1 ns defs _4 ->
       file ns (list defs) 
              
    "ImportDef_alt0" ==> fun (meta:Meta) _0 ns _2 n ->
       DImport (ns, n) 
              
    "UnionDef_alt0" ==> fun (meta:Meta) _0 n _2 cs _4 ->
       DUnion (n, list cs) 
              
    "UnionCase_alt0" ==> fun (meta:Meta) r ->
       recordToVariant r 
              
    "RecordDef_alt0" ==> fun (meta:Meta) _0 n _2 ps _4 ->
       DRecord (n, ps) 
              
    "FunctionDef_alt0" ==> fun (meta:Meta) _0 s _2 e t ->
       makeFunDeclaration s e t 
              
    "FunctionTest_alt0" ==> fun (meta:Meta) _0 e ->
       e 
              
    "AliasDef_alt0" ==> fun (meta:Meta) _0 n _2 e ->
       DDefine (n, e) 
              
    "ModuleDef_alt0" ==> fun (meta:Meta) _0 n _2 defs _4 ->
       DModule (n, list defs) 
              
    "TestDef_alt0" ==> fun (meta:Meta) _0 s _2 e _4 ->
       DTest (s, e) 
              
    "Signature_alt0" ==> fun (meta:Meta) n kws ->
       makeSig [n] kws 
              
    "Signature_alt1" ==> fun (meta:Meta) kws ->
       makeSig [] kws 
              
    "Signature_alt2" ==> fun (meta:Meta) l op r ->
       (op, [l, r]) 
              
    "Signature_alt3" ==> fun (meta:Meta) op n ->
       (op, [n]) 
              
    "Signature_alt4" ==> fun (meta:Meta) s n ->
       (n, [s]) 
              
    "Param_alt0" ==> fun (meta:Meta) n ->
       parameter n 
              
    "KeywordParams_alt0" ==> fun (meta:Meta) kws ->
       list kws 
              
    "KeywordParam_alt0" ==> fun (meta:Meta) kw n ->
       (kw, n) 
              
    "SequenceExpr_alt0" ==> fun (meta:Meta) l _1 r ->
       ESequence (l, r) 
              
    "LetExpr_alt0" ==> fun (meta:Meta) _0 p _2 v _4 e ->
       makeLet p v e 
              
    "IfExpr_alt0" ==> fun (meta:Meta) _0 t _2 c _4 a ->
       EIf (t, c, a) 
              
    "PipeExpr_alt0" ==> fun (meta:Meta) l _1 r ->
       EApply (r, [l]) 
              
    "KeywordExpr_alt0" ==> fun (meta:Meta) kws ->
       makeApp [] kws 
              
    "KeywordExpr_alt1" ==> fun (meta:Meta) s kws ->
       makeApp [s] kws 
              
    "KeywordApply_alt0" ==> fun (meta:Meta) ns _1 kws ->
       (Some ns, list kws) 
              
    "KeywordApply_alt1" ==> fun (meta:Meta) kws ->
       (None, list kws) 
              
    "KeywordApplyPair_alt0" ==> fun (meta:Meta) k e ->
       (k, e) 
              
    "AssertExpr_alt0" ==> fun (meta:Meta) _0 e ->
       EAssert e 
              
    "AssertExpr_alt1" ==> fun (meta:Meta) l _1 r ->
       EAssertMatch (l, r) 
              
    "BinaryExpr_alt0" ==> fun (meta:Meta) l op r ->
       EApply (EVariable op, [l; r]) 
              
    "UnaryExpr_alt0" ==> fun (meta:Meta) op s ->
       EApply (EVariable op, [s]) 
              
    "UnaryExpr_alt1" ==> fun (meta:Meta) s n ->
       EApply (projectPath n, [s]) 
              
    "ApplyExpr_alt0" ==> fun (meta:Meta) c ps ->
       EApply (c, ps) 
              
    "ApplyExpr_alt1" ==> fun (meta:Meta) o ps ->
       EUpdate (o, ps) 
              
    "PositionalArgumentList_alt0" ==> fun (meta:Meta) _0 ps _2 ->
       ps 
              
    "RecordProperties_alt0" ==> fun (meta:Meta) _0 ps _2 ->
       ps 
              
    "RecordPair_alt0" ==> fun (meta:Meta) l _1 e ->
       propertyExpr l e 
              
    "MemberExpr_alt0" ==> fun (meta:Meta) o _1 n ->
       EProject (o, LELabel n) 
              
    "MemberExpr_alt1" ==> fun (meta:Meta) o _1 _2 n _4 ->
       EProject (o, LEDynamic n) 
              
    "PrimaryExpr_alt2" ==> fun (meta:Meta) l ->
       ELiteral l 
              
    "PrimaryExpr_alt7" ==> fun (meta:Meta) n ->
       EVariable n 
              
    "PrimaryExpr_alt8" ==> fun (meta:Meta) _0 e _2 ->
       e 
              
    "MatchExpr_alt0" ==> fun (meta:Meta) _0 e _2 cs _4 ->
       EMatch (e, list cs) 
              
    "MatchCase_alt0" ==> fun (meta:Meta) _0 p _2 g _4 e ->
       matchCase p g e 
              
    "MatchCase_alt1" ==> fun (meta:Meta) _0 p _2 e ->
       matchCase0 p e 
              
    "Pattern_alt0" ==> fun (meta:Meta) p _1 n ->
       POuterBind (p, n) 
              
    "Pattern_alt1" ==> fun (meta:Meta) p _1 m ->
       PContract (p, m) 
              
    "Pattern_alt2" ==> fun (meta:Meta) l ->
       PLiteral l 
              
    "Pattern_alt3" ==> fun (meta:Meta) _0 hd _2 _3 tl _5 ->
       PCons (hd, tl) 
              
    "Pattern_alt4" ==> fun (meta:Meta) _0 ps _2 ->
       PList ps 
              
    "Pattern_alt5" ==> fun (meta:Meta) o _1 ps _3 ->
       PExtractor (o, ps) 
              
    "Pattern_alt6" ==> fun (meta:Meta) n ->
       PBind n 
              
    "Pattern_alt7" ==> fun (meta:Meta) _0 ->
       PAnything 
              
    "Pattern_alt8" ==> fun (meta:Meta) _0 p _2 ->
       p 
              
    "GroupPattern_alt0" ==> fun (meta:Meta) hd _1 tl ->
       PTuple (hd :: tl) 
              
    "PatternPair_alt0" ==> fun (meta:Meta) l _1 p ->
       propertyPattern l p 
              
    "PatternPair_alt1" ==> fun (meta:Meta) n ->
       propertyPattern (LELabel n) (PBind (NPName n)) 
              
    "Label_alt0" ==> fun (meta:Meta) _0 e _2 ->
       LEDynamic e 
              
    "Label_alt1" ==> fun (meta:Meta) n ->
       LELabel n 
              
    "RecordExpr_alt0" ==> fun (meta:Meta) ps ->
       ERecord ps 
              
    "LambdaExpr_alt0" ==> fun (meta:Meta) _0 ps _2 _3 e ->
       ELambda (ps, e) 
              
    "LiteralExpr_alt4" ==> fun (meta:Meta) n ->
       LNothing 
              
    "ListExpr_alt0" ==> fun (meta:Meta) _0 hd _2 _3 tl _5 ->
       ECons (hd, tl) 
              
    "ListExpr_alt1" ==> fun (meta:Meta) _0 xs _2 ->
       EList xs 
              
    "TupleExpr_alt0" ==> fun (meta:Meta) hd _1 tl ->
       ETuple (hd :: tl) 
              
    "HoleExpr_alt0" ==> fun (meta:Meta) _0 ->
       EHole 
              
    "Name_alt0" ==> fun (meta:Meta) n ->
       n 
              
    "Keyword_alt0" ==> fun (meta:Meta) kw ->
       kw 
              
    "RestrictedMember_alt0" ==> fun (meta:Meta) ns ->
       ns 
              
    "NamePattern_alt0" ==> fun (meta:Meta) n ->
       NPName n 
              
    "NamePattern_alt1" ==> fun (meta:Meta) _0 ->
       NPIgnore 
              
    "Float_alt0" ==> fun (meta:Meta) f ->
       LFloat (parseNumber f) 
              
    "Integer_alt0" ==> fun (meta:Meta) i ->
       LInteger i 
              
    "Boolean_alt0" ==> fun (meta:Meta) b ->
       LBoolean b 
              
    "String_alt0" ==> fun (meta:Meta) s ->
       LString (parseString s) 
              
    "List1_alt0" ==> fun (meta:Meta) x ->
       list x 
              
    "List0_alt0" ==> fun (meta:Meta) x ->
       list x 
              
    "boolean_alt0" ==> fun (meta:Meta) _0 ->
       true 
              
    "boolean_alt1" ==> fun (meta:Meta) _0 ->
       false 
              
  ]

let private primParser: obj  =
  makeParser(
    """
    Tamago {
      File =
        | Header module_ Namespace Definition* end -- alt0
              
      
      Header =
        | "%" "language" ":" "tamago" -- alt0
              
      
      Definition =
        | ImportDef -- alt0
        | RecordDef -- alt1
        | UnionDef -- alt2
        | FunctionDef -- alt3
        | AliasDef -- alt4
        | ModuleDef -- alt5
        | TestDef -- alt6
              
      
      ImportDef =
        | use_ Namespace as_ Alias -- alt0
              
      
      UnionDef =
        | union_ Name is_ UnionCase+ end_ -- alt0
              
      
      UnionCase =
        | RecordDef -- alt0
              
      
      RecordDef =
        | record_ Name "{" List0<Name, ","> "}" -- alt0
              
      
      FunctionDef =
        | function_ Signature "=" Expression FunctionTest? -- alt0
              
      
      FunctionTest =
        | where_ Expression -- alt0
              
      
      AliasDef =
        | define_ Name "=" Expression -- alt0
              
      
      ModuleDef =
        | module_ Name is_ Definition* end_ -- alt0
              
      
      TestDef =
        | test_ String do_ Expression end_ -- alt0
              
      
      Signature =
        | Param KeywordParams -- alt0
        | KeywordParams -- alt1
        | Param BinaryOp Param -- alt2
        | PrefixOp Param -- alt3
        | Param Name -- alt4
              
      
      Param =
        | NamePattern -- alt0
              
      
      KeywordParams =
        | KeywordParam+ -- alt0
              
      
      KeywordParam =
        | Keyword Param -- alt0
              
      
      ExprWithTuple =
        | TupleExpr -- alt0
        | Expression -- alt1
              
      
      Expression =
        | SequenceExpr -- alt0
              
      
      SequenceExpr =
        | SequenceExpr ";" Expression -- alt0
        | AssertExpr -- alt1
              
      
      LetExpr =
        | let_ Pattern "=" IfExpr in_ Expression -- alt0
        | IfExpr -- alt1
              
      
      IfExpr =
        | if_ PipeExpr then_ Expression else_ Expression -- alt0
        | PipeExpr -- alt1
              
      
      PipeExpr =
        | PipeExpr "|>" KeywordExpr -- alt0
        | KeywordExpr -- alt1
              
      
      KeywordExpr =
        | KeywordApply -- alt0
        | BinaryExpr KeywordApply -- alt1
        | BinaryExpr -- alt2
              
      
      KeywordApply =
        | Namespace "." KeywordApplyPair+ -- alt0
        | KeywordApplyPair+ -- alt1
              
      
      KeywordApplyPair =
        | Keyword BinaryExpr -- alt0
              
      
      AssertExpr =
        | assert_ PipeExpr -- alt0
        | PipeExpr "==>" PipeExpr -- alt1
              
      
      BinaryExpr =
        | UnaryExpr BinaryOp UnaryExpr -- alt0
        | UnaryExpr -- alt1
              
      
      UnaryExpr =
        | PrefixOp ApplyExpr -- alt0
        | ApplyExpr RestrictedMember -- alt1
        | ApplyExpr -- alt2
              
      
      ApplyExpr =
        | ApplyExpr PositionalArgumentList -- alt0
        | ApplyExpr RecordProperties -- alt1
        | MemberExpr -- alt2
              
      
      PositionalArgumentList =
        | "(" List0<Expression, ","> ")" -- alt0
              
      
      RecordProperties =
        | "{" List0<RecordPair, ","> "}" -- alt0
              
      
      RecordPair =
        | Label "=" Expression -- alt0
              
      
      MemberExpr =
        | MemberExpr "." Name -- alt0
        | MemberExpr "." "(" Expression ")" -- alt1
        | PrimaryExpr -- alt2
              
      
      PrimaryExpr =
        | MatchExpr -- alt0
        | LambdaExpr -- alt1
        | LiteralExpr -- alt2
        | RecordExpr -- alt3
        | ListExpr -- alt4
        | HoleExpr -- alt5
        | BreakExpr -- alt6
        | Name -- alt7
        | "(" ExprWithTuple ")" -- alt8
              
      
      MatchExpr =
        | match_ ExprWithTuple with_ MatchCase+ end_ -- alt0
              
      
      MatchCase =
        | "|" Pattern when_ Expression "=>" Expression -- alt0
        | "|" Pattern "=>" Expression -- alt1
              
      
      Pattern =
        | Pattern as_ NamePattern -- alt0
        | Pattern "::" MemberExpr -- alt1
        | LiteralExpr -- alt2
        | "[" List1<Pattern, ","> "," "..." Pattern "]" -- alt3
        | "[" List0<Pattern, ","> "]" -- alt4
        | MemberExpr "{" List0<PatternPair, ","> "}" -- alt5
        | NamePattern -- alt6
        | "_" -- alt7
        | "(" GroupPattern ")" -- alt8
              
      
      GroupPattern =
        | Pattern "," List1<Pattern, ","> -- alt0
        | Pattern -- alt1
              
      
      PatternPair =
        | Label "=" Pattern -- alt0
        | Name -- alt1
              
      
      Label =
        | "(" Expression ")" -- alt0
        | Name -- alt1
              
      
      RecordExpr =
        | RecordProperties -- alt0
              
      
      LambdaExpr =
        | "(" ListOf<Param, ","> ")" "=>" Expression -- alt0
              
      
      LiteralExpr =
        | Float -- alt0
        | Integer -- alt1
        | String -- alt2
        | Boolean -- alt3
        | nothing_ -- alt4
              
      
      ListExpr =
        | "[" List1<Expression, ","> "," "..." Expression "]" -- alt0
        | "[" List0<Expression, ","> "]" -- alt1
              
      
      TupleExpr =
        | Expression "," List1<Expression, ","> -- alt0
              
      
      HoleExpr =
        | hole -- alt0
              
      
      Name =
        | ~reserved id ~":" -- alt0
              
      
      Keyword =
        | keyword -- alt0
              
      
      RestrictedMember =
        | Namespace ~("." Keyword) -- alt0
              
      
      Namespace =
        | List1<Name, "."> -- alt0
              
      
      NamePattern =
        | Name -- alt0
        | hole -- alt1
              
      
      BinaryOp =
        | binary_op -- alt0
              
      
      PrefixOp =
        | prefix_op -- alt0
              
      
      Float =
        | float -- alt0
              
      
      Integer =
        | integer -- alt0
              
      
      Boolean =
        | boolean -- alt0
              
      
      String =
        | double_string -- alt0
              
      
      List1<A, B> =
        | NonemptyListOf<A, B> -- alt0
              
      
      List0<A, B> =
        | ListOf<A, B> -- alt0
              
      
      newline =
        | "\n" -- alt0
        | "\r" -- alt1
              
      
      line =
        | (~newline any)* -- alt0
              
      
      comment =
        | "//" line -- alt0
              
      
      space +=
        | comment -- alt0
              
      
      id_start =
        | letter -- alt0
              
      
      id_rest =
        | letter -- alt0
        | digit -- alt1
        | "-" -- alt2
              
      
      id =
        | id_start id_rest* -- alt0
              
      
      hole =
        | "_" -- alt0
              
      
      keyword =
        | id ":" -- alt0
              
      
      integer =
        | ("-" | "+")? digit+ -- alt0
              
      
      float =
        | ("-" | "+")? digit+ "." digit+ -- alt0
              
      
      boolean =
        | true_ -- alt0
        | false_ -- alt1
              
      
      hex_digit =
        | "a".."f" -- alt0
        | "A".."F" -- alt1
        | "0".."9" -- alt2
              
      
      escape_sequence =
        | "u" unicode_escape -- alt0
        | any -- alt1
              
      
      unicode_escape =
        | hex_digit hex_digit hex_digit hex_digit -- alt0
              
      
      string_character =
        | ~("\"" | "\\") any -- alt0
        | "\\" escape_sequence -- alt1
              
      
      double_string =
        | "\"" string_character* "\"" -- alt0
              
      
      binary_op =
        | ">=" -- alt0
        | ">>" -- alt1
        | ">" -- alt2
        | "<=" -- alt3
        | "<<" -- alt4
        | "<" -- alt5
        | "===" -- alt6
        | "=/=" -- alt7
        | "++" -- alt8
        | "+" -- alt9
        | "-" -- alt10
        | "/" -- alt11
        | "**" -- alt12
        | "*" -- alt13
        | and_ -- alt14
        | or_ -- alt15
              
      
      prefix_op =
        | not_ -- alt0
              
      
      kw<word> =
        | word ~id_rest -- alt0
              
      
      use_ =
        | kw<"use"> -- alt0
              
      
      as_ =
        | kw<"as"> -- alt0
              
      
      record_ =
        | kw<"record"> -- alt0
              
      
      module_ =
        | kw<"module"> -- alt0
              
      
      do_ =
        | kw<"do"> -- alt0
              
      
      end_ =
        | kw<"end"> -- alt0
              
      
      define_ =
        | kw<"define"> -- alt0
              
      
      let_ =
        | kw<"let"> -- alt0
              
      
      in_ =
        | kw<"in"> -- alt0
              
      
      if_ =
        | kw<"if"> -- alt0
              
      
      then_ =
        | kw<"then"> -- alt0
              
      
      else_ =
        | kw<"else"> -- alt0
              
      
      assert_ =
        | kw<"assert"> -- alt0
              
      
      true_ =
        | kw<"true"> -- alt0
              
      
      false_ =
        | kw<"false"> -- alt0
              
      
      nothing_ =
        | kw<"nothing"> -- alt0
              
      
      new_ =
        | kw<"new"> -- alt0
              
      
      and_ =
        | kw<"and"> -- alt0
              
      
      or_ =
        | kw<"or"> -- alt0
              
      
      not_ =
        | kw<"not"> -- alt0
              
      
      external_ =
        | kw<"external"> -- alt0
              
      
      match_ =
        | kw<"match"> -- alt0
              
      
      with_ =
        | kw<"with"> -- alt0
              
      
      when_ =
        | kw<"when"> -- alt0
              
      
      test_ =
        | kw<"test"> -- alt0
              
      
      for_ =
        | kw<"for"> -- alt0
              
      
      function_ =
        | kw<"function"> -- alt0
              
      
      process_ =
        | kw<"process"> -- alt0
              
      
      where_ =
        | kw<"where"> -- alt0
              
      
      yield_ =
        | kw<"yield"> -- alt0
              
      
      all_ =
        | kw<"all"> -- alt0
              
      
      break_ =
        | kw<"break"> -- alt0
              
      
      union_ =
        | kw<"union"> -- alt0
              
      
      is_ =
        | kw<"is"> -- alt0
              
      
      reserved =
        | use_ -- alt0
        | as_ -- alt1
        | record_ -- alt2
        | module_ -- alt3
        | do_ -- alt4
        | end_ -- alt5
        | define_ -- alt6
        | let_ -- alt7
        | in_ -- alt8
        | if_ -- alt9
        | then_ -- alt10
        | else_ -- alt11
        | assert_ -- alt12
        | true_ -- alt13
        | false_ -- alt14
        | nothing_ -- alt15
        | new_ -- alt16
        | and_ -- alt17
        | or_ -- alt18
        | not_ -- alt19
        | external_ -- alt20
        | match_ -- alt21
        | with_ -- alt22
        | when_ -- alt23
        | test_ -- alt24
        | for_ -- alt25
        | function_ -- alt26
        | process_ -- alt27
        | where_ -- alt28
        | yield_ -- alt29
        | all_ -- alt30
        | break_ -- alt31
        | union_ -- alt32
        | is_ -- alt33
              
    }
      
    """, 
    visitor
  )

let parse (rule: string) (source: string) (options: ParseOptions): Result<File, string> = 
  let (success, value) = !!(primParser$(source, rule, options))
  if success then Ok(!!value)
  else Error(!!value)
  