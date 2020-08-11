using System;
using Ast;

public class Visitor<T> {
  public Func<int, T> Int { get; set; }
  public Func<string, T> String { get; set; }
}

namespace Ast {
  public interface Expr {
    T Match_<T>(Visitor<T> c);
  }

  public abstract class Const<T> : Expr {
    public readonly T value;
    public Const(T value) { this.value = value; }
    public abstract U Match_<U>(Visitor<U> c);
  }

  public class Int : Const<int> {
    public Int(int x) : base(x) {}
    public override T Match_<T>(Visitor<T> c) => c.Int(value);
  }

  public class String : Const<string> {
    public String(string x) : base(x) {}
    public override T Match_<T>(Visitor<T> c) => c.String(value);
  }
}

public static class AstExtensionMethods {
  public static T Match<T>(
      this Ast.Expr e,
      Func<int, T> Int,
      Func<string, T> String
    ) {
    return e.Match_(new Visitor<T> {
      Int = Int,
      String = String
    });
  }
}