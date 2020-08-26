using S = Lexer.S;
using PrecedenceDAG = ImmutableDefaultDictionary<string, MixFix.DAGNode>;
using static Global;
using static MixFix;
using static MixFix.Associativity;

public static class DefaultGrammar {
  public static PrecedenceDAG DefaultPrecedenceDAG
    = EmptyPrecedenceDAG
      .WithOperator("bool",     NonAssociative, "equality|terminal", S.And, "equality|terminal")
      .WithOperator("equality", NonAssociative, "int|terminal|additive|multiplicative", S.Eq, "int|terminal|additive|multiplicative")
      .WithOperator("int",      NonAssociative, S.Int)
      .WithOperator("additive", LeftAssociative, "int|terminal|multiplicative", S.Plus, "int|terminal|multiplicative")
//      .WithOperator("multiplicative", LeftAssociative, "int|terminal", S.Times, "int|terminal")
      .WithOperator("terminal", NonAssociative, S.Ident)
      // This is the root set of operators
      .WithOperator("program",  NonAssociative,
      // "bool" // TODO: this needs aliases
      "equality|terminal", S.And, "equality|terminal");
}