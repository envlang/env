using System;
using System.Collections.Immutable;

public sealed class ImmutableListLens<T, Whole> : ILens<ImmutableList<T>, Whole> {
  public readonly Func<ImmutableList<T>, Whole> wrap;
  public readonly ImmutableList<T> oldHole;

  public ImmutableListLens(Func<ImmutableList<T>, Whole> wrap, ImmutableList<T> oldHole) {
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  // public ILens<ImmutableList<T>,Whole> sub-part => oldHole.sub-part.ChainLens(value => oldHole.with-sub-part(value));

  public Whole Update(Func<ImmutableList<T>, ImmutableList<T>> update) => wrap(update(oldHole));
}

public static class ImmutableListLensExtensionMethods {
  public static ILens<ImmutableList<string>, Whole> ChainLens<Whole>(this ImmutableList<string> hole, System.Func<ImmutableList<string>, Whole> wrap)
    => new ImmutableListLens<string, Whole>(wrap: wrap, oldHole: hole);
}