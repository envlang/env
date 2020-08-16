using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using S = Lexer.S;
using static Global;

public static class Parser {
  public static Ast.Expr Parse(string source) {
    foreach (var lexeme in Lexer.Lex(source)) {
      return lexeme.state.Match(
        Int: () => Ast.Expr.Int(Int32.Parse(lexeme.lexeme)),
        String: () => Ast.Expr.String(lexeme.lexeme),
        Space: () => throw new NotImplementedException(), // ignore
        End: () => throw new NotImplementedException(),
        Decimal: () => throw new NotImplementedException(),
        StringOpen: () => throw new NotImplementedException(),
        StringClose: () => throw new NotImplementedException()
      );
    }
    throw new Exception("empty file, rm this when consuming the whole stream of lexemes.");
  }
}