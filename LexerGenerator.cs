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
        Record("Lexeme",
          Field("S", "state"),
          // TODO: maybe keep the lexeme as a list of
          // grapheme clusters
          Field("string", "lexeme")),
        Variant("S",
          Case("StartOfInput"),
          Case("End"),
          Case("EndOfInput"),
          Case("Space"),
          Case("Int"),
          Case("Decimal"),
          Case("Ident"),
          Case("And"),
          Case("Plus"),
          Case("Times"),
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