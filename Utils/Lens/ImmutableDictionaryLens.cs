using System;
using System.Collections.Immutable;

public sealed class ImmutableDictionaryValueLens<TKey, TValue, Whole> : ILens<TValue, Whole> {
  public readonly Func<ImmutableDictionary<TKey, TValue>, Whole> wrap;
  public readonly ImmutableDictionary<TKey, TValue> oldDictionary;
  public readonly TKey oldKey;

  public ImmutableDictionaryValueLens(Func<ImmutableDictionary<TKey, TValue>, Whole> wrap, ImmutableDictionary<TKey, TValue> oldDictionary, TKey oldKey) {
    // TODO: check that key exists.
    this.wrap = wrap;
    this.oldDictionary = oldDictionary;
    this.oldKey = oldKey;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  // public ILens<ImmutableList<T>,Whole> sub-part => oldHole.sub-part.ChainLens(value => oldHole.with-sub-part(value));

  public Whole Update(Func<TValue, TValue> update) {
    var oldValue = oldDictionary[oldKey];
    return wrap(oldDictionary.SetItem(oldKey, update(oldValue)));
  }

  public ImmutableDictionaryValueLens<TKey, TValue, Whole> UpdateKey(Func<TKey, TKey> update) {
    var newKey = update(oldKey);
    return new ImmutableDictionaryValueLens<TKey, TValue, Whole>(
      wrap,
      oldDictionary.Remove(oldKey).Add(newKey, oldDictionary[oldKey]),
      newKey);
  }
}

public sealed class ImmutableDictionaryLens<TKey, TValue, Whole> : ILens<ImmutableDictionary<TKey, TValue>, Whole> {
  public readonly Func<ImmutableDictionary<TKey, TValue>, Whole> wrap;
  public readonly ImmutableDictionary<TKey, TValue> oldHole;

  public ImmutableDictionaryLens(Func<ImmutableDictionary<TKey, TValue>, Whole> wrap, ImmutableDictionary<TKey, TValue> oldHole) {
    // TODO: check that key exists.
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  public ImmutableDictionaryValueLens<TKey, TValue, Whole> this[TKey key] {
    get => new ImmutableDictionaryValueLens<TKey, TValue, Whole>(wrap, oldHole, key);
  }
  
  public Whole Update(Func<ImmutableDictionary<TKey, TValue>, ImmutableDictionary<TKey, TValue>> update) {
    return wrap(update(oldHole));
  }
}

public static class ImmutableDictionaryLensExtensionMethods {
  public static ImmutableDictionaryLens<TKey, TValue, ImmutableDictionary<TKey, TValue>> lens<TKey, TValue>(this ImmutableDictionary<TKey, TValue> d)
    => new ImmutableDictionaryLens<TKey, TValue, ImmutableDictionary<TKey, TValue>>(x => x, d);

  public static ImmutableDictionaryValueLens<TKey, TValue, Whole> UpdateKey<TKey, TValue, Whole, Whole>(this ImmutableDictionaryValueLens<TKey, TValue, Whole> lens, TKey newKey)
    => lens.UpdateKey(oldKey => newKey);

  // This would need an IFocusable<TValue> constraint which is hard to get
  //public static ILens<ImmutableDictionary<TKey, ?>, Whole> ChainLens<TKey, Whole>(this ImmutableDictionary<TKey, ?> hole, System.Func<ImmutableDictionary<TKey, ?>, Whole> wrap)
  //  => new ImmutableDictionaryLens<TKey, ?, Whole>(wrap: wrap, oldHole: hole);
}