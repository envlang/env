using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Immutable;
using S = Lexer.S;
using static Global;

public static class Parser {
  public static Ast.Expr Parse(string source) {
    return Lexer.Lex(source)
      .SelectMany(lexeme =>
        lexeme.state.Match(
          Int: () => Ast.Expr.Int(Int32.Parse(lexeme.lexeme)).Singleton(),
          String: () => Ast.Expr.String(lexeme.lexeme).Singleton(),
          Space: () => Enumerable.Empty<Ast.Expr>(), // ignore
          End: () => Enumerable.Empty<Ast.Expr>(),
          Decimal: () => Enumerable.Empty<Ast.Expr>(),
          StringOpen: () => Enumerable.Empty<Ast.Expr>(),
          StringClose: () => Enumerable.Empty<Ast.Expr>()
        )
      )
      .Single()
      .ElseThrow(() => new Exception("empty file or more than one expression in file."));
  }
}

// Notes:

// (a, b, c) is parsed as (expr (paren (expr comma (expr a) (expr comma (expr b) (expr c))))) where expr is a run-time wrapper allowing e.g. passing an explicit environment or (useful in this case) distinguish between a tuple-value referenced by c and a paren expression. In contrast, (a, (b, c)) is parsed as (expr (paren (expr comma (expr a) (expr paren (expr comma (expr b) (expr c))))))

// (a < b <= c < d > e) is parsed similarly as the sequence of commas, allowing the comparison operators to compare their predecessor instead of the boolean output value.

// ("if" condition "then" clause) returns a boolean-like value, indicating what the original condition was. It's as simple as (operator ("if" condition "then" clause) = real_if condition real_then { clause with condition_was = true } real_else { condition_was = false }). (ifthen "else" clause) is just a binary operator.

// It is also possible to have the "else" operator taks an AST as its left operand, and inspect it to extract and rewrite the "if".

// -3 is recognized by the lexer, but -x is not allowed. Otherwise f -x is ambiguous, could be f (-x) or (f) - (x)

// relaxed unicity: the symbols must not appear in other operators of the same namespace nor as the closing bracket symbols which delimit the uses of this namespace in closed operators. Rationale: once the closing bracket is known, if the entire sub-expression doesn't include that bracket then the parser can fast-forward until the closing bracket, only caring about matching open and close symbols which may delimit sub-expressions with different namespaces, and know that whatever's inside is unambiguous.