using static Generator;

public static class LexerGenerator {
  public static void Main() {
    Generator.Generate(
      "LexerGenerated.cs",
      "",
      "public static partial class Lexer {",
      "}",
      "Lexer.",
      Types(
        Variant("S",
          Case("End"),
          Case("Space"),
          Case("Int"),
          Case("Decimal"),
          Case("String"),
          Case("StringOpen"),
          Case("StringClose")),
        Record("Rule",
          Field("S", "oldState"),
          Field("string", "description"),
          Field("Func<GraphemeCluster, bool>", "test"),
          Field("S", "throughState"),
          Field("S", "newState"))));
  }
}