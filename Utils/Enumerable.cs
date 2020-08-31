using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Immutable;

public static class Collection {
  public static void ForEach<T>(this IEnumerable<T> x, Action<T> f)
    => x.ToImmutableList().ForEach(f);

  public static ImmutableList<T> Cons<T>(this T x, ImmutableList<T> l)
    => l.Add(x);

  public static IEnumerable<T> Cons<T>(this T x, IEnumerable<T> l)
    => x.Singleton().Concat(l);

  public static IEnumerable<T> Concat<T>(this IEnumerable<T> l, T x)
    => l.Concat(x.Singleton());

  public static ImmutableList<Tuple<T,U>> Add<T,U>(this ImmutableList<Tuple<T,U>> l, T x, U y)
    => l.Add(Tuple.Create(x,y));

  public static ImmutableList<Tuple<T,U,V>> Add<T,U,V>(this ImmutableList<Tuple<T,U,V>> l, T x, U y, V z)
    => l.Add(Tuple.Create(x,y,z));

  public static void Deconstruct<A, B>(this Tuple<A, B> t, out A a, out B b) {
    a = t.Item1;
    b = t.Item2;
  }

  public struct Item<T> {
    public readonly T item;
    public readonly long index;
    public readonly bool first;
    public readonly bool last;
    public Item(T item, long index, bool first, bool last) {
      this.item = item;
      this.index = index;
      this.first = first;
      this.last = last;
    }
  }

  public static IEnumerable<Item<T>> Indexed<T>(this IEnumerable<T> e) {
    long i         = 0L;
    bool first     = true;

    // These default(…) are written below before being read
    T    prevX     = default(T);
    long prevI     = default(long);
    bool prevFirst = default(bool);

    foreach (var x in e) {
      if (!first) {
        yield return new Item<T>(prevX, prevI, prevFirst, false);
      }
      prevX = x;
      prevI = i;
      prevFirst = first;
      first = false;
      i++;
    }
    if (!first) {
      yield return new Item<T>(prevX, prevI, prevFirst, true);
    }
  }

  public struct Peekable<T> : IEnumerator<T>, System.Collections.IEnumerator {
    private IEnumerator<T> e;
    private bool peeked;
    private T previous;
    public T Current { get => peeked ? previous : e.Current; }
    object System.Collections.IEnumerator.Current {
      get => this.Current;
    }
    public bool MoveNext() {
      this.peeked = false;
      this.previous = default(T); // guarded by peeked
      return this.e.MoveNext();
    }
    public bool Peek() {
      if (this.peeked) {
        throw new Exception("Already peeked once");
      } else {
        this.previous = e.Current;
        this.peeked = true;
        return this.e.MoveNext();
      }
    }
    public void Dispose() { e.Dispose(); }
    public void Reset() { e.Reset(); }

    public Peekable(IEnumerable<T> e) {
      this.e = e.GetEnumerator();
      this.peeked = false;
      this.previous = default(T); // guarded by peeked
    }
  }

  public static Peekable<T> Peekable<T>(this IEnumerable<T> e) {
    return new Peekable<T>(e);
  }

  public static IEnumerable<T> SingleUseEnumerable<T>(this IEnumerator<T> e) {
    while (e.MoveNext()) {
      yield return e.Current;
    }
  }

