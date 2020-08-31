using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Immutable;
using Ast;
using S = Lexer.S;
using Lexeme = Lexer.Lexeme;
using Grammar2 = MixFix.Grammar2;
using static Global;

public static partial class Parser {
  public static Option<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>> Parse3(
    Func<IImmutableEnumerator<Lexeme>,
         Grammar2,
         //Option<IImmutableEnumerator<Lexeme>>
         Option<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>>
         >
      Parse3,
    IImmutableEnumerator<Lexeme> tokens,
    Grammar2 grammar
    ) =>
    tokens
      .FirstAndRest()
      .Match(
        None: () =>
          //throw new Exception("EOF, what to do?"),
          None<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>>(),
        Some: firstRest => {
          var first = firstRest.Item1;
          var rest = firstRest.Item2;
          Log(first.lexeme);
          Log(grammar.ToString());
          Log(grammar.Match(
            RepeatOnePlus: _ => "RepeatOnePlus",
            Or: _ => "Or",
            Sequence: _ => "Sequence",
            Terminal: t => "Terminal:"+t.ToString()));
          return grammar.Match(
            RepeatOnePlus: g =>
              rest.FoldMapWhileSome(restI => Parse3(restI, g))
                .If<IImmutableEnumerator<Lexeme>, IEnumerable<AstNode>>((restN, nodes) => nodes.Count() > 1)
                .IfSome((restN, nodes) => (restN, AstNode.Operator(nodes))),
            // TODO: to check for ambiguous parses, we can use
            // .Single(…) instead of .First(…).
            Or: l =>
              l.First(g => Parse3(rest, g)),
            Sequence: l => {
              return l.BindFoldMap(rest, (restI, g) => Parse3(restI, g))
               .IfSome((restN, nodes) => {
                 Log($"{nodes.Count()}/{l.Count()}");
                 return (restN, AstNode.Operator(nodes));
               });
            },
            Terminal: t =>
              first.state.Equals(t)
              ? (rest,
                 AstNode.Terminal(/* TODO: */ Expr.String(rest.ToString())))
                .Some()
              : None<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>>()
          );
          // TODO: at the top-level, check that the lexemes
          // are empty if the parser won't accept anything else.
        }
      );

  public static Option<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>> Parse2(string source) {
    Grammar2 grammar =
      DefaultGrammar.DefaultPrecedenceDAG.ToGrammar2();
    //Log(grammar.Str());

    var P = Func.YMemoize<
              IImmutableEnumerator<Lexeme>,
              Grammar2,
              Option<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>>>(
      Parse3
    );

    return P(Lexer.Lex(source), grammar);
  }

  public static Ast.Expr Parse(string source) {
    Log("");
    Log("" + Parse2(source).ToString());
    Log("");
    Environment.Exit(0);

    return Lexer.Lex(source)
      .SelectMany(lexeme =>
        lexeme.state.Match(
          Int: () => Ast.Expr.Int(Int32.Parse(lexeme.lexeme)).Singleton(),
          String: () => Ast.Expr.String(lexeme.lexeme).Singleton(),
          Ident: () => Enumerable.Empty<Ast.Expr>(), // TODO
          And: () => Enumerable.Empty<Ast.Expr>(), // TODO
          Plus: () => Enumerable.Empty<Ast.Expr>(), // TODO
          Times: () => Enumerable.Empty<Ast.Expr>(), // TODO
          Space: () => Enumerable.Empty<Ast.Expr>(), // ignore
          Eq: () => Enumerable.Empty<Ast.Expr>(), // TODO
          End: () => Enumerable.Empty<Ast.Expr>(), // TODO
          Decimal: () => Enumerable.Empty<Ast.Expr>(), // TODO
          StringOpen: () => Enumerable.Empty<Ast.Expr>(), // TODO
          StringClose: () => Enumerable.Empty<Ast.Expr>()
        )
      )
      .Single()
      .ElseThrow(() => new ParserErrorException(
        "empty file or more than one expression in file."));
  }

  public static void RecursiveDescent(IEnumerable<Lexeme> e) {

  }
}

// Notes:

// (a, b, c) is parsed as (expr (paren (expr comma (expr a) (expr comma (expr b) (expr c))))) where expr is a run-time wrapper allowing e.g. passing an explicit environment or (useful in this case) distinguish between a tuple-value referenced by c and a paren expression. In contrast, (a, (b, c)) is parsed as (expr (paren (expr comma (expr a) (expr paren (expr comma (expr b) (expr c))))))

// (a < b <= c < d > e) is parsed similarly as the sequence of commas, allowing the comparison operators to compare their predecessor instead of the boolean output value.

// ("if" condition "then" clause) returns a boolean-like value, indicating what the original condition was. It's as simple as (operator ("if" condition "then" clause) = real_if condition real_then { clause with condition_was = true } real_else { condition_was = false }). (ifthen "else" clause) is just a binary operator.

// It is also possible to have the "else" operator taks an AST as its left operand, and inspect it to extract and rewrite the "if".

// -3 is recognized by the lexer, but -x is not allowed. Otherwise f -x is ambiguous, could be f (-x) or (f) - (x)

// relaxed unicity: the symbols must not appear in other operators of the same namespace nor as the closing bracket symbols which delimit the uses of this namespace in closed operators. Rationale: once the closing bracket is known, if the entire sub-expression doesn't include that bracket then the parser can fast-forward until the closing bracket, only caring about matching open and close symbols which may delimit sub-expressions with different namespaces, and know that whatever's inside is unambiguous.

// Future: lex one by one to allow extending the grammar & lexer; when a new symbol is bound, re-start parsing from the start of the binding form and check that the parsing does find the same new binding at the same position. E.g. (a op b where "op" x y = x * y) is okay, but (a op b where "where" str = stuff) is not, because during the second pass, the unquoted where token does not produce a binding form anymore. E.g (a op b w/ "op" x y = x * y where "w/" = where) is okay, because during the first pass the w/ is treated as garbage, during the second pass it is treated as a binding form, but the where token which retroactively extended the grammar still parsed as the same grammar extension. In other words, re-parsing can rewrite part of the AST below the binding node, but the binding node itself should be at the same position (this includes the fact that it shouldn't be moved with respect to its ancestor AST nodes).