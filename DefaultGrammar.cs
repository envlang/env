using S = Lexer.S;
using PrecedenceDAG = ImmutableDefaultDictionary<string, MixFix.DAGNode>;
using static Global;
using static MixFix;
using static MixFix.Associativity;

public static class DefaultGrammar {
  public static PrecedenceDAG DefaultPrecedenceDAG
    = EmptyPrecedenceDAG
      .WithOperator("bool",     NonAssociative, "equality|terminal", S.And, "equality|terminal")
      .WithOperator("equality", NonAssociative, "int", S.Eq, "int")
      .WithOperator("int",      NonAssociative, S.Int)
      .WithOperator("terminal", NonAssociative, S.Ident);
}