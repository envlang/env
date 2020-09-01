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
    //=> Log($"Parser {grammar.ToString()} against {tokens.FirstAndRest().IfSome((first, rest) => first)}", ()
    => grammar.Match(
        RepeatOnePlus: g =>
          tokens.FoldMapWhileSome(restI => Parse3(restI, g))
            .If((restN, nodes) => nodes.Count() >= 1)
            .IfSome((restN, nodes) => (restN, ParserResult.Productions(nodes))),
        // TODO: to check for ambiguous parses, we can use
        // .Single(…) instead of .First(…).
        Or: l => {
          var i = 0;
          return l.First(g => {
            i++;
            //Log($"{i}/{l.Count()}: trying…");
            var res = Parse3(tokens, g);
            //Log($"{i}/{l.Count()}: {res}");
            return res;
          });
        },
        Sequence: l =>
          l.BindFoldMap(tokens, (restI, g) => Parse3(restI, g))
            .IfSome((restN, nodes) => (restN, ParserResult.Productions(nodes))),
        Terminal: t => {
          var attempt = tokens
            .FirstAndRest()
            // When EOF is reached, the parser can't accept this derivation.
            .If((first, rest) => first.state.Equals(t))
            .IfSome((first, rest) => (rest, ParserResult.Terminal(first)));
            /*if (attempt.IsNone) {
              Log($"failed to match {tokens.FirstAndRest().IfSome((first, rest) => first)} against terminal {t}.");
            }*/
            return attempt;
        },
        Annotated: a =>
          Parse3(tokens, a.Item2).IfSome((rest, g) =>
            (rest, ParserResult.Annotated((a.Item1, g)))));

  // We lost some typing information and the structure is scattered around
  // in Annotation nodes. For now gather everything back into the right
  // structure after the fact.
  public static ParserResult2 Gather(this ParserResult parserResult)
    => parserResult
      .AsAnnotated
      .ElseThrow(new ParserErrorException("Internal error: Expected Annotated"))
      .Pipe(a => a.Item1.AsSamePrecedence
        .ElseThrow(new ParserErrorException("Internal error: Expected SamePrecedence"))
        .Pipe(associativity =>
          ParserResult2.SamePrecedence(
            (associativity, a.Item2.GatherOperatorOrHole()))));

  public static IEnumerable<OperatorOrHole> GatherOperatorOrHole(this ParserResult parserResult)
    => parserResult.Match(
      Annotated: a => a.Item1.Match(
        Operator: @operator =>
          OperatorOrHole.Operator(
            (@operator, a.Item2.GatherSamePrecedenceOrTerminal()))
          .Singleton(),
        Hole: () =>
          OperatorOrHole.Hole(
            a.Item2.Gather())
          .Singleton(),
        SamePrecedence: associativity =>
          throw new ParserErrorException("Internal error: Expected Operator or Hole")
        ),
      Productions: p =>
        p.SelectMany(GatherOperatorOrHole),
      Terminal: t =>
        throw new ParserErrorException("Internal error: Expected Annotated or Productions"));

  public static IEnumerable<SamePrecedenceOrTerminal> GatherSamePrecedenceOrTerminal(this ParserResult parserResult)
    => parserResult.Match(
      Annotated: a => a.Item1.Match(
        SamePrecedence: associativity =>
          SamePrecedenceOrTerminal.SamePrecedence(
            parserResult.Gather())
          .Singleton(),
        Hole: () =>
          throw new ParserErrorException("Internal error: Expected SamePrecedence or Terminal"),
        Operator: associativity =>
          throw new ParserErrorException("Internal error: Expected SamePrecedence or Terminal")
        ),
      Productions: p =>
        p.SelectMany(GatherSamePrecedenceOrTerminal),
      Terminal: lexeme =>
        SamePrecedenceOrTerminal.Terminal(lexeme)
          .Singleton());

  // ParserResult2 =
  // | (MixFix.Associativity, IEnumerable<OperatorOrHole>) SamePrecedence
  // OperatorOrHole =
  // | (MixFix.Operator, IEnumerable<SamePrecedenceOrTerminal>) Operator
  // | ParserResult2 Hole
  // SamePrecedenceOrTerminal =
  // | ParserResult2 SamePrecedence
  // | Lexer.Lexeme Terminal

  public static ValueTuple<MixFix.Associativity, IEnumerable<OperatorOrHole>> Get(this ParserResult2 parserResult)
  => parserResult.Match(SamePrecedence: p =>p);

  /*
  public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> e, Func<T, T, bool> predicate) {
    var currentList = ImmutableStack<T>.Empty;
    T prev = default(T);
    var first = true;
    foreach (var x in e) {
      if (first) {
        first = false;
      } else {
        if (predicate(prev, x)) {
          yield return currentList.Reverse();
          currentList = ImmutableStack<T>.Empty;
        }
      }
      currentList = currentList.Push(x);
      prev = x;
    }
    yield return currentList.Reverse();
  }*/

  public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> e, Func<T, bool> predicate) {
    var currentList = ImmutableStack<T>.Empty;
    foreach (var x in e) {
      if (predicate(x)) {
        // yield the elements
        yield return currentList.Reverse();
        currentList = ImmutableStack<T>.Empty;
        // yield the separator
        yield return x.Singleton();
      } else {
        currentList = currentList.Push(x);
      }
    }
    // yield the last (possibly empty) unclosed batch of elements
    yield return currentList.Reverse();
  }

  /*
  // TODO: use an Either<T, U> class instead of an Option with another Func.
  public static IEnumerable<IEnumerable<U>> Split<T, U>(this IEnumerable<T> e, Func<T, Option<U>> predicate, Func<IEnumerable<T>, U> transform) {
    var currentList = ImmutableStack<T>.Empty;
    foreach (var x in e) {
      var p = predicate(x);
      if (p.IsSome) {
        // yield the elements
        yield return transform(currentList.Reverse());
        currentList = ImmutableStack.Empty;
        // yield the separator
        yield return p.AsSome.ElseThrow("impossible");
      } else {
        currentList = currentList.Push(x);
      }
    }
    // yield the last (possibly empty) unclosed batch of elements
    yield return transform(currentList.Reverse());
  }*/

  public static A FoldLeft3<T,A>(this IEnumerable<T> ie, Func<T,T,T,A> init, Func<A,T,T,A> f) {
    var e = ie.GetEnumerator();
    e.MoveNext();
    T a = e.Current;
    e.MoveNext();
    T b = e.Current;
    e.MoveNext();
    T c = e.Current;
    A acc = init(a, b, c);
    while (e.MoveNext()) {
      T x = e.Current;
      e.MoveNext();
      T y = e.Current;
      acc = f(acc, x, y);
    }
    return acc;
  }

  public static A FoldRight3<T,A>(this IEnumerable<T> ie, Func<T,T,T,A> init, Func<T,T,A,A> f) {
    var e = ie.Reverse().GetEnumerator();
    e.MoveNext();
    T a = e.Current;
    e.MoveNext();
    T b = e.Current;
    e.MoveNext();
    T c = e.Current;
    A acc = init(c, b, a);
    while (e.MoveNext()) {
      T x = e.Current;
      e.MoveNext();
      T y = e.Current;
      acc = f(y, x, acc);
    }
    return acc;
  }

  public static AstNode PostProcess(this SamePrecedenceOrTerminal samePrecedenceOrTerminal)
    => samePrecedenceOrTerminal.Match(
      SamePrecedence: x => PostProcess(x),
      // TODO: just writing Terminal: AstNode.Terminal causes a null exception
      Terminal: x => AstNode.Terminal(x));

  public static IEnumerable<AstNode> PostProcess(this OperatorOrHole operatorOrHole)
    => operatorOrHole.Match(
      Operator: o => o.Item2.Select(PostProcess),
      Hole: h => h.PostProcess().Singleton());
      

  public static AstNode PostProcess(this ParserResult2 parserResult) {
    // Let's start with right associativity
    // TODO: handle other associativities

    // We flatten by converting to a sequence of SamePrecedenceOrTerminal

    // turn this:  h   h   o   h   o      o      o   h   h   o      o   h
    // into this: (h   h) (o) (h) (o) () (o) () (o) (h   h) (o) () (o) (h)
    // and  this:          o   h   o      o      o   h   h   o      o
    // into this:      () (o) (h) (o) () (o) () (o) (h   h) (o) () (o) ()
    // i.e. always have a (possibly empty) list on both ends.

    var split = parserResult.Get().Item2.Split(x => x.IsOperator);

    return parserResult.Get().Item1.Match(
      NonAssociative: () => {
        if (split.Count() != 3) {
          throw new ParserErrorException($"Internal error: NonAssociative operator within group of {split.Count()} elements, expected exactly 3");
        } else {
          var @operator =
            split.ElementAt(1)
              .Single().ElseThrow(new Exception("impossible"))
              .AsOperator.ElseThrow(new Exception("impossible"));
          return AstNode.Operator(
            (@operator.Item1,
                     split.ElementAt(0).SelectMany(PostProcess)
             .Concat(split.ElementAt(1).SelectMany(PostProcess))
             .Concat(split.ElementAt(2).SelectMany(PostProcess))));
        }
      },
      RightAssociative: () =>
        split.FoldRight3(
          // Last group of three
          (hsl, o, hsr) => {
            var @operator = o
              .Single().ElseThrow(new Exception("impossible"))
              .AsOperator.ElseThrow(new Exception("impossible"));
            return AstNode.Operator(
              (@operator.Item1,
                      hsl.SelectMany(PostProcess)
              .Concat(o.SelectMany(PostProcess))
              .Concat(hsr.SelectMany(PostProcess))));
          },
          // Subsequent groups of two starting with accumulator
          (hsl, o, a) => {
            var @operator = o
              .Single().ElseThrow(new Exception("impossible"))
              .AsOperator.ElseThrow(new Exception("impossible"));
            return AstNode.Operator(
              (@operator.Item1,
                      hsl.SelectMany(PostProcess)
              .Concat(o.SelectMany(PostProcess))
              .Concat(a)));
          }),
      LeftAssociative: () =>
        split.FoldLeft3(
          // Fist group of three
          (hsl, o, hsr) => {
            var @operator = o
              .Single().ElseThrow(new Exception("impossible"))
              .AsOperator.ElseThrow(new Exception("impossible"));
            return AstNode.Operator(
              (@operator.Item1,
                      hsl.SelectMany(PostProcess)
              .Concat(o.SelectMany(PostProcess))
              .Concat(hsr.SelectMany(PostProcess))));
          },
          // Subsequent groups of two starting with accumulator
          (a, o, hsr) => {
            var @operator = o
              .Single().ElseThrow(new Exception("impossible"))
              .AsOperator.ElseThrow(new Exception("impossible"));
            return AstNode.Operator(
              (@operator.Item1,
                      a.Singleton()
              .Concat(o.SelectMany(PostProcess))
              .Concat(hsr.SelectMany(PostProcess))));
          }));
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

  public static ValueTuple<IImmutableEnumerator<Lexeme>, AstNode> Parse2(string source) {
    Grammar2 grammar =
      DefaultGrammar.DefaultPrecedenceDAG.ToGrammar2();
    //Log(grammar.Str());

    var P = Func.YMemoize<
              IImmutableEnumerator<Lexeme>,
              Grammar2,
              Option<ValueTuple<IImmutableEnumerator<Lexeme>, ParserResult>>>(
      Parse3
    );

    var lexSrc = Lexer.Lex(source);

    /*lexSrc
      .ToIEnumerable()
      .Select(c => c.state.ToString())
      .JoinWith(" ")
      .Pipe(x => Log(x));*/

    //Log("");

    var parsed = P(lexSrc, grammar)
      .IfSome((rest, result) => (rest, result.Gather().PostProcess()))
      .ElseThrow(() => new ParserErrorException("Parse error."));
    
    parsed.Item1.FirstAndRest().IfSome(
      (first, rest) => {
        lexSrc
          .ToIEnumerable()
          .TakeUntil(c => c.Equals(parsed.Item1))
          .Select(c => c.lexeme.ToString())
          .JoinWith(" ")
          .Pipe(x => throw new ParserErrorException(
        $"Trailing rubbish: {x}."));
        return Unit.unit;
      });

    return parsed;
  }

  public static Ast.AstNode Parse(string source)
    => Parse2(source).Item2;
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