using System;
using System.Linq;

public static class Piping {
  public static U Pipe<T, U>(this T x, Func<T, U> f) => f(x);
  
  public static void Pipe<T>(this T x, Action<T> f) => f(x);

  public static T Do<T>(this T x, Action<T> f) { f(x); return x; }
}