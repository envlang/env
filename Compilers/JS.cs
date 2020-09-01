namespace Compilers {
  public class JS {
    public static string Compile(Ast.AstNode source) {
      return "process.stdout.write(String("
      + "\"no JS for now\""
/*      + source.Match(
         Int: i => i.ToString(),
         String: s => $"'{s.ToString()}'"
        )*/
      + "));";
    }
  }
}