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
          Field("Fixity", "fixity"),
          Field("List<S>", "parts"),
          Field("List<string>", "holes")),

        Record("DAGNode",
          Field("List<Operator>", "infixLeftAssociative"),
          Field("List<Operator>", "infixRightAssociative"),
          Field("List<Operator>", "infixNonAssociative"),
          Field("List<Operator>", "prefix"),
          Field("List<Operator>", "postfix"),
          Field("List<Operator>", "closed"),
          Field("List<Operator>", "terminal"),
          Field("List<string>", "successorNodes"))));
  }
}