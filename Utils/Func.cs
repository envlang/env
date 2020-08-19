using System;
using System.Collections.Generic;
using System.Linq;

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

  public static Func<A, B> Memoize<A, B>(this Func<A, B> f) where A : IEquatable<A> {
    var d = new Dictionary<A, B>();
    return a => {
      if (d.TryGetValue(a, out var b)) {
        return b;
      } else {
        var calcB = f(a);
        d.Add(a, calcB);
        return calcB;
      }
    };
  }
}