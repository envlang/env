using static Generator;

public static class ParserGenerator {
  public static void Main() {
    Generate(
      "ParserGenerated.cs",
        "using System.Collections.Generic;\n"
      + "using S = Lexer.S;",
      "public static partial class Parser {",
      "}",
      "Parser.",
      Types(
        Variant("Grammar",
          Case("List<Parser.Grammar>", "Or"),
          Case("List<Parser.Grammar>", "Sequence")),

        Variant("Fixity",
          Case("Closed"),
          Case("InfixLeftAssociative"),
          Case("InfixRightAssociative"),
          Case("InfixNonAssociative"),
          Case("Prefix"),
          Case("Postfix"),
          Case("Terminal")),

        Record("Operator",
          Field("List<S>", "Parts"),
          Field("List<string>", "Holes")),

        Record("Closed",
          Field("S", "openSymbol"),
          Field("S", "closedSymbol")),

        Record("DAGNode",
          Field("List<S>", "infixLeftAssociative"),
          Field("List<S>", "infixRightAssociative"),
          Field("List<S>", "infixNonAssociative"),
          Field("List<S>", "prefix"),
          Field("List<S>", "postfix"),
          Field("List<Closed>", "closed"),
          Field("List<S>", "terminal"),
          Field("List<string>", "successors"))));
  }
}