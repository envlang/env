using System;
using System.Linq;
using System.Collections.Generic;

public static class Collection {
  public static void ForEach<T>(this IEnumerable<T> x, Action<T> f)
    => x.ToList().ForEach(f);

  /*
  public static ListI<Tuple<T,U>> Add<T,U>(this ListI<Tuple<T,U>> l, T x, U y)
    => l.Add(Tuple.Create(x,y));
  */

  // System.Collections.Immutable requires NuGet and is not available on repl.it
  public static List<T> Cons<T>(this List<T> l, T x) { l.Add(x); return l; }

  // Circumvent bug with collection initializers, tuples and
  // first-class functions by using repeated .Add()
  // See https://repl.it/@suzannesoy/WarlikeWorstTraining#main.cs

  public static List<Tuple<T,U>> Cons<T,U>(this List<Tuple<T,U>> l, T x, U y)
    => l.Cons(Tuple.Create(x,y));

  public static List<Tuple<T,U,V>> Cons<T,U,V>(this List<Tuple<T,U,V>> l, T x, U y, V z)
    => l.Cons(Tuple.Create(x,y,z));

  public static void Deconstruct<A, B>(this Tuple<A, B> t, out A a, out B b) {
    a = t.Item1;
    b = t.Item2;
  }
}