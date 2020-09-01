using static Generator;

public static class AstGenerator {
  public static void Main() {
    Generate(
      "AstGenerated.cs",
      "using System.Collections.Generic;",
      "namespace Ast {",
      "}",
      "Ast.",
      Types(
        Variant("Expr",
          Case("int", "Int"),
          Case("string", "String")),

        Variant("Val",
          Case("int", "Int"),
          Case("string", "String")),

        Variant("ParserResult",
          Case("(MixFix.Annotation, ParserResult)", "Annotated"),
          Case("Lexer.Lexeme", "Terminal"),
          Case("IEnumerable<ParserResult>", "Productions")),

        Variant("ParserResult2",
          Case("ValueTuple<MixFix.Associativity, IEnumerable<OperatorOrHole>>", "SamePrecedence")),
        Variant("OperatorOrHole",
          Case("ValueTuple<MixFix.Operator, IEnumerable<SamePrecedenceOrTerminal>>", "Operator"),
          Case("ParserResult2", "Hole")),
        Variant("SamePrecedenceOrTerminal",
          Case("ParserResult2", "SamePrecedence"),
          Case("Lexer.Lexeme", "Terminal")),

        Variant("AstNode",
          Case("Lexer.Lexeme", "Terminal"),
          Case("ValueTuple<MixFix.Operator, IEnumerable<AstNode>>", "Operator"))));
  }
}