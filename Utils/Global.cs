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

  public static Unit unit() => Unit.unit;

  public static Option<T> None<T>() => Option.None<T>();

  public static ImmutableList<T> ImmutableList<T>(params T[] xs)
    => xs.ToImmutableList();
}