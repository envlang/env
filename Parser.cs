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
  public static Option<ValueTuple<IImmutableEnumerator<Lexeme>, ParserResult>> Parse3(
    Func<IImmutableEnumerator<Lexeme>,
         Grammar2,
         //Option<IImmutableEnumerator<Lexeme>>
         Option<ValueTuple<IImmutableEnumerator<Lexeme>, ParserResult>>
         >
      Parse3,
    IImmutableEnumerator<Lexeme> tokens,
    Grammar2 grammar
    )
    => grammar.Match(
        RepeatOnePlus: g =>
          tokens.FoldMapWhileSome(restI => Parse3(restI, g))
            .If((restN, nodes) => nodes.Count() > 1)
            .IfSome((restN, nodes) => (restN, ParserResult.Productions(nodes))),
        // TODO: to check for ambiguous parses, we can use
        // .Single(…) instead of .First(…).
        Or: l =>
          l.First(g => Parse3(tokens, g)),
        Sequence: l =>
          l.BindFoldMap(tokens, (restI, g) => Parse3(restI, g))
            .IfSome((restN, nodes) => (restN, ParserResult.Productions(nodes))),
        Terminal: t =>
        // TODO: move the FirstAndRest here!
          tokens
            .FirstAndRest()
            // When EOF is reached, the parser can't accept this derivation.
            .If((first, rest) => first.state.Equals(t))
            .IfSome((first, rest) => (rest, ParserResult.Terminal(first))),
        Annotated: a =>
          // TODO: use the annotation to give some shape to these lists
          Parse3(tokens, a.Item2).IfSome((rest, g) =>
            (rest, ParserResult.Annotated((a.Item1, g)))));
      // TODO: at the top-level, check that the lexemes
      // are empty if the parser won't accept anything else.












    // Variant("ParserResult",
    //   Case("(Annotation, IEnumerable<ParserResult>)", "Annotated"),
    //   Case("Lexer.Lexeme", "Terminal"))

    // ParserResult = A(SamePrecedence, *) | A(Operator, *) | A(Hole, *)

    // Annotated(Hole, lsucc);
    // Annotated(Operator, closed, nonAssoc, prefix, postfix, infixl, infixr)

    // return
    //     // TODO: we can normally remove the ?: checks, as the constructors for grammars
    //     // now coalesce Impossible cases in the correct way.
    //     (closed ? N(closed) : Impossible)
    //   | (nonAssoc ? N( (lsucc, nonAssoc, rsucc) ) : Impossible)
    //   | ((prefix || infixr) ? R( ((prefix | (lsucc, infixr))["+"], rsucc) ) : Impossible)
    //   | ((postfix || infixl) ? L( (lsucc, (postfix || (infixl, rsucc))["+"]) ) : Impossible);

  public static AstNode PostProcess(this ParserResult parserResult) {
    parserResult.Match(
      Annotated: 
    )
    throw new ParserErrorException("TODO:" + parserResult.ToString());
  }

  /*
  public static IEnumerable<ParserResult.Cases.Annotated> FlattenUntilAnnotation(this IEnumerable<ParserResult> parserResults)
    // TODO: SelectMany is probably not very efficient…
    => parserResults.SelectMany(parserResult =>
      parserResult.Match(
        Terminal: t => throw new ParserErrorException($"Internal error: expected Annotated or Productions but got Terminal({t})"),
        Annotated: a => new ParserResult.Cases.Annotated(a).Singleton(),
        Productions: p => p.FlattenUntilAnnotation()));

  // Code quality of this method: Low.
  public static AstNode PostProcess2(ParserResult parserResult)
    => parserResult.Match(
      Annotated: a => {
        if (a.Item1.IsOperator || a.Item1.IsSamePrecedence) {
          return a.Item2.Match(
            Annotated: p => parserResult.PostProcess(),
            Terminal: t => AstNode.Terminal(t),
            Productions: p => parserResult.PostProcess() // This will fail.
          );
        } else {
          throw new ParserErrorException(
            $"Internal error: unexpected annotation {a}, expected Operator(…) inside a part");
        }
      },
      Terminal: t => throw new ParserErrorException($"Internal error: expected Annotated but got {parserResult}"),
      Productions: p =>  throw new ParserErrorException($"Internal error: expected Annotated but got {parserResult}"));

  // Code quality of this method: Low.
  public static AstNode PostProcess(this ParserResult parserResult) {
    var annotated = parserResult
      .AsAnnotated
      .ElseThrow(() => new ParserErrorException($"Internal error: expected Annotated but got {parserResult}"));

    var annotation = annotated.Item1;
    var production = annotated.Item2;

    var associativity = annotation
      .AsSamePrecedence
      .ElseThrow(() => new ParserErrorException($"Internal error: unexpected annotation {annotation}, expected SamePrecedence(…)"));

    return associativity.Match<AstNode>(
      NonAssociative: () => {
        if (production.IsAnnotated) {
          return AstNode.Terminal(production.AsAnnotated.ElseThrow(new Exception("impossible")));
        }

        var prods = production
          .AsProductions
          .ElseThrow(new ParserErrorException($"Internal error: unexpected node {production}, expected a Productions node inside a NonAssociative annotation."))
          .FlattenUntilAnnotation();

        var stk = ImmutableStack<IEnumerable<ParserResult>>.Empty
          .Push(Enumerable.Empty<ParserResult>());

        var fld =
          prods
            .Aggregate(stk, (pending, prod) =>
              prod.value.Item1.Match(
                Hole: () =>
                  pending.Pop().Push(pending.Peek().Concat(prod.value.Item2)),
                Operator: op =>
                  pending.Pop().Push(pending.Peek().Concat(prod.value.Item2))
                    .Push(Enumerable.Empty<ParserResult>()),
                SamePrecedence: p =>
                  throw new ParserErrorException($"Internal error: unexpected annotation {annotation}, expected Hole() or Operator(…)")));
        
        var www = fld.Pop().Pop().Aggregate(
          AstNode.Operator(fld.Pop().Peek().Concat(fld.Peek()).Select(PostProcess2)),
          (right, left) => AstNode.Operator(left.Select(PostProcess2).Concat(right)));
        Log("\n"+www.ToString());

        return www;

        // var sm = prods.SelectMany(prod =>
        //   prod.value.Item1.Match(
        //     Hole: () => Log("Hole", () => new []{42}),
        //     Operator: op => Log("Operator" + op.ToString(), () => new []{42}),
        //     SamePrecedence: p => throw new ParserErrorException($"Internal error: unexpected annotation {annotation}, expected Hole() or Operator(…)")
        //   )
        // ).ToList();
        
        //Log("\n"+op1.ToString());
        //Log("\n"+prods.Select(prod => prod.value.Item1).JoinToStringWith(",\n"));
        //Log("\n"+prods.Select(prod => prod.value.Item2).JoinToStringWith(",\n"));
        //throw new ParserErrorException($"TODO SamePrecedence({associativity})");
      },
      LeftAssociative: () => throw new ParserErrorException($"Internal error: unexpected annotation SamePrecedence({associativity})"),
      RightAssociative: () => throw new ParserErrorException($"Internal error: unexpected annotation SamePrecedence({associativity})")
    );
  }
  */

  public static Option<ValueTuple<IImmutableEnumerator<Lexeme>, AstNode>> Parse2(string source) {
    Grammar2 grammar =
      DefaultGrammar.DefaultPrecedenceDAG.ToGrammar2();
    //Log(grammar.Str());

    var P = Func.YMemoize<
              IImmutableEnumerator<Lexeme>,
              Grammar2,
              Option<ValueTuple<IImmutableEnumerator<Lexeme>, ParserResult>>>(
      Parse3
    );

    return P(Lexer.Lex(source), grammar)
      .IfSome((rest, result) => (rest, PostProcess(result)));
  }

  public static Ast.Expr Parse(string source) {
    Parse2(source).ToString();
    //Log("");
    //Log("" + Parse2(source).ToString());
    //Log("");
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

// Random note: why don't we have named return values, i.e. C#'s "out" with a sane syntax for functional programming? Tuples are a way to do that, but unpacking / repacking them is cumbersome.