  public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> e, Func<T, bool> f)
    => e.TakeWhile(x => !f(x));

  public static IEnumerable<T> Singleton<T>(this T x) {
    yield return x;
  }

  public static string Join(this string separator, IEnumerable<string> strings)
    => String.Join(separator, strings);

  public static Option<T> First<T>(this IEnumerable<T> ie) {
    var e = ie.GetEnumerator();
    if (e.MoveNext()) {
      return e.Current.Some();
    } else {
      return Option.None<T>();
    }
  }

  public static Option<T> Last<T>(this IEnumerable<T> ie) {
    var e = ie.GetEnumerator();
    T element = default(T);
    bool found = false;
    while (e.MoveNext()) {
      element = e.Current;
      found = true;
    }
    if (found) {
      return element.Some();
    } else {
      return Option.None<T>();
    }
  }

  public static Option<T> First<T>(this IEnumerable<T> ie, Func<T, bool> predicate) {
    var e = ie.GetEnumerator();
    while (e.MoveNext()) {
      if (predicate(e.Current)) {
        return e.Current.Some();
      }
    }
    return Option.None<T>();
  }

  public static Option<U> First<T, U>(this IEnumerable<T> ie, Func<T, Option<U>> selector) {
    var e = ie.GetEnumerator();
    while (e.MoveNext()) {
      var found = selector(e.Current);
      if (found.IsSome) {
        return found;
      }
    }
    return Option.None<U>();
  }

  public static Option<T> Single<T>(this IEnumerable<T> ie) {
    var e = ie.GetEnumerator();
    if (e.MoveNext()) {
      var value = e.Current;
      if (e.MoveNext()) {
        return Option.None<T>();
      } else {
        return value.Some();
      }
    } else {
      return Option.None<T>();
    }
  }

  public static Option<V> GetValue<K, V>(this ImmutableDictionary<K, V> d, K key) {
    V result = default(V);
    if (d.TryGetValue(key, out result)) {
      return result.Some();
    } else {
      return Option.None<V>();
    }
  }

  public static V GetOrDefault<K, V>(this ImmutableDictionary<K, V> d, K key, V defaultValue) {
    V result = default(V);
    if (d.TryGetValue(key, out result)) {
      return result;
    } else {
      return defaultValue;
    }
  }

  public static Option<T> Aggregate<T, T>(this IEnumerable<T> ie, Func<T, T, T> f) {
    var e = ie.GetEnumerator();
    if (e.MoveNext()) {
      var accumulator = e.Current;
      while (e.MoveNext()) {
        accumulator = f(accumulator, e.Current);
      }
      return accumulator.Some();
    }
    return Option.None<T>();
  }

  public static string JoinWith(this IEnumerable<string> strings, string joiner)
    // TODO: use StringBuilder, there is no complexity info in the docs.
    => String.Join(joiner, strings);

  public static string JoinToStringWith<T>(this IEnumerable<T> objects, string joiner)
    // TODO: use StringBuilder, there is no complexity info in the docs.
    => String.Join(joiner, objects.Select(o => o.ToString()));

  public static bool SetEquals<T>(this ImmutableHashSet<T> a, ImmutableHashSet<T> b)
    => a.All(x => b.Contains(x)) && b.All(x => a.Contains(x));

  public static (Option<T> firstElement, ImmutableQueue<T> rest) Dequeue<T>(this ImmutableQueue<T> q) {
    if (q.IsEmpty) {
      return (Option.None<T>(), q);
    } else {
      T firstElement;
      var rest = q.Dequeue(out firstElement);
      return (firstElement.Some(), rest);
    }
  }

  public static IEnumerable<T> SkipLastWhile<T>(this IEnumerable<T> e, Func<T, bool> predicate) {
    var pending = ImmutableQueue<T>.Empty;
    foreach (var x in e) {
      if (predicate(x)) {
        pending = pending.Enqueue(x);
      } else {
        while (!pending.IsEmpty) {
          yield return pending.Peek();
          pending = pending.Dequeue();
        }
        yield return x;
      }
    }
  }

  public static Option<A> BindFold<T, A>(this IEnumerable<T> e, A init, Func<A, T, Option<A>> f) {
    var acc = init;
    foreach (var x in e) {
      var newAcc = f(acc, x);
      if (newAcc.IsNone) {
        return Option.None<A>();
      } else {
        acc = newAcc.ElseThrow(new Exception("impossible"));
      }
    }
    return acc.Some();
  }

  public static Option<ValueTuple<A, IEnumerable<U>>> BindFoldMap<T, A, U>(this IEnumerable<T> e, A init, Func<A, T, Option<ValueTuple<A, U>>> f)
    => e.BindFold(
      (init, ImmutableStack<U>.Empty),
      (accL, x) =>
        f(accL.Item1, x).IfSome((newAcc, result) =>
          (newAcc, accL.Item2.Push(result)))
    ).IfSome((acc, l) => (acc, l.Reverse<U>()));

  public static A FoldWhileSome<A>(this A init, Func<A, Option<A>> f) {
    var lastGood = init;
    while (true) {
      var @new = f(lastGood);
      if (@new.IsNone) {
        return lastGood;
      } else {
        lastGood = @new.ElseThrow(() => new Exception("impossible"));
      }
    }
  }

  public static Option<Tuple<A, B>> FoldWhileSome<A, B>(this Option<Tuple<A, B>> init, Func<A, B, Option<Tuple<A, B>>> f)
    => init.IfSome(ab1 => ab1.FoldWhileSome(ab => f(ab.Item1, ab.Item2)));

  public static Option<ValueTuple<A, B>> FoldWhileSome<A, B>(this Option<ValueTuple<A, B>> init, Func<A, B, Option<ValueTuple<A, B>>> f)
    => init.IfSome(ab1 => ab1.FoldWhileSome(ab => f(ab.Item1, ab.Item2)));

  public static ValueTuple<A, IEnumerable<U>> FoldMapWhileSome<A, U>(this A init, Func<A, Option<ValueTuple<A, U>>> f)
    => FoldWhileSome(
      (init, ImmutableStack<U>.Empty),
      accL =>
        f(accL.Item1).IfSome((newAcc, result) =>
          (newAcc, accL.Item2.Push(result)))
    ).Pipe((acc, l) => (acc, l.Reverse<U>()));
}