using System;
using System.Linq;
using System.Collections.Generic;
using Immutable;

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

  public static Option<T> First<T>(this IEnumerable<T> ie, Func<T, bool> predicate) {
    var e = ie.GetEnumerator();
    while (e.MoveNext()) {
      if (predicate(e.Current)) {
        return e.Current.Some();
      }
    }
    return Option.None<T>();
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
}