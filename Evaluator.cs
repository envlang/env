using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Immutable;
using static Global;

public class Evaluator {
  public static string EvaluateWrap(Ast.AstNode source)
    => Evaluate(source, ImmutableDictionary<string, Ast.Val>.Empty).Match(
         Int: i => i.ToString(),
         String: s => s);

  public static Ast.Val Evaluate(Ast.AstNode source, ImmutableDictionary<string, Ast.Val> env)
    // => Log(source.Str(), ()
    => source.Match(
      Operator: o => o.Item1.semantics.Match(
        // The wrapper around the whole program:
        Program: () => {
          if (o.Item2.Count() != 3) {
            throw new RuntimeErrorException("The Program wrapper should contain two parts: StartOfInput, prog and EndOfInput");
          }
          // TODO: check that the last token is indeed Program
          return Evaluate(o.Item2.ElementAt(1), env);
        },
        And: () => {
          if (o.Item2.Count() != 3) {
            throw new RuntimeErrorException("The And operator should contain three parts");
          }
          // TODO: check that the last token is indeed Program
          var a = Evaluate(o.Item2.ElementAt(0), env);
          var b = Evaluate(o.Item2.ElementAt(2), env);
          return Ast.Val.Int(999);
        },
        LiteralInt: () =>
          o.Item2
            .Single()
            .ElseThrow(
              new RuntimeErrorException("LiteralInt should contain a single lexeme"))
            .AsTerminal
            .ElseThrow(
              new RuntimeErrorException("LiteralInt's contents should be a lexeme"))
            .lexeme
            .Pipe(x => Ast.Val.Int(Int32.Parse(x))),
        LiteralString: () => {
          if (o.Item2.Count() != 3) {
            throw new RuntimeErrorException("LiteralString should contain three lexemes: OpenString, String and CloseString");
          }
          // TODO: check that the open & close are indeed that
          return o.Item2.ElementAt(1)
            .AsTerminal.ElseThrow(
              new RuntimeErrorException("LiteralInt's contents should be a lexeme"))
            .lexeme
            .Pipe(x => Ast.Val.String(x));
        },
        Unsupported: () => throw new RuntimeErrorException($"Unsupported opeartor {o}, sorry.")),
      Terminal: t => Ast.Val.String(t.lexeme)/*TODO*/);
}

// Note: for typeclass resolution, ask that functions have their parameters and return types annotated. This annotation is added to the values at run-time, which allows to dispatch based on the annotation rather than on the actual value.