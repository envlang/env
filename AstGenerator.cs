using static Generator;

public static class AstGenerator {
  public static void Main() {
    Generate(
      "AstGenerated.cs",
      "namespace Ast {",
      "}",
      "Ast.",
      Types(
        Variant("Expr",
          Case("Int", "int"),
          Case("String", "string"))));
  }
}