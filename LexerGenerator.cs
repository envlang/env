// Run with: sh ./T4/Generators.sh

using System.Collections.Generic;

public static class LexerGenerator {
  public static void Generate() {
    Generator.Generate("LexerGenerated.cs", "public static partial class Lexer {", "}", "Lexer.", "S", new Dictionary<string, string> {
      { "End", null },
      { "Space", null },
      { "Int", null },
      { "Decimal", null },
      { "String", null },
      { "StringOpen", null },
      { "StringClose", null },
    });
  }
}