using static Generator;

public static class ParserGenerator {
  public static void Main() {
    Generate(
      "ParserGenerated.cs",
        "using System.Collections.Immutable;\n"
      + "using S = Lexer.S;",
      "public static partial class Parser {",
      "}",
      "Parser.",
      Types(
        Variant("Grammar",
          Case("ImmutableList<Parser.Grammar>", "Or"),
          Case("ImmutableList<Parser.Grammar>", "Sequence")),

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
          Field("ImmutableList<S>", "parts"),
          Field("ImmutableList<string>", "holes")),

        Record("DAGNode",
          Field("ImmutableList<Operator>", "infixLeftAssociative"),
          Field("ImmutableList<Operator>", "infixRightAssociative"),
          Field("ImmutableList<Operator>", "infixNonAssociative"),
          Field("ImmutableList<Operator>", "prefix"),
          Field("ImmutableList<Operator>", "postfix"),
          Field("ImmutableList<Operator>", "closed"),
          Field("ImmutableList<Operator>", "terminal"),
          Field("ImmutableList<string>", "successorNodes"))));
  }
}