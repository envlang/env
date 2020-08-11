public class Evaluator {
  public static string Evaluate(Ast.Expr source) {
    return source.Match(
      Int: i => i.ToString(),
      String: s => s.ToString()
    );
  }
}