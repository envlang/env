using static Generator;

public static class ParserGenerator {
  public static void Main() {
    Generate(
      "MixFixGenerated.cs",
        "using System.Collections.Generic;\n"
      + "using System.Collections.Immutable;\n"
      + "using S = Lexer.S;\n"
      + "using PrecedenceGroupName = System.String;",
      "public static partial class MixFix {",
      "}",
      "MixFix.",
      Types(
        Variant("Grammar",
          Case("Grammar",              "RepeatOnePlus"),
          Case("IEnumerable<Grammar>", "Or"),
          Case("IEnumerable<Grammar>", "Sequence"),
          Case("S",                    "Terminal"),
          Case("string",               "Rule")),

        Variant("Fixity",
          Case("Closed"),
          Case("InfixLeftAssociative"),
          Case("InfixRightAssociative"),
          Case("InfixNonAssociative"),
          Case("Prefix"),
          Case("Postfix")),

        Variant("Associativity",
          Case("NonAssociative"),
          Case("LeftAssociative"),
          Case("RightAssociative")),

        Record("Operator",
          Field("PrecedenceGroupName", "precedenceGroup"),
          Field("Associativity",       "associativity"),
          Field("ImmutableList<Part>", "parts")),

        Variant("Part",
          Case("S",                                     "Name"),
          Case("ImmutableHashSet<PrecedenceGroupName>", "Hole")),

        Record("DAGNode",
          Field("ImmutableList<Operator>", "infixLeftAssociative"),
          Field("ImmutableList<Operator>", "infixRightAssociative"),
          Field("ImmutableList<Operator>", "infixNonAssociative"),
          Field("ImmutableList<Operator>", "prefix"),
          Field("ImmutableList<Operator>", "postfix"),
          Field("ImmutableList<Operator>", "closed"))));
  }
}