// Run with: sh ./T4/Generators.sh

using System.Collections.Generic;

public static class AstGenerator {
  public static void Generate() {
    Generator.Generate("Ast.cs", "namespace Ast {", "}", "Ast.", "Expr", new Dictionary<string, string> {
      { "Int", "int" },
      { "String", "string" },
    });
  }
}