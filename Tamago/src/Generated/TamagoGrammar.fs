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
open Fable.Core

[<Emit("Number($0)")>]
let parseNumber s : double = jsNative

[<Emit("JSON.parse($0)")>]
let parseJson s : string = jsNative

let parseString (s:string) = parseJson ((s.Replace("\r\n", "\\n")).Replace("\n", "\\n"))


open Fable.Core
open Fable.Core.JsInterop

[<Import("makeParser", from="./fohm-runtime.js")>]
let private makeParser (source: string, visitor: obj): obj = jsNative

let private visitor = 
  createObj [
    "File_alt0" ==> fun (meta:Meta) _0 _1 ns defs _4 ->
       file ns defs 
              
    "DeclarationList_alt0" ==> fun (meta:Meta) xs ->
       xs 
              
    "StatementList_alt0" ==> fun (meta:Meta) xs ->
      xs
              
    "Statement_alt16" ==> fun (meta:Meta) e ->
       SExpression e 
              
    "UseStatement_alt0" ==> fun (meta:Meta) _0 ns n bs _4 ->
       makeUse ns n bs 
              
    "UseStatement_alt1" ==> fun (meta:Meta) _0 ns n _3 ->
       SUse (ns, n) 
              
    "UseStatement_alt2" ==> fun (meta:Meta) _0 ns bs _3 ->
       makeUse ns (NFresh (fresh ns)) bs 
              
    "UseAlias_alt0" ==> fun (meta:Meta) _0 n ->
       NStatic n 
              
    "UseExposing_alt0" ==> fun (meta:Meta) _0 bs ->
       bs 
              
    "OpenStatement_alt0" ==> fun (meta:Meta) _0 e _2 bs _4 ->
       SOpen (e, bs) 
              
    "ExposeBinding_alt0" ==> fun (meta:Meta) s ->
       BFunction (sigToBinding s) 
              
    "ExposeBinding_alt1" ==> fun (meta:Meta) e ->
       BVariable e 
              
    "DefineStatement_alt0" ==> fun (meta:Meta) _0 n _2 e ->
       SDefine (NStatic n, e) 
              
    "FunctionStatement_alt0" ==> fun (meta:Meta) _0 s _2 _3 xs t ->
       SMulti [|makeFun s xs; t (sigToName s)|] 
              
    "FunctionStatement_alt1" ==> fun (meta:Meta) _0 s _2 e ->
       makeFun s [|SExpression e|] 
              
    "Signature_alt0" ==> fun (meta:Meta) op n ->
       (prefixName op, [|n|]) 
              
    "Signature_alt1" ==> fun (meta:Meta) l op r ->
       binarySig op l r 
              
    "Signature_alt2" ==> fun (meta:Meta) n kws ->
       keywordSig kws (Some n) 
              
    "Signature_alt3" ==> fun (meta:Meta) kws ->
       keywordSig kws None 
              
    "Signature_alt4" ==> fun (meta:Meta) s n ->
       unarySig n s 
              
    "Signature_alt5" ==> fun (meta:Meta) n _1 ps _3 ->
       (NStatic n, ps) 
              
    "Param_alt0" ==> fun (meta:Meta) n ->
       parameter n 
              
    "KeywordParam_alt0" ==> fun (meta:Meta) kw n ->
       (kw, n) 
              
    "TrailingTest_alt0" ==> fun (meta:Meta) _0 xs _2 ->
       box (fun n -> STest (n, xs)) 
              
    "TestStatement_alt0" ==> fun (meta:Meta) _0 n _2 _3 xs _5 ->
       let (LText n) = n in STest (n, xs) 
              
    "TestStatement_alt1" ==> fun (meta:Meta) _0 n _2 e ->
       let (LText n) = n in STest (n, [|SExpression e|]) 
              
    "DataStatement_alt0" ==> fun (meta:Meta) _0 c _2 ->
       SData (NStatic (caseName c), c) 
              
    "DataStatement_alt1" ==> fun (meta:Meta) _0 n _2 _3 cs _5 ->
       SUnion (NStatic n, cs) 
              
    "UnionCase_alt0" ==> fun (meta:Meta) n _1 ps _3 ->
       DDRecord (n, ps) 
              
    "FieldDef_alt0" ==> fun (meta:Meta) n _1 e ->
       FOptional (n, e) 
              
    "FieldDef_alt1" ==> fun (meta:Meta) n ->
       FRequired n 
              
    "HandlerStatement_alt0" ==> fun (meta:Meta) _0 _1 n _3 cs _5 ->
       SHandler(n, cs) 
              
    "LetStatement_alt0" ==> fun (meta:Meta) _0 n _2 e ->
       SLet (n, e) 
              
    "LetStatement_alt1" ==> fun (meta:Meta) _0 _1 n _3 e ->
       SLetMutable (n, e) 
              
    "LoopStatement_alt0" ==> fun (meta:Meta) _0 n _2 e _4 s _6 ->
       SFor (n, e, s) 
              
    "LoopStatement_alt1" ==> fun (meta:Meta) _0 _1 bs _3 s _5 ->
       SRepeatWith (bs, s) 
              
    "LoopStatement_alt2" ==> fun (meta:Meta) _0 _1 bs _3 ->
       SContinueWith bs 
              
    "LoopStatement_alt3" ==> fun (meta:Meta) _0 s _2 ->
       SRepeat s 
              
    "LoopStatement_alt4" ==> fun (meta:Meta) _0 _1 ->
       SBreak 
              
    "RepeatBinding_alt0" ==> fun (meta:Meta) n _1 e ->
       (n, e) 
              
    "IfStatement_alt0" ==> fun (meta:Meta) cs _1 a _3 ->
       SCond (cs, a) 
              
    "IfStatement_alt1" ==> fun (meta:Meta) cs _1 ->
       SCond (cs, [|SExpression (ELiteral (LNothing))|]) 
              
    "IfClauses_alt0" ==> fun (meta:Meta) _0 e _2 s cs ->
       Array.append [|e,s|] cs 
              
    "IfClauseCont_alt0" ==> fun (meta:Meta) _0 _1 e _3 s ->
       (e, s) 
              
    "MatchStatement_alt0" ==> fun (meta:Meta) _0 e _2 cs _4 ->
       SMatch (e, cs) 
              
    "MatchCase_alt0" ==> fun (meta:Meta) _0 p _2 g _4 s ->
       MCGuarded (p, g, s) 
              
    "MatchCase_alt1" ==> fun (meta:Meta) _0 p _2 s ->
       MCUnguarded (p, s) 
              
    "MatchCase_alt2" ==> fun (meta:Meta) _0 _1 _2 s ->
       MCDefault s 
              
    "Pattern_alt0" ==> fun (meta:Meta) p _1 n ->
       PBindAs (p, (NStatic n)) 
              
    "Pattern_alt1" ==> fun (meta:Meta) l ->
       PLiteral l 
              
    "Pattern_alt2" ==> fun (meta:Meta) _0 ps _2 ->
       PTuple ps 
              
    "Pattern_alt3" ==> fun (meta:Meta) _0 ps _2 ->
       PRecord ps 
              
    "Pattern_alt4" ==> fun (meta:Meta) _0 e ->
       PEqual e 
              
    "Pattern_alt5" ==> fun (meta:Meta) p _1 e ->
       PCheck (p, e) 
              
    "Pattern_alt6" ==> fun (meta:Meta) e _1 ps _3 ->
       PExtractRecord (e, ps) 
              
    "Pattern_alt7" ==> fun (meta:Meta) _0 ->
       PAnything 
              
    "Pattern_alt8" ==> fun (meta:Meta) n ->
       PBind (NStatic n) 
              
    "PairPattern_alt0" ==> fun (meta:Meta) l _1 e ->
       (LStatic l, e) 
              
    "PairPattern_alt1" ==> fun (meta:Meta) l ->
       (LStatic l, PBind (NStatic l)) 
              
    "PairPattern_alt2" ==> fun (meta:Meta) _0 k _2 _3 e ->
       (LDynamic k, e) 
              
    "AssignStatement_alt0" ==> fun (meta:Meta) n _1 e ->
       SAssign (NStatic n, e) 
              
    "AssertStatement_alt0" ==> fun (meta:Meta) _0 e ->
       SAssert (e, (Array.item 1 meta.children).sourceSlice) 
              
    "AssertStatement_alt1" ==> fun (meta:Meta) _0 s _2 ->
       let (LText x) = s in SUnreachable x 
              
    "ResumeStatement_alt0" ==> fun (meta:Meta) _0 _1 e _3 ->
       SResumeWith e 
              
    "ProtocolStatement_alt0" ==> fun (meta:Meta) _0 n _2 ps _4 _5 xs _7 ->
       SProtocol(n, ps, xs) 
              
    "ProtocolDefinition_alt0" ==> fun (meta:Meta) _0 s _2 e ->
       optionalProtoDef s [|SExpression e|] 
              
    "ProtocolDefinition_alt1" ==> fun (meta:Meta) _0 s _2 xs _4 ->
       optionalProtoDef s xs 
              
    "ProtocolDefinition_alt2" ==> fun (meta:Meta) _0 s _2 ->
       requiredProtoDef s 
              
    "ProtocolDefinition_alt3" ==> fun (meta:Meta) _0 e _2 ts _4 _5 ->
       PDRequires (e, ts) 
              
    "ImplementStatement_alt0" ==> fun (meta:Meta) _0 p _2 ps _4 _5 xs _7 ->
       SImplement (p, ps, xs) 
              
    "ImplementDefinition_alt0" ==> fun (meta:Meta) _0 s _2 e ->
       implDef s [| SExpression e |] 
              
    "ImplementDefinition_alt1" ==> fun (meta:Meta) _0 s _2 xs _4 ->
       implDef s xs 
              
    "ImplementDefinition_alt2" ==> fun (meta:Meta) s ->
       ISStatement s 
              
    "TrailingExpression_alt0" ==> fun (meta:Meta) e ->
       e 
              
    "TrailingExpression_alt1" ==> fun (meta:Meta) e _1 ->
       e 
              
    "PipeExpression_alt0" ==> fun (meta:Meta) l _1 r ->
       EApply (r, [|l|]) 
              
    "LambdaExpression_alt1" ==> fun (meta:Meta) _0 _1 ps _3 _4 e ->
       ELambda (ps, [|SExpression e|]) 
              
    "BlockLambdaExpression_alt0" ==> fun (meta:Meta) _0 _1 ps _3 _4 _5 xs _7 ->
       ELambda (ps, xs) 
              
    "AssertMatchExpression_alt0" ==> fun (meta:Meta) e _1 p ->
       assertMatch e p 
              
    "YieldExpression_alt0" ==> fun (meta:Meta) _0 _1 e ->
       EYieldAll e 
              
    "YieldExpression_alt1" ==> fun (meta:Meta) _0 e ->
       EYield e 
              
    "YieldExpression_alt2" ==> fun (meta:Meta) _0 e ->
       EPerform e 
              
    "KeywordExpression_alt0" ==> fun (meta:Meta) kws ->
       keywordApply None kws 
              
    "KeywordExpression_alt1" ==> fun (meta:Meta) s kws ->
       keywordApply (Some s) kws 
              
    "KeywordApplyPair_alt0" ==> fun (meta:Meta) k e ->
       (k, e) 
              
    "BinaryExpression_alt0" ==> fun (meta:Meta) l op r ->
       EApply (EVariable (binaryName op), [|l; r|]) 
              
    "BinaryExpression_alt1" ==> fun (meta:Meta) v _1 t ->
       ECheck (t, v) 
              
    "UnaryExpression_alt0" ==> fun (meta:Meta) op s ->
       EApply (EVariable (prefixName op), [|s|]) 
              
    "UnaryExpression_alt1" ==> fun (meta:Meta) _0 s ->
       EType s 
              
    "UnaryExpression_alt2" ==> fun (meta:Meta) s n ->
       EApply (EVariable (unaryName n), [|s|]) 
              
    "ApplyExpression_alt0" ==> fun (meta:Meta) c ps ->
       EApply (c, ps) 
              
    "ApplyExpression_alt1" ==> fun (meta:Meta) o r ->
       let (ERecord ps) = r in EUpdate (o, ps) 
              
    "MemberExpression_alt0" ==> fun (meta:Meta) o _1 k ->
       EProject (o, (LStatic k)) 
              
    "MemberExpression_alt1" ==> fun (meta:Meta) o _1 _2 k _4 ->
       EProject (o, (LDynamic k)) 
              
    "PositionalArgumentList_alt0" ==> fun (meta:Meta) _0 ps _2 ->
       ps 
              
    "PrimaryExpression_alt6" ==> fun (meta:Meta) l ->
       ELiteral l 
              
    "PrimaryExpression_alt7" ==> fun (meta:Meta) n ->
       EVariable (NStatic n) 
              
    "PrimaryExpression_alt8" ==> fun (meta:Meta) _0 ->
       EHole 
              
    "PrimaryExpression_alt9" ==> fun (meta:Meta) _0 e _2 ->
       e 
              
    "TupleExpression_alt0" ==> fun (meta:Meta) _0 xs _2 ->
       ETuple xs 
              
    "BeginExpression_alt0" ==> fun (meta:Meta) _0 xs _2 ->
       EBlock xs 
              
    "DoExpression_alt0" ==> fun (meta:Meta) _0 s _2 ->
       EDo s 
              
    "HandleExpression_alt0" ==> fun (meta:Meta) _0 s _2 cs _4 ->
       EHandle (s, cs) 
              
    "HandleCase_alt0" ==> fun (meta:Meta) _0 p _2 g _4 s ->
       MCGuarded (p, g, s) 
              
    "HandleCase_alt1" ==> fun (meta:Meta) _0 p _2 s ->
       MCUnguarded (p, s) 
              
    "HandlePattern_alt0" ==> fun (meta:Meta) e _1 ps _3 ->
       PExtractRecord (e, ps) 
              
    "ProcessExpression_alt0" ==> fun (meta:Meta) _0 s _2 ->
       EProcess s 
              
    "RecordExpression_alt0" ==> fun (meta:Meta) _0 xs _2 ->
       ERecord xs 
              
    "PairExpression_alt0" ==> fun (meta:Meta) l _1 e ->
       (LStatic l, e) 
              
    "PairExpression_alt1" ==> fun (meta:Meta) l ->
       (LStatic l, EVariable (NStatic l)) 
              
    "PairExpression_alt2" ==> fun (meta:Meta) _0 k _2 _3 e ->
       (LDynamic k, e) 
              
    "LiteralExpression_alt4" ==> fun (meta:Meta) n ->
       LNothing 
              
    "PropName_alt0" ==> fun (meta:Meta) n ->
       n 
              
    "Name_alt0" ==> fun (meta:Meta) n ->
       n 
              
    "Keyword_alt0" ==> fun (meta:Meta) kw ->
       kw 
              
    "RestrictedMember_alt0" ==> fun (meta:Meta) ns ->
       ns 
              
    "NamePattern_alt0" ==> fun (meta:Meta) n ->
       NStatic n 
              
    "NamePattern_alt1" ==> fun (meta:Meta) _0 ->
       NIgnore 
              
    "Float_alt0" ==> fun (meta:Meta) f ->
       LFloat (parseNumber f) 
              
    "Integer_alt0" ==> fun (meta:Meta) i ->
       LInteger i 
              
    "Boolean_alt0" ==> fun (meta:Meta) b ->
       LBoolean b 
              
    "String_alt0" ==> fun (meta:Meta) s ->
       LText s 
              
    "CommaList_alt0" ==> fun (meta:Meta) x _1 ->
       x 
              
    "CommaList_alt1" ==> fun (meta:Meta)  ->
       [||] 
              
    "boolean_alt0" ==> fun (meta:Meta) _0 ->
       true 
              
    "boolean_alt1" ==> fun (meta:Meta) _0 ->
       false 
              
    "string_alt0" ==> fun (meta:Meta) s ->
       parseString s 
              
  ]

