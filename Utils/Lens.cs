using System;
using System.Collections.Immutable;

public interface ILens<Hole, Whole> {
  Hole value { get; }
  Whole Update(Func<Hole, Hole> update);
}

public static class LensExtensionMethods {
  public static Whole Update<Hole, Whole>(this ILens<Hole, Whole> lens, Hole newHole)
    => lens.Update(oldHole => newHole);

  public static Whole Cons<T, Whole>(this T value, ILens<ImmutableList<T>, Whole> lens)
    => lens.Update(oldHole => value.Cons(oldHole));

  public static ILens<string, Whole> ChainLens<Whole>(this string hole, System.Func<string, Whole> wrap) => new LeafLens<string, Whole>(wrap: wrap, oldHole: hole);

  public static ILens<Func<GraphemeCluster,bool>, Whole> ChainLens<Whole>(this Func<GraphemeCluster,bool> hole, System.Func<Func<GraphemeCluster,bool>, Whole> wrap) => new LeafLens<Func<GraphemeCluster,bool>, Whole>(wrap: wrap, oldHole: hole);

  public class FocusableLeaf<T> {
    private readonly T value;
    public FocusableLeaf(T value) { this.value = value; }
    public LeafLens<T, Whole> ChainLens<Whole>(Func<T, Whole> wrap)
      => new LeafLens<T, Whole>(wrap: wrap, oldHole: value);
  }
}
