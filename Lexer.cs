using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Immutable;
using System.Globalization;
using C = System.Globalization.UnicodeCategory;
using static Global;

public static partial class Lexer {
  public sealed class EOF { }

  public static class Rules {
    private static Rule Rule(S oldState, UnicodeCategory cat, S throughState, S newState = null)
      => new Rule(
           oldState: oldState,
           description: cat.ToString(),
           test: c => c.codePoints
                       .First()
                      .Match(some: (x => x.UnicodeCategory(0) == cat),
                             none: false),
           throughState: throughState,
           newState: newState ?? throughState);

    private static Rule Rule(S oldState, EOF eof, S throughState, S newState = null)
      => new Rule(
           oldState: oldState,
           description: "End of file",
           test: c => c.endOfFile,
           throughState: throughState,
           newState: newState ?? throughState);

    private static string CharDescription(char c)
      => (c == '"') ? "'\"'" : $"\"{c.ToString()}\"";

    private static Rule Rule(S oldState, char c, S throughState, S newState = null)
      => new Rule(
           oldState: oldState,
           description: CharDescription(c),
           test: x => x.codePoints
                       .Single()
                       .Match(some: xx => xx == c.ToString(),
                              none: false),
           throughState: throughState,
           newState: newState ?? throughState);

    private static Rule Rule(S oldState, char[] cs, S throughState, S newState = null) {
      var csl = cs.Select(x => x.ToString()).ToImmutableList();
      return new Rule(
        oldState: oldState,
        description: ", ".Join(cs.Select(CharDescription)),
        test: x => x.codePoints
                    .Single()
                    .Match(some: csl.Contains, none: false),
        throughState: throughState,
        newState: newState ?? throughState);
    }

    public static EOF EOF = new EOF();
    public static ImmutableList<Rule> Default = ImmutableList(
      Rule(S.Space,   C.DecimalDigitNumber, S.Int),
      Rule(S.Space,   C.SpaceSeparator,     S.Space),
      Rule(S.Space,   EOF,                  S.End),
      Rule(S.Space,   '"',                  S.StringOpen, S.String),
      Rule(S.Space,   '=',                  S.Eq),
      Rule(S.Eq,      '=',                  S.Eq, S.Space),
      Rule(S.Space,   '&',                  S.And),
      Rule(S.And,     '&',                  S.And, S.Space),
      Rule(S.Space,   '+',                  S.Plus, S.Space),
      Rule(S.Space,   '*',                  S.Times, S.Space),
      // TODO: https://unicode.org/reports/tr31/#D1 has a lot to say about
      // identifiers
      Rule(S.Space,   C.LowercaseLetter,    S.Ident),
      Rule(S.Space,   C.UppercaseLetter,    S.Ident),
      Rule(S.Space,   C.TitlecaseLetter,    S.Ident),
      Rule(S.Space,   C.ModifierLetter,     S.Ident),
      Rule(S.Space,   C.OtherLetter,        S.Ident),
      Rule(S.Space,   C.LetterNumber,       S.Ident),

      Rule(S.Ident,   C.LowercaseLetter,    S.Ident),
      Rule(S.Ident,   C.UppercaseLetter,    S.Ident),
      Rule(S.Ident,   C.TitlecaseLetter,    S.Ident),
      Rule(S.Ident,   C.ModifierLetter,     S.Ident),
      Rule(S.Ident,   C.OtherLetter,        S.Ident),
      Rule(S.Ident,   C.LetterNumber,       S.Ident),

      Rule(S.Int,     C.DecimalDigitNumber, S.Int),
      Rule(S.Int,     C.SpaceSeparator,     S.Space),
      Rule(S.Int,     new[]{'.',','},       S.Decimal, S.Int),
      Rule(S.Decimal, C.SpaceSeparator,     S.Space),
      
      Rule(S.String,  C.LowercaseLetter,    S.String),
      Rule(S.String,  C.UppercaseLetter,    S.String),
      Rule(S.String,  C.DecimalDigitNumber, S.String),
      Rule(S.String,  '"',                  S.StringClose, S.Space)
    );

    public static ImmutableDefaultDictionary<S, List<Rule>> Dict =
      Default
        .GroupBy(r => r.oldState, r => r)
        .ToImmutableDefaultDictionary(
          new List<Rule>(),
          rs => rs.Key,
          rs => rs.ToList()) ;

    // This adds transitions through an implicit empty whitespace.
    public static ImmutableDefaultDictionary<S, List<Rule>> WithEpsilonTransitions =
      Dict.ToImmutableDefaultDictionary(
        new List<Rule>(),
        kv => kv.Key,
        kv => kv.Value.Any(r => true) // r.test(" ")
              // This is a bit of a hack, the lexer tries the rules in
              // order so later rules with different results are masked
              // by former rules
              ? kv.Value.Concat(Dict[S.Space]).ToList()
              : kv.Value);
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

  public static ParserErrorException ParserError(StringBuilder context, IEnumerator<GraphemeCluster> stream, S state, List<Rule> possibleNext, GraphemeCluster gc) {
    var rest =
      stream
        .SingleUseEnumerable()
        .TakeUntil(c => c.str.StartsWith("\n"))
        .Select(c => c.str)
        .JoinWith("");

    var expected = ", ".Join(possibleNext.Select(p => p.description));
    var actual = (gc.endOfFile ? "" : "grapheme cluster ") + gc.Description();
    var cat = gc.codePoints
                 .First()
                 .Match(some: (x => x.UnicodeCategory(0).ToString()),
                        none: "None (empty string)");
    return new ParserErrorException(
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
      var possibleNext = Rules.WithEpsilonTransitions[state];
      yield return
        possibleNext
          .First(r => r.test(c))
          .IfSome(rule => Transition(ref state, ref lexeme, c, rule))
          .ElseThrow(() => ParserError(context, e, state, possibleNext, c));
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