using System;
using System.Collections.Generic;
using System.Linq;
using Mutable = System.Collections.Generic;

public static class Func {
  // supply 1 argument to function of 2 arguments
  public static Func<B,C> Partial<A,B,C>(this Func<A,B,C> f, A a) {
    return b => f(a, b);
  }

  // supply 1 argument to function of 3 arguments
  public static Func<B,C,D> Partial<A,B,C,D>(this Func<A,B,C,D> f, A a) {
    return (b, c) => f(a, b, c);
  }

  // supply 2 arguments to function of 3 arguments
  public static Func<C,D> Partial<A,B,C,D>(this Func<A,B,C,D> f, A a, B b) {
    return c => f(a, b, c);
  }

  // break down function of 2 arguments to require 2 successive 1-argument calls
  public static Func<A,Func<B,C>> Curry<A,B,C>(this Func<A,B,C> f) {
    return a => b => f(a, b);
  }

  // break down function of 3 arguments to require 2 successive 1-argument calls
  public static Func<A,Func<B,Func<C,D>>> Curry<A,B,C,D>(this Func<A,B,C,D> f) {
    return a => b => c => f(a, b, c);
  }

  public static Func<A, B> YMemoize<A, B>(this Func<Func<A, B>, A, B> f) where A : IEquatable<A> {
    var d = new Mutable.Dictionary<A, B>();
    // I'm too lazy to implement the Y combinator…
    Func<A, B> memf = null;
    memf = a => {
      if (d.TryGetValue(a, out var b)) {
        return b;
      } else {
        var calcB = f(memf, a);
        d.Add(a, calcB);
        return calcB;
      }
    };
    return memf;
  }

  public static Func<A, B, C> YMemoize<A, B, C>(this Func<Func<A, B, C>, A, B, C> f) where A : IEquatable<A> where B : IEquatable<B>
  => (a, b) => YMemoize<ValueTuple<A, B>, C>(
                 (memf, ab) =>
                   f((aa, bb) => memf((aa, bb)),
                     ab.Item1,
                     ab.Item2))
               ((a, b));

  public static Func<A, B> Memoize<A, B>(this Func<A, B> f) where A : IEquatable<A>
    => YMemoize<A, B>((memf, aa) => f(aa));

  public static Func<A, B, C> Memoize<A, B, C>(this Func<A, B, C> f) where A : IEquatable<A> where B : IEquatable<B>
    => YMemoize<A, B, C>((memf, aa, bb) => f(aa, bb));

  public static B Memoized<A, B>(this Func<A, B> f, A a) where A : IEquatable<A>
    => f.Memoize()(a);

  public static C Memoized<A, B, C>(this Func<A, B, C> f, A a, B b) where A : IEquatable<A> where B : IEquatable<B>
    => f.Memoize()(a, b);

  public static B YMemoized<A, B>(this Func<Func<A, B>, A, B> f, A a) where A : IEquatable<A>
    => f.YMemoize()(a);

  public static C YMemoized<A, B, C>(this Func<Func<A, B, C>, A, B, C> f, A a, B b) where A : IEquatable<A> where B : IEquatable<B>
    => f.YMemoize()(a, b);
}