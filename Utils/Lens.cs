// Code quality of this file: low.

using System;
using System.Collections.Immutable;

public interface ILens<Hole, Whole> {
  Whole Update(Func<Hole, Hole> update);
}

public sealed class ImmutableListLens<T, Whole> : ILens<ImmutableList<T>, Whole> {
  public readonly System.Func<ImmutableList<T>, Whole> wrap;
  public readonly ImmutableList<T> oldHole;

  public ImmutableListLens(System.Func<ImmutableList<T>, Whole> wrap, ImmutableList<T> oldHole) {
    this.wrap = wrap;
    this.oldHole = oldHole;
  }

  // Put methods with the following signature here to focus on sub-parts of the list as needed.
  // public ILens<ImmutableList<T>,Whole> sub-part => oldHole.sub-part.ChainLens(value => oldHole.with-sub-part(value));

  public Whole Update(Func<ImmutableList<T>, ImmutableList<T>> update) => wrap(update(oldHole));
}

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

public static class LensExtensionMethods {
  public static Whole Update<Hole, Whole>(this ILens<Hole, Whole> lens, Hole newHole)
    => lens.Update(oldHole => newHole);

  public static Whole Cons<T, Whole>(this ILens<ImmutableList<T>, Whole> lens, T value)
    => lens.Update(oldHole => oldHole.Cons(value));

  public static ImmutableListLens<T, Whole>
    ChainLens<T, Whole>(
      this ImmutableList<T> hole,
      System.Func<ImmutableList<T>, Whole> wrap)
    => new ImmutableListLens<T, Whole>(wrap: wrap, oldHole: hole);

  public static ILens<string, Whole> ChainLens<Whole>(this string hole, System.Func<string, Whole> wrap) => new LeafLens<string, Whole>(wrap: wrap, oldHole: hole);

  public static ILens<Func<GraphemeCluster,bool>, Whole> ChainLens<Whole>(this Func<GraphemeCluster,bool> hole, System.Func<Func<GraphemeCluster,bool>, Whole> wrap) => new LeafLens<Func<GraphemeCluster,bool>, Whole>(wrap: wrap, oldHole: hole);

  public class FocusableLeaf<T> {
    private readonly T value;
    public FocusableLeaf(T value) { this.value = value; }
    public LeafLens<T, Whole> ChainLens<Whole>(Func<T, Whole> wrap)
      => new LeafLens<T, Whole>(wrap: wrap, oldHole: value);
  }

  public static ILens<ImmutableList<string>, Whole> ChainLens<Whole>(this ImmutableList<string> hole, System.Func<ImmutableList<string>, Whole> wrap) => new ImmutableListLens<string, Whole>(wrap: wrap, oldHole: hole);
}