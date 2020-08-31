using System;
using System.Linq;
using System.Collections.Generic;
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

  public static string Str<T>(this ImmutableHashSet<string> h)
    => $"ImmutableHashSet({h.Select(x => x.Str<string>()).JoinWith(", ")})";

  public static string Str<T>(this IEnumerable<MixFix.Grammar2> e)
    => $"IEnumerabl({e.Select(x => x.Str<MixFix.Grammar2>()).JoinWith(", ")})";

  public static string Str<T>(this IEnumerable<Ast.AstNode> e)
    => $"IEnumerab({e.Select(x => x.Str<Ast.AstNode>()).JoinWith(", ")})";

  public static string Str<T>(this IEnumerable<Ast.ParserResult> e)
    => $"IEnumerab({e.Select(x => x.Str<Ast.ParserResult>()).JoinWith(", ")})";

  public static string Str<Grammar>(this ImmutableDictionary<string,Grammar> h)
    => $"ImmutableDictionary(\n{h.Select(x => $"  {x.Key.Str<string>()}:{x.Value.Str<Grammar>()}").JoinWith(",\n")}\n)";

  public static string Str<T>(this string s) => $"\"{s}\"";

  public static string Str<T>(this object o) => ""+o.ToString();
}