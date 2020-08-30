using static Generator;

public static class AstGenerator {
  public static void Main() {
    Generate(
      "AstGenerated.cs",
      "using System.Collections.Generic;",
      "namespace Ast {",
      "}",
      "Ast.",
      Types(
        Variant("Expr",
          Case("int", "Int"),
          Case("string", "String")),
        Variant("AstNode",
          Case("Expr", "Terminal"),
          Case("IEnumerable<AstNode>", "Operator"))));
  }
}