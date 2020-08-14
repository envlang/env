using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using C = System.Globalization.UnicodeCategory;
using static Global;

public static class Parser {
  public enum S {
    End,
    Space,
    Int,
    Decimal,
    String,
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

  // Transition
  private static IEnumerable<Lexeme> T(ref S state, ref string lexeme, GraphemeCluster c, S newState) {
    if (newState != state) {
      var toReturn = new Lexeme(state, lexeme);
      state = newState;
      lexeme = "";
      lexeme += c.str;
      return toReturn.Singleton();
    } else {
      lexeme += c.str;
      return Enumerable.Empty<Lexeme>();
    }
  }

  public static void ParseError(StringBuilder context, IEnumerator<GraphemeCluster> stream) {
    var rest =
      stream
        .SingleUseEnumerable()
        .TakeUntil(c => c.str.StartsWith("\n"))
        .Select(c => c.str)
        .Aggregate(new StringBuilder(), Append);

    throw new Exception(
      $"Cannot parse this:{Environment.NewLine}{context}__HERE:__{rest}"
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
      var charCategory =
        c.endOfFile
          ? EndOfFile
          : Char.GetUnicodeCategory(c.codePoints.First(), 0);
      switch (state) {
        case S.Space:
        {
          switch (charCategory) {
            case C.DecimalDigitNumber:
              yield return T(ref state, ref lexeme, c, S.Int);
              break;
            case C.SpaceSeparator:
              yield return T(ref state, ref lexeme, c, S.Space);
              break;
            case EndOfFile:
              yield return T(ref state, ref lexeme, c, S.End);
              break;
            default:
              ParseError(context, e);
              break;
          }
        }
        break;

        case S.Int:
        {
          switch (charCategory) {
            case C.DecimalDigitNumber:
              yield return T(ref state, ref lexeme, c, S.Int);
              break;
            case C.SpaceSeparator:
              yield return T(ref state, ref lexeme, c, S.Space);
              break;
            case EndOfFile:
              yield return T(ref state, ref lexeme, c, S.End);
              break;
            default:
              ParseError(context, e);
              break;
          }
        }
        break;
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

  public static Ast.Expr Parse(string source) {
    foreach (var lexeme in Lex(source)) {
      switch (lexeme.state) {
        case S.Int:
          return new Ast.Int(Int32.Parse(lexeme.lexeme));
        case S.String:
          return new Ast.String(lexeme.lexeme);
        default:
          throw new NotImplementedException();
      }
    }
    throw new Exception("empty file, rm this when consuming the whole stream of lexemes.");
  }
}