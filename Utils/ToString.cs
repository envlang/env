using System;
using System.Linq;
using System.Collections.Immutable;

public interface IString {
  string Str();
}

public static class ToStringImplementations {
  // These allow string conversion for uncooperative classes
  // as long as their type arguments are cooperative.

  // For some reason Str<object> takes precedence over this…
  public static string Str<T>(this ImmutableList<T> l)
    where T : IString
    => $"ImmutableList({l.Select(x => x.Str()).JoinWith(", ")})";

  // …but not over this:
  public static string Str<T>(this ImmutableList<MixFix.Operator> l)
    => $"ImmutableList({l.Select(x => x.Str()).JoinWith(", ")})";

  public static string Str<T>(this ImmutableList<MixFix.Part> l)
    => $"ImmutableList({l.Select(x => x.Str()).JoinWith(", ")})";

  public static string Str<T>(this ImmutableHashSet<String> h)
    => $"ImmutableHashSet({h.Select(x => x.Str<String>()).JoinWith(", ")})";

  public static string Str<T>(this string s) => $"\"{s}\"";

  public static string Str<T>(this object o) => ""+o.ToString();
}