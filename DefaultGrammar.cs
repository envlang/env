using S = Lexer.S;
using PrecedenceDAG = ImmutableDefaultDictionary<string, MixFix.DAGNode>;
using static Global;
using static MixFix;
using static MixFix.Associativity;
using static MixFix.Semantics;

public static class DefaultGrammar {
  public static PrecedenceDAG DefaultPrecedenceDAG
    = EmptyPrecedenceDAG
      .WithOperator("bool",     Unsupported, NonAssociative, "equality|terminal", S.And, "equality|terminal")
      .WithOperator("equality", Unsupported, NonAssociative, "int|terminal", S.Eq, "int|terminal") // |additive|multiplicative
      .WithOperator("int",      LiteralInt,     NonAssociative, S.Int)
      .WithOperator("additive", Unsupported, LeftAssociative, "int|terminal|multiplicative", S.Plus, "int|terminal|multiplicative")
      .WithOperator("multiplicative", Unsupported, LeftAssociative, "int|terminal", S.Times, "int|terminal")
      .WithOperator("terminal", Unsupported, NonAssociative, S.Ident)
      // This is the root set of operators
      // TODO: this needs aliases
      .WithOperator("prog",  Unsupported, LeftAssociative, "equality|terminal", S.And, "equality|terminal")
      .WithOperator("prog",  LiteralInt, NonAssociative, S.Int)
      .WithOperator("prog",  LiteralString, NonAssociative, S.StringOpen, S.String, S.StringClose)
      .WithOperator("program",  Program, NonAssociative, S.StartOfInput, "prog", S.EndOfInput)
      ;
}