//namespace Immutable {
  using System;
  using System.Collections.Generic;

  public class DefaultDictionary<TKey, TValue> : ImmutableDictionary<TKey, TValue> {
    public readonly TValue defaultValue;
    
    public DefaultDictionary(TValue defaultValue, ImmutableDictionary<TKey, TValue> dictionary) : base(dictionary) {
      this.defaultValue = defaultValue;
    }

    public DefaultDictionary(DefaultDictionary<TKey, TValue> dictionary, TKey key, TValue value) : base(dictionary, key, value) {
      this.defaultValue = dictionary.defaultValue;
    }

    public new TValue this[TKey key] {
      get {
        return this.GetOrDefault(key, defaultValue);
      }
    }

    public new DefaultDictionary<TKey, TValue> With(TKey key, TValue value)
      => new DefaultDictionary<TKey, TValue>(this, key, value);
  }

  public static class DefaultDictionaryExtensionMethods {
    public static DefaultDictionary<UKey, UValue> ToDefaultDictionary<T, UKey, UValue>(this IEnumerable<T> e, UValue defaultValue, Func<T, UKey> key, Func<T, UValue> value)
      => new DefaultDictionary<UKey, UValue>(defaultValue, e.ToImmutableDictionary(key, value));
  }
//}