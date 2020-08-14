namespace Compilers {
  public class JS {
    public static string Compile(Ast.Expr source) {
      return "process.stdout.write(String("
      + source.Match(
         Int: i => i.ToString(),
         String: s => $"'{s.ToString()}'"
        )
      + "));";
    }
  }
}