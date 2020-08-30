using System;
using System.Text;
using System.Collections.Immutable;
using Immutable;

public static class Global {
  public static File File(string str) => new File(str);
  public static Ext  Ext (string str) => new Ext (str);
  public static Dir  Dir (string str) => new Dir (str);
  public static Exe  Exe (string str) => new Exe (str);
  
  public static void Log (string str) => Console.WriteLine(str);

  public static Unit unit { get => Unit.unit; }

  public static Option<T> None<T>() => Option.None<T>();

  public static ImmutableList<T> ImmutableList<T>(params T[] xs)
    => xs.ToImmutableList();

  public static ImmutableHashSet<T> ImmutableHashSet<T>(params T[] xs)
    => xs.ToImmutableHashSet();

  public static T To<T>(this T x) => x;

  public static A FoldWhileSome<A>(A init, Func<A, Option<A>> f)
    => Collection.FoldWhileSome(init, f);

  public static Option<Tuple<A, B>> FoldWhileSome<A, B>(Option<Tuple<A, B>> init, Func<A, B, Option<Tuple<A, B>>> f)
    => Collection.FoldWhileSome(init, f);

  public static IImmutableEnumerator<T> Empty<T>()
    => ImmutableEnumeratorExtensionMethods.Empty<T>();
}