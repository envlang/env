using System;
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
                      .Match(
                        Some: x =>
                          x.UnicodeCategory(0) == cat,
                        None: false),
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
                       .Match(
                         Some: xx => xx == c.ToString(),
                         None: false),
           throughState: throughState,
           newState: newState ?? throughState);

    private static Rule Rule(S oldState, char[] cs, S throughState, S newState = null) {
      var csl = cs.Select(x => x.ToString()).ToImmutableList();
      return new Rule(
        oldState: oldState,
        description: ", ".Join(cs.Select(CharDescription)),
        test: x => x.codePoints
                    .Single()
                    .Match(Some: csl.Contains,
                           None: false),
        throughState: throughState,
        newState: newState ?? throughState);
    }

    public static EOF EOF = new EOF();
    public static ImmutableList<Rule> Default = ImmutableList(
      Rule(S.Space,   C.DecimalDigitNumber, S.Int),
      Rule(S.Space,   C.SpaceSeparator,     S.Space),
      Rule(S.Space,   EOF,                  S.EndOfInput, S.End),
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

  public partial class Lexeme {
    private string CustomToString() {
      return $"new Lexeme({state}, \"{lexeme}\")";
    }
  }

  private static ValueTuple<S, string, IImmutableEnumerator<Lexeme>> Transition(S state, string lexeme, GraphemeCluster c, Rule rule) {
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
    return (state, lexeme, result.GetImmutableEnumerator());
  }

  public static ParserErrorException ParserError(IImmutableEnumerator<GraphemeCluster> context, IImmutableEnumerator<GraphemeCluster> rest, S state, List<Rule> possibleNext, GraphemeCluster gc) {
    var strContext =
      context
        .ToIEnumerable()
        .TakeUntil(c => c.Equals(rest))
        .Select(c => c.str)
        .JoinWith("");

    var strRest =
      rest
        .TakeUntil(c => c.str.StartsWith("\n"))
        .Select(c => c.str)
        .JoinWith("");

    var expected = ", ".Join(possibleNext.Select(p => p.description));
    var actual = (gc.endOfFile ? "" : "grapheme cluster ") + gc.Description();
    var cat = gc.codePoints
                 .First()
                 .Match(Some: x => x.UnicodeCategory(0).ToString(),
                        None: "None (empty string)");
    return new ParserErrorException(
      $"Unexpected {actual} (Unicode category {cat}) while the lexer was in state {state}: expected one of {expected}{Environment.NewLine}{strContext}  <--HERE  {strRest}"
    );
  }

  // fake Unicode category
  private const UnicodeCategory EndOfFile = (UnicodeCategory)(-1);

  public static U Foo<T, U>(T x, Func<T, U> f) => f(x);

  [F]
  private partial class Flub {
    public ValueTuple<ValueTuple<string, S, IImmutableEnumerator<GraphemeCluster>>, IImmutableEnumerator<Lexeme>> F(
      ValueTuple<string, S, IImmutableEnumerator<GraphemeCluster>> t,
      ValueTuple<GraphemeCluster, IImmutableEnumerator<GraphemeCluster>> cur
    )
    {
      var (lexeme, state, context) = t;
      var (c, current) = cur;
      var possibleNext = Rules.WithEpsilonTransitions[state];

      return
        possibleNext
          .First(r => r.test(c))
          .IfSome(rule => {
            var r = Transition(state, lexeme, c, rule);
            var newState = r.Item1;
            var newLexeme = r.Item2;
            var tokens = r.Item3;
            return ((newLexeme, newState, context), tokens);
          })
          .ElseThrow(() => ParserError(context, current, state, possibleNext, c));
    }
  }

  public static IImmutableEnumerator<IImmutableEnumerator<Lexeme>> Lex2(IImmutableEnumerator<GraphemeCluster> ie) {
    var lexeme = "";
    var state = S.Space;
    // In a REPL we could reset the context at the end of each statement.
    // We could also reset the context to the containing line or function when processing files.
    var context = ie;

    return ie.SelectAggregate((lexeme, state, context), Flub.Eq);
  }

  public static IImmutableEnumerator<IImmutableEnumerator<Lexeme>>
    Lex1(string source)
    => Lex2(source.TextElements().GetImmutableEnumerator());

  [F]
  private partial class SkipInitialEmptyWhitespace {
    public IImmutableEnumerator<Lexeme> F(
      IImmutableEnumerator<Lexeme> lx
    )
    => lx.FirstAndRest().Match(
      Some: hdtl =>
        // skip the initial empty whitespace
          "".Equals(hdtl.Item1.lexeme)
        ? hdtl.Item2
        : hdtl.Item1.ImSingleton().Concat(hdtl.Item2),
      None: Empty<Lexeme>());
  }

  // TODO: move this to a .Filter() extension method.
  [F]
  private partial class DiscardWhitespace {
    public IImmutableEnumerator<Lexeme> F(
      IImmutableEnumerator<Lexeme> lx
    )
    => lx.FirstAndRest().Match<Tuple<Lexeme, IImmutableEnumerator<Lexeme>>, IImmutableEnumerator<Lexeme>>(
      Some: hdtl => {
        var rest = hdtl.Item2.Lazy(DiscardWhitespace.Eq);
        return hdtl.Item1.state.Equals(S.Space)
          ? rest
          : hdtl.Item1.ImSingleton().Concat(rest);
      },
      None: Empty<Lexeme>());
  }

  public static IImmutableEnumerator<Lexeme> Lex(string source)
    => new Lexeme(S.StartOfInput, "").ImSingleton()
         .Concat(
            Lex1(source)
            .Flatten()
            //.Lazy(SkipInitialEmptyWhitespace.Eq)
            .Lazy(DiscardWhitespace.Eq));
}