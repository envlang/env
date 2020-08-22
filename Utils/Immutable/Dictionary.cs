/*
//namespace Immutable {
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Collections;
  using Mutable = System.Collections.Generic;
  using Microsoft.FSharp.Collections;
  using System.Collections.Immutable;

  // TODO: use Microsoft.FSharp.Collections.FSharpMap
  public class ImmutableDictionary<TKey, TValue> : Mutable.IReadOnlyDictionary<TKey, TValue> {
    private readonly Mutable.Dictionary<TKey, TValue> d;
    private System.Collections.Immutable.ImmutableDictionary<TKey, TValue> i = System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.Empty;
    //private readonly FSharpMap<TKey, TValue> x = new FSharpMap<int, int>(Enumerable.Enpty<Tuple<int, int>>());

    public ImmutableDictionary() {
      d = new Mutable.Dictionary<TKey, TValue>();
    }

    public ImmutableDictionary(Mutable.Dictionary<TKey, TValue> d) {
      // Clone the mutable dictionary.
      this.d = new Mutable.Dictionary<TKey, TValue>(d, d.Comparer);
    }

    public ImmutableDictionary(ImmutableDictionary<TKey, TValue> immutableDictionary, TKey key, TValue value) {
      // Clone the mutable dictionary contained within that immutable
      // dictionary before updating it.
      var clone = new Mutable.Dictionary<TKey, TValue>(immutableDictionary.d, immutableDictionary.d.Comparer);
      clone.Add(key, value);
      this.d = clone;
    }

    public ImmutableDictionary(ImmutableDictionary<TKey, TValue> immutableDictionary) {
      // No need to clone the mutable dictionary contained within
      // that immutable dictionary, since we know it is never mutated.
      this.d = immutableDictionary.d;
    }

    public TValue this[TKey key] { get => d[key]; }

    public IEnumerable<TKey> Keys { get => d.Keys; }
    public IEnumerable<TValue> Values { get => d.Values; }
    public int Count { get => d.Count; }
    public IEqualityComparer<TKey> Comparer { get => d.Comparer; }

    public bool TryGetValue(TKey key, out TValue value)
      => d.TryGetValue(key, out value);

    public bool ContainsKey(TKey key)
      => d.ContainsKey(key);

    public ImmutableDictionary<TKey, TValue> With(TKey key, TValue value)
      => new ImmutableDictionary<TKey, TValue>(this, key, value);

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
      => d.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => d.GetEnumerator();
  }

  public static class ImmutableDictionaryExtensionMethods {
    public static ImmutableDictionary<UKey, UValue> ToImmutableDictionary<T, UKey, UValue>(this IEnumerable<T> e, Func<T, UKey> key, Func<T, UValue> value)
      => new ImmutableDictionary<UKey, UValue>(e.ToDictionary(key, value));
  }

  // Prevent usage of the mutable dictionary.
  public abstract class Dictionary<TKey, TValue> : ImmutableDictionary<TKey, TValue> {
  }

//}
*/