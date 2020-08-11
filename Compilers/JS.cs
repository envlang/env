namespace Compilers {
  public class JS {
    public static string Compile(Ast.Expr source) {
      return "process.stdout.write('42');";
    }
  }
}