public class Evaluator {
  public static string Evaluate(Ast.Expr source) {
    return source.Match(
      Int: i => i.ToString(),
      String: s => s.ToString()
    );
  }
}

// Note: for typeclass resolution, ask that functions have their parameters and return types annotated. This annotation is added to the values at run-time, which allows to dispatch based on the annotation rather than on the actual value.