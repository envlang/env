using System;
using System.Linq;

public static class Piping {
  public static U Pipe<T, U>(this T x, Func<T, U> f) => f(x);
  public static U Pipe<T1, T2, U>(this ValueTuple<T1, T2> x, Func<T1, T2, U> f) => f(x.Item1, x.Item2);
  
  public static void Pipe<T>(this T x, Action<T> f) => f(x);

  public static T Do<T>(this T x, Action<T> f) { f(x); return x; }
}