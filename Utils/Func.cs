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

// IEquatableFunction
// Possible with <in T, out U> if we remove the IEquatable constraint
public interface IEqF<T, U> {// : IEquatable<IEqF<T, U>> {
  U F(T x);
}

public interface IEqF<T1, T2, U> {// : IEquatable<IEqF<T, U>> {
  U F(T1 x, T2 y);
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class F : System.Attribute {}

public class PartialEqF<T1, T2, U> : IEqF<T2, U>, IEquatable<PartialEqF<T1, T2, U>> {
  private readonly IEqF<T1, T2, U> f;
  private readonly T1 arg1;
  public PartialEqF(IEqF<T1, T2, U> f, T1 arg1) {
    this.f = f;
    this.arg1 = arg1;
    hashCode = Equality.HashCode("PartialEqF<T1, T2, U>", f, arg1);
  }
  public U F(T2 arg2) => f.F(arg1, arg2);
  public static bool operator ==(PartialEqF<T1, T2, U> a, PartialEqF<T1, T2, U> b)
    => Equality.Operator(a, b);
  public static bool operator !=(PartialEqF<T1, T2, U> a, PartialEqF<T1, T2, U> b)
    => !(a == b);
  public override bool Equals(object other)
    => Equality.Untyped<PartialEqF<T1, T2, U>>(
      this,
      other,
      x => x as PartialEqF<T1, T2, U>,
      x => x.hashCode,
      x => x.f,
      x => x.arg1);
  public bool Equals(PartialEqF<T1, T2, U> other)
    => Equality.Equatable<PartialEqF<T1, T2, U>>(this, other);
  private int hashCode;
  public override int GetHashCode() => hashCode;
  public override string ToString() => "Equatable function PartialEqF<T1, T2, U>()";
}

public static class EqFExtensionMethods {
  public static IEqF<T2, U> ImPartial<T1, T2, U>(this IEqF<T1, T2, U> f, T1 arg1)
  => new PartialEqF<T1, T2, U>(f, arg1);
}
