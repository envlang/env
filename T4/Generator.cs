// Code quality of this file: medium.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public enum Kind {
  Record,
  Variant,
}

public static class Generator {
  public static void Generate(string outputFile, string singleHeader, string header, string footer, string qualifier, ImmutableDictionary<string, Tuple<Kind, ImmutableDictionary<string, string>>> types) {
    using (var o = new System.IO.StreamWriter(outputFile)) {
      Action<string> w = o.WriteLine;
      w("// This file was generated by T4/Generator.cs");
      w("");

      w("using System;");
      w("using Immutable;");
      w($"{singleHeader}");
      foreach (var type in types) {
        var name = type.Key;
        var kind = type.Value.Item1;
        var components = type.Value.Item2;
        switch (kind) {
          case Kind.Record:
            w.RecordUsing(header, footer, qualifier, name, @components);
            break;
          case Kind.Variant:
            w.VariantUsing(header, footer, qualifier, name, @components);
            break;
        }
      }
      w("");
      foreach (var type in types) {
        var name = type.Key;
        var kind = type.Value.Item1;
        var components = type.Value.Item2;
        switch (kind) {
          case Kind.Record:
            w.Record(header, footer, qualifier, name, @components);
            break;
          case Kind.Variant:
            w.Variant(header, footer, qualifier, name, @components);
            break;
        }
        w("");
      }
    }
  }

  // Below are shorthands for making the last argument to Generate().
  public static ImmutableDictionary<string, Tuple<Kind, ImmutableDictionary<string, string>>> Types(params Tuple<string, Tuple<Kind, ImmutableDictionary<string, string>>>[] types)
    => types.ToImmutableDictionary(t => t.Item1, t => t.Item2);

  public static Tuple<string, Tuple<Kind, ImmutableDictionary<string, string>>> Record(string name, params Tuple<string, string>[] fields)
    => new Tuple<string, Tuple<Kind, ImmutableDictionary<string, string>>>(
         name,
         new Tuple<Kind, ImmutableDictionary<string, string>>(
           Kind.Record,
           fields.ToImmutableDictionary(t => t.Item1, t => t.Item2)));

  public static Tuple<string, Tuple<Kind, ImmutableDictionary<string, string>>> Variant(string name, params Tuple<string, string>[] cases)
    => new Tuple<string, Tuple<Kind, ImmutableDictionary<string, string>>>(
         name,
         new Tuple<Kind, ImmutableDictionary<string, string>>(
           Kind.Variant,
           cases.ToImmutableDictionary(t => t.Item1, t => t.Item2)));

  public static Tuple<string, string> Field(string type, string name)
    => new Tuple<string, string>(name, type);

  public static Tuple<string, string> Case(string type, string name)
    => new Tuple<string, string>(name, type);

  public static Tuple<string, string> Case(string name)
    => new Tuple<string, string>(name, null);
}