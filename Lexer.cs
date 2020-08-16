using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Immutable;
using System.Globalization;
using C = System.Globalization.UnicodeCategory;
using static Global;

public static partial class Lexer {
  public class Rule {
    public readonly S oldState;
    public readonly string description;
    public readonly Func<GraphemeCluster, bool> test;
    public readonly S throughState;
    public readonly S newState;

    public Rule(S oldState, string description, Func<GraphemeCluster, bool> test, S throughState, S newState) {
      this.oldState = oldState;
      this.description = description;
      this.test = test;
      this.throughState = throughState;
      this.newState = newState;
    }
  }

  public sealed class EOF { }

  public static class Rules {
    private static Rule Rule(S oldState, UnicodeCategory cat, S throughState, S newState = null)
      => new Rule(
           oldState,
           cat.ToString(),
           c => c.codePoints
                 .First()
                 .Match(some: (x => x.UnicodeCategory(0) == cat),
                        none: false),
           throughState,
           newState ?? throughState);

    private static Rule Rule(S oldState, EOF eof, S throughState, S newState = null)
      => new Rule(
           oldState,
           "End of file",
           c => c.endOfFile,
           throughState,
           newState ?? throughState);

    private static string CharDescription(char c)
      => (c == '"') ? "'\"'" : $"\"{c.ToString()}\"";

    private static Rule Rule(S oldState, char c, S throughState, S newState = null)
      => new Rule(
           oldState,
           CharDescription(c),
           x => x.codePoints
                 .Single()
                 .Match(some: xx => xx == c.ToString(),
                        none: false),
           throughState,
           newState ?? throughState);

    private static Rule Rule(S oldState, char[] cs, S throughState, S newState = null) {
      var csl = cs.Select(x => x.ToString()).ToList();
      return new Rule(
        oldState,
        ", ".Join(cs.Select(CharDescription)),
        x => x.codePoints.Single().Match(some: csl.Contains, none: false),
        throughState,
        newState ?? throughState);
    }

    public static EOF EOF = new EOF();
    public static List<Rule> Default = new List<Rule> {
      Rule(S.Space,   C.DecimalDigitNumber, S.Int),
      Rule(S.Space,   C.SpaceSeparator,     S.Space),
      Rule(S.Space,   EOF,                  S.End),
      Rule(S.Space,   '"',                  S.StringOpen, S.String),

      Rule(S.Int,     C.DecimalDigitNumber, S.Int),
      Rule(S.Int,     C.SpaceSeparator,     S.Space), // epsilon
      Rule(S.Int,     new[]{'.',','},       S.Decimal, S.Int),
      Rule(S.Int,     EOF,                  S.End),   // epsilon
      Rule(S.Decimal, C.SpaceSeparator,     S.Space), // epsilon
      
      Rule(S.String,  C.LowercaseLetter,    S.String),
      Rule(S.String,  C.UppercaseLetter,    S.String),
      Rule(S.String,  C.DecimalDigitNumber, S.String),
      Rule(S.String,  '"',                  S.StringClose, S.Space),
    };

    public static Dictionary<S, List<Rule>> Dict =
      Default
        .GroupBy(r => r.oldState, r => r)
        .ToDictionary(rs => rs.Key, rs => rs.ToList()) ;
    // TODO: upon failure, do an epsilon-transition to the whitespace state, and try again.
  }

  public struct Lexeme {
    public readonly S state;
    // TODO: maybe keep this as a list of grapheme clusters
    public readonly string lexeme;
    public Lexeme(S state, string lexeme) {
      this.state = state;
      this.lexeme = lexeme;
    }
    public override string ToString() {
      return $"new Lexeme({state}, \"{lexeme}\")";
    }
  }

  private static IEnumerable<Lexeme> Transition(ref S state, ref string lexeme, GraphemeCluster c, Rule rule) {
    List<Lexeme> result = new List<Lexeme>();
    if (rule.throughState != state) {
      result.Add(new Lexeme(state, lexeme));
      state = rule.throughState;
      lexeme = "";
    }
    lexeme += c.str;
    if (rule.newState != state) {
      result.Add(new Lexeme(state, lexeme));
      state = rule.newState;
      lexeme = "";
    }
    return result;
  }

  public static void ParseError(StringBuilder context, IEnumerator<GraphemeCluster> stream, S state, List<Rule> possibleNext, GraphemeCluster gc) {
    var rest =
      stream
        .SingleUseEnumerable()
        .TakeUntil(c => c.str.StartsWith("\n"))
        .Select(c => c.str)
        .Aggregate(new StringBuilder(), Append);

    var expected = ", ".Join(possibleNext.Select(p => p.description));
    var actual = (gc.endOfFile ? "" : "grapheme cluster ") + gc.Description();
    var cat = gc.codePoints
                 .First()
                 .Match(some: (x => x.UnicodeCategory(0).ToString()),
                        none: "None (empty string)");
    throw new Exception(
      $"Unexpected {actual} (Unicode category {cat}) while the lexer was in state {state}: expected one of {expected}{Environment.NewLine}{context}  <--HERE  {rest}"
    );
  }

  // fake Unicode category
  private const UnicodeCategory EndOfFile = (UnicodeCategory)(-1);

  public static IEnumerable<IEnumerable<Lexeme>> Lex1(string source) {
    var context = new StringBuilder();
    var lexeme = "";
    var state = S.Space;
    var e = source.TextElements().GetEnumerator();
    while (e.MoveNext()) {
      var c = e.Current;
      context.Append(c.str);
      List<Rule> possibleNext;
      if (Rules.Dict.TryGetValue(state, out possibleNext)) {
        var rule = possibleNext.FirstOrDefault(r => r.test(c));
        if (rule != null) {
          yield return Transition(ref state, ref lexeme, c, rule);
        } else {
          ParseError(context, e, state, possibleNext, c);
        }
      }
    }
  }

  public static IEnumerable<Lexeme> Lex(string source) {
    var first = true;
    foreach (var x in Lex1(source).SelectMany(x => x)) {
      if (first && "".Equals(x.lexeme)) {
        // skip the initial empty whitespace
      } else {
        first = false;
        yield return x;
      }
    }
  }
}