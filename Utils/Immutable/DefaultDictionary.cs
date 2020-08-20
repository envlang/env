using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

public class ImmutableDefaultDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
  public readonly TValue defaultValue;
  public readonly ImmutableDictionary<TKey, TValue> dictionary;
  
  public ImmutableDefaultDictionary(TValue defaultValue, ImmutableDictionary<TKey, TValue> dictionary) {
    this.defaultValue = defaultValue;
    this.dictionary = dictionary;
  }

  public TValue this[TKey key] {
    get => dictionary.GetOrDefault(key, defaultValue);
  }

  public ImmutableDefaultDictionary<TKey, TValue> With(TKey key, TValue value)
    => new ImmutableDefaultDictionary<TKey, TValue>(defaultValue, dictionary.Add(key, value));

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dictionary.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();
}

public static class ImmutableDefaultDictionaryExtensionMethods {
  public static ImmutableDefaultDictionary<UKey, UValue> ToImmutableDefaultDictionary<T, UKey, UValue>(this IEnumerable<T> e, UValue defaultValue, Func<T, UKey> key, Func<T, UValue> value)
    => new ImmutableDefaultDictionary<UKey, UValue>(defaultValue, e.ToImmutableDictionary(key, value));

  public static ImmutableDefaultDictionary<TKey, TValue> ToImmutableDefaultDictionary<TKey, TValue>(this ImmutableDictionary<TKey, TValue> d, TValue defaultValue)
    => new ImmutableDefaultDictionary<TKey, TValue>(defaultValue, d);
}