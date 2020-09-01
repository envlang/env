namespace Compilers {
  public class JS {
    public static string Compile(Ast.AstNode source) {
      return "process.stdout.write(String("
      + "\"NO JavaScript COMPILATION FOR NOW\""
/*      + source.Match(
         Int: i => i.ToString(),
         String: s => $"'{s.ToString()}'"
        )*/
      + "));";
    }
  }
}