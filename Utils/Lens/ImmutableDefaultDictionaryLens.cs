// Code quality of this file: medium.

using System;
using System.Collections.Immutable;

public sealed class ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole> : ILens<TValue, Whole> {
  public readonly Func<ImmutableDefaultDictionary<TKey, TValue>, Whole> wrap;
  
  private readonly ImmutableDefaultDictionary<TKey, TValue> oldDictionary;
  
  private readonly TKey oldKey;

  public TValue value { get => oldDictionary[oldKey]; }

  public ImmutableDefaultDictionaryValueLens(Func<ImmutableDefaultDictionary<TKey, TValue>, Whole> wrap, ImmutableDefaultDictionary<TKey, TValue> oldDictionary, TKey oldKey) {
    // TODO: check that key exists.
    this.wrap = wrap;
    this.oldDictionary = oldDictionary;
    this.oldKey = oldKey;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  // public ILens<ImmutableDefaultDictionary<T>,Whole> sub-part => oldHole.sub-part.ChainLens(value => oldHole.with-sub-part(value));

  public Whole Update(Func<TValue, TValue> update) {
    var oldValue = oldDictionary[oldKey];
    return wrap(oldDictionary.SetItem(oldKey, update(oldValue)));
  }

  public ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole> UpdateKey(Func<TKey, TKey> update) {
    var newKey = update(oldKey);
    return new ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole>(
      wrap,
      oldDictionary.Remove(oldKey).Add(newKey, oldDictionary[oldKey]),
      newKey);
  }
}

public sealed class ImmutableDefaultDictionaryLens<TKey, TValue, Whole> : ILens<ImmutableDefaultDictionary<TKey, TValue>, Whole> {
  public readonly Func<ImmutableDefaultDictionary<TKey, TValue>, Whole> wrap;
  private readonly ImmutableDefaultDictionary<TKey, TValue> oldHole;

  public ImmutableDefaultDictionary<TKey, TValue> value { get => oldHole; }

  public ImmutableDefaultDictionaryLens(Func<ImmutableDefaultDictionary<TKey, TValue>, Whole> wrap, ImmutableDefaultDictionary<TKey, TValue> oldHole) {
    // TODO: check that key exists.
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  public ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole> this[TKey key] {
    get => new ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole>(wrap, oldHole, key);
  }
  
  public Whole Update(Func<ImmutableDefaultDictionary<TKey, TValue>, ImmutableDefaultDictionary<TKey, TValue>> update) {
    return wrap(update(oldHole));
  }
}

public static class ImmutableDefaultDictionaryLensExtensionMethods {
  public static ImmutableDefaultDictionaryLens<TKey, TValue, Whole>
    ChainLens<TKey, TValue, Whole>(
      this ImmutableDefaultDictionary<TKey, TValue> hole,
      System.Func<ImmutableDefaultDictionary<TKey, TValue>, Whole> wrap)
    => new ImmutableDefaultDictionaryLens<TKey, TValue, Whole>(wrap: wrap, oldHole: hole);

  // this is a shorthand since we don't have extension properties
  public static ImmutableDefaultDictionaryValueLens<TKey, TValue, ImmutableDefaultDictionary<TKey, TValue>>
    lens<TKey, TValue>(
      this ImmutableDefaultDictionary<TKey, TValue> d,
      TKey key)
    => d.lens[key];

  public static ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole>
    UpdateKey<TKey, TValue, Whole>(
      this ImmutableDefaultDictionaryValueLens<TKey, TValue, Whole> lens,
      TKey newKey)
    => lens.UpdateKey(oldKey => newKey);
}