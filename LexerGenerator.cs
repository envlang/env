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
          Case("Eq"),
          Case("Space"),
          Case("String"),
          Case("StringOpen"),
          Case("StringClose")),
        Record("Rule",
          Field("Lexer.S", "oldState"),
          Field("string", "description"),
          Field("Func<GraphemeCluster, bool>", "test"),
          Field("Lexer.S", "throughState"),
          Field("Lexer.S", "newState"))));
  }
}