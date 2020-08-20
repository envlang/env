using System;
using System.Collections.Immutable;

public sealed class ImmutableListLens<T, Whole> : ILens<ImmutableList<T>, Whole> {
  public readonly Func<ImmutableList<T>, Whole> wrap;
  private readonly ImmutableList<T> oldHole;

  public ImmutableList<T> value { get => oldHole; }

  public ImmutableListLens(Func<ImmutableList<T>, Whole> wrap, ImmutableList<T> oldHole) {
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  // public ILens<ImmutableList<T>,Whole> sub-part
  //   => oldHole.sub-part.ChainLens(value => oldHole.with-sub-part(value));

  public Whole Update(Func<ImmutableList<T>, ImmutableList<T>> update) => wrap(update(oldHole));
}

public static class ImmutableListLensExtensionMethods {
  public static ImmutableListLens<T, Whole>
    ChainLens<T, Whole>(
      this ImmutableList<T> hole,
      System.Func<ImmutableList<T>, Whole> wrap)
    => new ImmutableListLens<T, Whole>(wrap: wrap, oldHole: hole);

  public static ImmutableListLens<T, ImmutableList<T>>
    lens<T>(
      this ImmutableList<T> d)
    => d.ChainLens(x => x);
}
