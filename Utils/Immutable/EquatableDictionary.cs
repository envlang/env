using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

public class EquatableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IString, IEquatable<EquatableDictionary<TKey, TValue>> {
  public readonly ImmutableDictionary<TKey, TValue> dictionary;
  
  public EquatableDictionary() {
    this.dictionary = ImmutableDictionary<TKey, TValue>.Empty;
    this.hashCode = "EquatableDictionary".GetHashCode();
  }

  public EquatableDictionary(ImmutableDictionary<TKey, TValue> dictionary) {
    this.dictionary = dictionary;
    this.hashCode = dictionary.Aggregate(
      "EquatableDictionary".GetHashCode(),
      (h, kvp) =>
        h ^ kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
  }

  private EquatableDictionary(EquatableDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
    this.dictionary = dictionary.dictionary.Add(key, value);
    this.hashCode =
      dictionary.hashCode ^ key.GetHashCode() ^ value.GetHashCode();
  }

  public TValue this[TKey key] {
    get => dictionary[key];
  }

  public EquatableDictionary<TKey, TValue> Add(TKey key, TValue value)
    => new EquatableDictionary<TKey, TValue>(this, key, value);

  // These would need to update the hashCode, disabled for now.
  /*public EquatableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
    => new EquatableDictionary<TKey, TValue>(dictionary.SetItem(key, value));

  public EquatableDictionary<TKey, TValue> Remove(TKey key)
    => new EquatableDictionary<TKey, TValue>(dictionary.Remove(key));*/

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();

  /*public EquatableDictionaryLens<
           TKey,
           TValue,
           EquatableDictionary<TKey, TValue>>
    lens {
      get => this.ChainLens(x => x);
    }*/

  public override string ToString()
    => "EquatableDictionary {\n"
       + this.Select(kvp => (ks: kvp.Key.ToString(),
                             vs: kvp.Value.ToString()))
          .OrderBy(p => p.ks)
          .Select(p => $"{{ {p.ks}, {p.vs} }}")
          .JoinWith(",\n")
       + "\n}";

  public string Str() => ToString();

  private bool SameKVP(EquatableDictionary<TKey, TValue> other) {
    foreach (var kvp in this) {
      // Let's hope that this uses EqualityComparer<TKey>.Default and EqualityComparer<TValue>.Default.
      if (!other.Contains(kvp)) { return false; }
    }
    foreach (var kvp in this) {
      if (!this.Contains(kvp)) { return false; }
    }
    return false;
  }

  public override bool Equals(object other)
    => Equality.Untyped<EquatableDictionary<TKey, TValue>>(
      this,
      other,
      x => x as EquatableDictionary<TKey, TValue>,
      x => x.hashCode,
      (x, y) => x.SameKVP(y));
  public bool Equals(EquatableDictionary<TKey, TValue> other)
    => Equality.Equatable<EquatableDictionary<TKey, TValue>>(this, other);
  private readonly int hashCode;
  public override int GetHashCode() => hashCode;
}

public static class EquatableDictionaryExtensionMethods {
  public static EquatableDictionary<UKey, UValue> ToEquatableDictionary<T, UKey, UValue>(this IEnumerable<T> e, Func<T, UKey> key, Func<T, UValue> value)
    => new EquatableDictionary<UKey, UValue>(e.ToImmutableDictionary(key, value));

  public static EquatableDictionary<TKey, TValue> ToEquatableDictionary<TKey, TValue>(this ImmutableDictionary<TKey, TValue> d)
    => new EquatableDictionary<TKey, TValue>(d);
}