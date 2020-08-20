using System;
using System.Collections.Immutable;

// Lenses for primitive types and other types that are not
// interesting to further focus.
public sealed class LeafLens<T, Whole> : ILens<T, Whole> {
  public readonly System.Func<T, Whole> wrap;
  public readonly T oldHole;

  public LeafLens(System.Func<T, Whole> wrap, T oldHole) {
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  public Whole Update(Func<T, T> update) => wrap(update(oldHole));
}