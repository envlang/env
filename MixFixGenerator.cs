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
        // TODO: maybe instead of going through a
        // Rule node we could just memoize the ToGrammar1
        // function, and attach an optional name to nodes
        Variant("Grammar1",
          Case("Grammar1",              "RepeatOnePlus"),
          Case("IEnumerable<Grammar1>", "Or"),
          Case("IEnumerable<Grammar1>", "Sequence"),
          Case("S",                     "Terminal"),
          Case("ValueTuple<Annotation, Grammar1>", "Annotated"),
          Case("string",                "Rule")),

        Variant("Annotation",
          Case("MixFix.Operator", "Operator"),
          Case("Associativity", "SamePrecedence"),
          Case("Hole")),

        Variant("Grammar2",
          Case("Grammar2",              "RepeatOnePlus"),
          Case("IEnumerable<Grammar2>", "Or"),
          Case("IEnumerable<Grammar2>", "Sequence"),
          Case("S",                     "Terminal"),
          Case("ValueTuple<Annotation, Grammar2>", "Annotated")),

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
          Field("Semantics",           "semantics"),
          Field("PrecedenceGroupName", "precedenceGroup"),
          Field("Associativity",       "associativity"),
          Field("ImmutableList<Part>", "parts")),

        Variant("Semantics",
          Case("Program"),
          Case("LiteralInt"),
          Case("LiteralString"),
          Case("And"),
          Case("Unsupported")),

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