let private primParser: obj  =
  makeParser(
    """
    Tamago {
      File =
        | Header namespace_ Namespace DeclarationList end -- alt0
              
      
      Header =
        | "%" "language" ":" "tamago" -- alt0
              
      
      DeclarationList =
        | Declaration+ -- alt0
              
      
      Declaration =
        | UseStatement -- alt0
        | OpenStatement -- alt1
        | DefineStatement -- alt2
        | FunctionStatement -- alt3
        | TestStatement -- alt4
        | DataStatement -- alt5
        | ProtocolStatement -- alt6
        | ImplementStatement -- alt7
        | HandlerStatement -- alt8
              
      
      StatementList =
        | Statement+ -- alt0
              
      
      Statement =
        | UseStatement -- alt0
        | OpenStatement -- alt1
        | DefineStatement -- alt2
        | FunctionStatement -- alt3
        | TestStatement -- alt4
        | DataStatement -- alt5
        | HandlerStatement -- alt6
        | LetStatement -- alt7
        | LoopStatement -- alt8
        | IfStatement -- alt9
        | MatchStatement -- alt10
        | AssignStatement -- alt11
        | AssertStatement -- alt12
        | ResumeStatement -- alt13
        | ProtocolStatement -- alt14
        | ImplementStatement -- alt15
        | TrailingExpression -- alt16
              
      
      UseStatement =
        | use_ Namespace UseAlias UseExposing ";" -- alt0
        | use_ Namespace UseAlias ";" -- alt1
        | use_ Namespace UseExposing ";" -- alt2
              
      
      UseAlias =
        | as_ Name -- alt0
              
      
      UseExposing =
        | exposing_ CommaList<ExposeBinding> -- alt0
              
      
      OpenStatement =
        | open_ Expression exposing_ CommaList<ExposeBinding> ";" -- alt0
              
      
      ExposeBinding =
        | Signature -- alt0
        | Name -- alt1
              
      
      DefineStatement =
        | define_ Name "=" TrailingExpression -- alt0
              
      
      FunctionStatement =
        | define_ Signature "=" begin_ StatementList TrailingTest -- alt0
        | define_ Signature "=" TrailingExpression -- alt1
              
      
      Signature =
        | PrefixOp Param -- alt0
        | Param BinaryOp Param -- alt1
        | Param KeywordParams -- alt2
        | KeywordParams -- alt3
        | Param Name -- alt4
        | Name "(" CommaList<Param> ")" -- alt5
              
      
      Param =
        | NamePattern -- alt0
              
      
      KeywordParams =
        | KeywordParam+ -- alt0
              
      
      KeywordParam =
        | Keyword Param -- alt0
              
      
      TrailingTest =
        | where_ StatementList end_ -- alt0
              
      
      TestStatement =
        | test_ String "=" begin_ StatementList end_ -- alt0
        | test_ String "=" TrailingExpression -- alt1
              
      
      DataStatement =
        | data_ UnionCase ";" -- alt0
        | data_ Name "=" "|"? NonemptyListOf<UnionCase, "|"> ";" -- alt1
              
      
      UnionCase =
        | Name "{" CommaList<FieldDef> "}" -- alt0
              
      
      FieldDef =
        | Name "=" Expression -- alt0
        | Name -- alt1
              
      
      HandlerStatement =
        | default_ handler_ Name with_ HandleCase+ end_ -- alt0
              
      
      LetStatement =
        | let_ NamePattern "=" TrailingExpression -- alt0
        | let_ mutable_ NamePattern "=" TrailingExpression -- alt1
              
      
      LoopStatement =
        | for_ NamePattern in_ Expression begin_ StatementList end_ -- alt0
        | repeat_ with_ CommaList<RepeatBinding> begin_ StatementList end_ -- alt1
        | continue_ with_ CommaList<RepeatBinding> ";" -- alt2
        | repeat_ StatementList end_ -- alt3
        | break_ ";" -- alt4
              
      
      RepeatBinding =
        | NamePattern "=" Expression -- alt0
              
      
      IfStatement =
        | IfClauses else_ StatementList end_ -- alt0
        | IfClauses end_ -- alt1
              
      
      IfClauses =
        | if_ Expression then_ StatementList IfClauseCont* -- alt0
              
      
      IfClauseCont =
        | else_ if_ Expression then_ StatementList -- alt0
              
      
      MatchStatement =
        | match_ Expression with_ MatchCase+ end_ -- alt0
              
      
      MatchCase =
        | "|" Pattern when_ Expression "=>" StatementList -- alt0
        | "|" Pattern "=>" StatementList -- alt1
        | "|" default_ "=>" StatementList -- alt2
              
      
      Pattern =
        | Pattern as_ Name -- alt0
        | LiteralExpression -- alt1
        | "[" CommaList<Pattern> "]" -- alt2
        | "{" CommaList<PairPattern> "}" -- alt3
        | "^" Expression -- alt4
        | Pattern is_ MemberExpression -- alt5
        | MemberExpression "{" CommaList<PairPattern> "}" -- alt6
        | hole -- alt7
        | Name -- alt8
              
      
      PairPattern =
        | Name "=" Pattern -- alt0
        | Name -- alt1
        | "(" Expression ")" "=" Pattern -- alt2
              
      
      AssignStatement =
        | Name "<-" TrailingExpression -- alt0
              
      
      AssertStatement =
        | assert_ TrailingExpression -- alt0
        | unreachable_ String ";" -- alt1
              
      
      ResumeStatement =
        | resume_ with_ Expression ";" -- alt0
              
      
      ProtocolStatement =
        | protocol_ Name "(" CommaList<Name> ")" with_ ProtocolDefinition+ end_ -- alt0
              
      
      ProtocolDefinition =
        | method_ Signature "=" TrailingExpression -- alt0
        | method_ Signature begin_ StatementList end_ -- alt1
        | method_ Signature ";" -- alt2
        | requires_ MemberExpression "(" CommaList<Name> ")" ";" -- alt3
              
      
      ImplementStatement =
        | implement_ MemberExpression "(" CommaList<Expression> ")" with_ ImplementDefinition+ end_ -- alt0
              
      
      ImplementDefinition =
        | method_ Signature "=" TrailingExpression -- alt0
        | method_ Signature begin_ StatementList end_ -- alt1
        | Statement -- alt2
              
      
      TrailingExpression =
        | AnyBlockExpression -- alt0
        | Expression ";" -- alt1
              
      
      Expression =
        | PipeExpression -- alt0
              
      
      PipeExpression =
        | PipeExpression "|>" AssertMatchExpression -- alt0
        | AssertMatchExpression -- alt1
        | LambdaExpression -- alt2
              
      
      LambdaExpression =
        | BlockLambdaExpression -- alt0
        | fun_ "(" CommaList<Param> ")" "->" Expression -- alt1
              
      
      BlockLambdaExpression =
        | fun_ "(" CommaList<Param> ")" "->" begin_ StatementList end_ -- alt0
              
      
      AssertMatchExpression =
        | YieldExpression "==>" Pattern -- alt0
        | YieldExpression -- alt1
              
      
      YieldExpression =
        | yield_ all_ KeywordExpression -- alt0
        | yield_ KeywordExpression -- alt1
        | "!" KeywordExpression -- alt2
        | KeywordExpression -- alt3
              
      
      KeywordExpression =
        | KeywordApply -- alt0
        | BinaryExpression KeywordApply -- alt1
        | BinaryExpression -- alt2
              
      
      KeywordApply =
        | KeywordApplyPair+ -- alt0
              
      
      KeywordApplyPair =
        | Keyword BinaryExpression -- alt0
              
      
      BinaryExpression =
        | UnaryExpression BinaryOp UnaryExpression -- alt0
        | UnaryExpression is_ UnaryExpression -- alt1
        | UnaryExpression -- alt2
              
      
      UnaryExpression =
        | PrefixOp ApplyExpression -- alt0
        | type_ ApplyExpression -- alt1
        | ApplyExpression Name -- alt2
        | ApplyExpression -- alt3
              
      
      ApplyExpression =
        | ApplyExpression PositionalArgumentList -- alt0
        | ApplyExpression RecordExpression -- alt1
        | MemberExpression -- alt2
              
      
      MemberExpression =
        | MemberExpression "." PropName -- alt0
        | MemberExpression "." "(" Expression ")" -- alt1
        | PrimaryExpression -- alt2
              
      
      PositionalArgumentList =
        | "(" CommaList<Expression> ")" -- alt0
              
      
      PrimaryExpression =
        | TupleExpression -- alt0
        | RecordExpression -- alt1
        | BeginExpression -- alt2
        | DoExpression -- alt3
        | HandleExpression -- alt4
        | ProcessExpression -- alt5
        | LiteralExpression -- alt6
        | Name -- alt7
        | hole -- alt8
        | "(" Expression ")" -- alt9
              
      
      TupleExpression =
        | "[" CommaList<Expression> "]" -- alt0
              
      
      BeginExpression =
        | begin_ StatementList end_ -- alt0
              
      
      DoExpression =
        | do_ StatementList end_ -- alt0
              
      
      HandleExpression =
        | handle_ StatementList with_ HandleCase+ end_ -- alt0
              
      
      HandleCase =
        | "|" HandlePattern when_ Expression "=>" StatementList -- alt0
        | "|" HandlePattern "=>" StatementList -- alt1
              
      
      HandlePattern =
        | MemberExpression "{" CommaList<PairPattern> "}" -- alt0
              
      
      ProcessExpression =
        | process_ StatementList end_ -- alt0
              
      
      RecordExpression =
        | "{" CommaList<PairExpression> "}" -- alt0
              
      
      PairExpression =
        | Name "=" Expression -- alt0
        | Name -- alt1
        | "(" Expression ")" "=" Expression -- alt2
              
      
      LiteralExpression =
        | Float -- alt0
        | Integer -- alt1
        | String -- alt2
        | Boolean -- alt3
        | nothing_ -- alt4
              
      
      AnyBlockExpression =
        | ProcessExpression -- alt0
        | DoExpression -- alt1
        | HandleExpression -- alt2
        | BeginExpression -- alt3
        | BlockLambdaExpression -- alt4
              
      
      PropName =
        | id ~":" -- alt0
              
      
      Name =
        | ~reserved id ~":" -- alt0
              
      
      Keyword =
        | keyword -- alt0
              
      
      RestrictedMember =
        | ~reserved Namespace ~("." Keyword) -- alt0
              
      
      Namespace =
        | namespace -- alt0
              
      
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
        | string -- alt0
              
      
      CommaList<Rule> =
        | NonemptyListOf<Rule, ","> ","? -- alt0
        |  -- alt1
              
      
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
              
      
      namespace =
        | nonemptyListOf<id, "::"> -- alt0
              
      
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
              
      
      string =
        | double_string -- alt0
              
      
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
              
      
      namespace_ =
        | kw<"namespace"> -- alt0
              
      
      do_ =
        | kw<"do"> -- alt0
              
      
      end_ =
        | kw<"end"> -- alt0
              
      
      let_ =
        | kw<"let"> -- alt0
              
      
      mutable_ =
        | kw<"mutable"> -- alt0
              
      
      if_ =
        | kw<"if"> -- alt0
              
      
      then_ =
        | kw<"then"> -- alt0
              
      
      else_ =
        | kw<"else"> -- alt0
              
      
      true_ =
        | kw<"true"> -- alt0
              
      
      false_ =
        | kw<"false"> -- alt0
              
      
      and_ =
        | kw<"and"> -- alt0
              
      
      or_ =
        | kw<"or"> -- alt0
              
      
      not_ =
        | kw<"not"> -- alt0
              
      
      nothing_ =
        | kw<"nothing"> -- alt0
              
      
      begin_ =
        | kw<"begin"> -- alt0
              
      
      assert_ =
        | kw<"assert"> -- alt0
              
      
      unreachable_ =
        | kw<"unreachable"> -- alt0
              
      
      data_ =
        | kw<"data"> -- alt0
              
      
      match_ =
        | kw<"match"> -- alt0
              
      
      when_ =
        | kw<"when"> -- alt0
              
      
      as_ =
        | kw<"as"> -- alt0
              
      
      default_ =
        | kw<"default"> -- alt0
              
      
      with_ =
        | kw<"with"> -- alt0
              
      
      use_ =
        | kw<"use"> -- alt0
              
      
      exposing_ =
        | kw<"exposing"> -- alt0
              
      
      for_ =
        | kw<"for"> -- alt0
              
      
      in_ =
        | kw<"in"> -- alt0
              
      
      repeat_ =
        | kw<"repeat"> -- alt0
              
      
      break_ =
        | kw<"break"> -- alt0
              
      
      continue_ =
        | kw<"continue"> -- alt0
              
      
      yield_ =
        | kw<"yield"> -- alt0
              
      
      all_ =
        | kw<"all"> -- alt0
              
      
      process_ =
        | kw<"process"> -- alt0
              
      
      resume_ =
        | kw<"resume"> -- alt0
              
      
      handle_ =
        | kw<"handle"> -- alt0
              
      
      handler_ =
        | kw<"handler"> -- alt0
              
      
      protocol_ =
        | kw<"protocol"> -- alt0
              
      
      method_ =
        | kw<"method"> -- alt0
              
      
      implement_ =
        | kw<"implement"> -- alt0
              
      
      requires_ =
        | kw<"requires"> -- alt0
              
      
      open_ =
        | kw<"open"> -- alt0
              
      
      test_ =
        | kw<"test"> -- alt0
              
      
      where_ =
        | kw<"where"> -- alt0
              
      
      is_ =
        | kw<"is"> -- alt0
              
      
      fun_ =
        | kw<"fun"> -- alt0
              
      
      define_ =
        | kw<"define"> -- alt0
              
      
      type_ =
        | kw<"type"> -- alt0
              
      
      reserved =
        | namespace_ -- alt0
        | do_ -- alt1
        | end_ -- alt2
        | let_ -- alt3
        | mutable_ -- alt4
        | if_ -- alt5
        | then_ -- alt6
        | else_ -- alt7
        | true_ -- alt8
        | false_ -- alt9
        | and_ -- alt10
        | or_ -- alt11
        | not_ -- alt12
        | nothing_ -- alt13
        | begin_ -- alt14
        | assert_ -- alt15
        | unreachable_ -- alt16
        | data_ -- alt17
        | match_ -- alt18
        | when_ -- alt19
        | as_ -- alt20
        | default_ -- alt21
        | with_ -- alt22
        | use_ -- alt23
        | exposing_ -- alt24
        | for_ -- alt25
        | in_ -- alt26
        | repeat_ -- alt27
        | yield_ -- alt28
        | all_ -- alt29
        | process_ -- alt30
        | resume_ -- alt31
        | handle_ -- alt32
        | handler_ -- alt33
        | protocol_ -- alt34
        | method_ -- alt35
        | implement_ -- alt36
        | requires_ -- alt37
        | open_ -- alt38
        | test_ -- alt39
        | where_ -- alt40
        | is_ -- alt41
        | fun_ -- alt42
        | define_ -- alt43
        | type_ -- alt44
              
    }
      
    """, 
    visitor
  )

let parse (rule: string) (source: string) (options: ParseOptions): Result<File, string> = 
  let (success, value) = !!(primParser$(source, rule, options))
  if success then Ok(!!value)
  else Error(!!value)
  