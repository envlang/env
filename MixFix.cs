using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Immutable;
using S = Lexer.S;
using Lexeme = Lexer.Lexeme;
using PrecedenceDAG = ImmutableDefaultDictionary<string, MixFix.DAGNode>;
using PrecedenceGroupName = System.String;
using Hole = System.Collections.Immutable.ImmutableHashSet<
               string /* PrecedenceGroupName */>;
using static Global;
using static MixFix.Fixity;

public class Foo {
  public string v;
  public static implicit operator Foo(string s) => new Foo{v=s};
  public static Foo operator |(Foo a, string b) => a.v + b;
}

public static partial class MixFix {
  public partial class Operator {
    private string CustomToString()
    => $"Operator(\"{precedenceGroup}\", {fixity}, {parts.Select(x => x.Match(Hole: h => h.Select(g => g.ToString()).JoinWith("|"), Name: n => $"\"{n}\"")).JoinWith(", ")})";
  }

  public abstract partial class Part {
    public static implicit operator Part(S s)
      => Part.Name(s);
    public static implicit operator Part(string s)
      => Part.Hole(s.Split("|").ToImmutableHashSet());
  }

  public partial class Fixity {
    // Used in the fixity table below.
    public static implicit operator Func<Fixity>(Fixity fixity) => () => fixity;
  }

  public partial class Operator {
    // TODO: cache this for improved complexity
    public Option<ImmutableHashSet<string>> leftmostHole {
      get => parts.First().Bind(firstPart => firstPart.AsHole);
    }

    // TODO: cache this for improved complexity
    public Option<ImmutableHashSet<string>> rightmostHole {
      get => parts.Last().Bind(lastPart => lastPart.AsHole);
    }

    private static Func<Fixity> error =
      () => throw new Exception("Internal error: unexpected fixity.");

    private static Func<Fixity> notYet =
      () => throw new NotImplementedException(
        "NonAssociative prefix and postfix operators are not supported for now.");

    private ImmutableList<ImmutableList<Func<Fixity>>> fixityTable = new [] {
      //     neither start    end     both              ←Hole//↓Associativity
      new[]{ Closed, notYet,  notYet, InfixNonAssociative   },//NonAssociative
      new[]{ error,  Postfix, error,  InfixLeftAssociative  },//LeftAssociative
      new[]{ error,  error,   Prefix, InfixRightAssociative },//RightAssociative
    }.Select(ImmutableList.ToImmutableList).ToImmutableList();
    
    public Fixity fixity {
      get {
        var startsWithHole = parts.First()
                                  .Bind(lastPart => lastPart.AsHole)
                                  .IsSome;

        var endsWithHole = parts.First()
                                .Bind(lastPart => lastPart.AsHole)
                                .IsSome;

        var row = associativity.Match(
          NonAssociative: () => 0,
          LeftAssociative: () => 1,
          RightAssociative: () => 2);

        var column = ((!startsWithHole) && (!endsWithHole)) ? 0
                   : (( startsWithHole) && (!endsWithHole)) ? 1
                   : ((!startsWithHole) && ( endsWithHole)) ? 2
                   : (( startsWithHole) && ( endsWithHole)) ? 3
                   : throw new Exception("impossible");

        return fixityTable[row][column]();
      }
    }
  }

  public partial class DAGNode {
    // TODO: cache this for improved complexity
    public IEnumerable<Operator> allOperators {
      get => ImmutableList(
          closed,
          prefix,
          postfix,
          infixNonAssociative,
          infixRightAssociative,
          infixLeftAssociative
        ).SelectMany(x => x);
    }

    // TODO: cache this for improved complexity
    public Option<ImmutableHashSet<string>> leftmostHole {
      get => allOperators.First(@operator => @operator.leftmostHole);
    }

    // TODO: cache this for improved complexity
    public Option<ImmutableHashSet<string>> rightmostHole {
      get => allOperators.First(@operator => @operator.rightmostHole);
    }
  }

  public static DAGNode EmptyDAGNode = new DAGNode(
       closed:                ImmutableList<Operator>.Empty,
       prefix:                ImmutableList<Operator>.Empty,
       postfix:               ImmutableList<Operator>.Empty,
       infixNonAssociative:   ImmutableList<Operator>.Empty,
       infixRightAssociative: ImmutableList<Operator>.Empty,
       infixLeftAssociative:  ImmutableList<Operator>.Empty
     );

  public static PrecedenceDAG EmptyPrecedenceDAG
    = new PrecedenceDAG(EmptyDAGNode);

  public static Whole Add<Whole>(this ILens<DAGNode, Whole> node, Operator @operator) {
    return @operator.fixity.Match(
      Closed:
        () => node.Closed(),
      Prefix:
        () => node.Prefix(),
      Postfix:
        () => node.Postfix(),
      InfixNonAssociative:
        () => node.InfixNonAssociative(),
      InfixRightAssociative:
        () => node.InfixRightAssociative(),
      InfixLeftAssociative:
        () => node.InfixLeftAssociative()
    ).Cons(@operator);
  }

  public static void CheckHole(PrecedenceDAG precedenceDAG, Operator @operator, string name, Option<Hole> existing, Option<Hole> @new) {
    existing.IfSome(existingHole =>
      @new.IfSome(newHole => {
        if (! newHole.SetEquals(existingHole)) {
          throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, its {name} hole ({newHole.ToString()}) must either be empty or else use the same precedence groups as the existing operators in {@operator.precedenceGroup}, i.e. {existingHole}.");
        }
        return unit;
      })
    );
  }

  public static void CheckLeftmostHole(PrecedenceDAG precedenceDAG, Operator @operator)
    => CheckHole(
      precedenceDAG,
      @operator,
      "leftmost",
      precedenceDAG[@operator.precedenceGroup].leftmostHole,
      @operator.leftmostHole);

  public static void CheckRightmostHole(PrecedenceDAG precedenceDAG, Operator @operator)
    => CheckHole(
      precedenceDAG,
      @operator,
      "rightmost",
      precedenceDAG[@operator.precedenceGroup].rightmostHole,
      @operator.rightmostHole);

  public static void CheckNonEmpty(PrecedenceDAG precedenceDAG, Operator @operator) {
    if (@operator.parts.Count == 0) {
      throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, it has no parts.");
    }
  }

  public static void CheckConsecutiveHoles(PrecedenceDAG precedenceDAG, Operator @operator) {
    @operator.parts.Aggregate((previous, current) => {
      if (previous.IsHole && current.IsHole) {
        throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, it has two consecutive holes. This is not supported for now.");
      }
      return current;
    });
  }

  public static void CheckSingleUseOfNames(PrecedenceDAG precedenceDAG, Operator @operator) {
    // TODO: check that each name part isn't used elsewhere in a way that could cause ambiguity (use in a different namespace which is bracketed is okay, etc.). Probably something to do with paths reachable from the root.
  }

  public static void CheckNotAlias(PrecedenceDAG precedenceDAG, Operator @operator) {
    if (@operator.parts.Single().Bind(part => part.AsHole).IsSome) {
      throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, it only contains a single hole. Aliases built like this are not supported for now.");
    }
  }

  public static PrecedenceDAG With(this PrecedenceDAG precedenceDAG, Operator @operator) {
    // This is where all the checks are done to ensure that the
    //   resulting grammar is well-formed and assuredly unambiguous.
    // Future extension idea: add an "ambiguous" keyword to
    //   alleviate some restrictions.
    CheckLeftmostHole(precedenceDAG, @operator);
    CheckRightmostHole(precedenceDAG, @operator);
    CheckNonEmpty(precedenceDAG, @operator);
    CheckConsecutiveHoles(precedenceDAG, @operator);
    CheckSingleUseOfNames(precedenceDAG, @operator);
    CheckNotAlias(precedenceDAG, @operator);
    return precedenceDAG.lens[@operator.precedenceGroup].Add(@operator);
  }

  public static PrecedenceDAG WithOperator(this PrecedenceDAG precedenceDAG, string precedenceGroup, Associativity associativity, params Part[] parts)
    => precedenceDAG.With(
      new Operator(
        precedenceGroup: precedenceGroup,
        associativity: associativity,
        parts: parts.ToImmutableList()));

  public static Grammar OperatorToGrammar(Operator @operator) {
    //@operator.
    throw new NotImplementedException();
  }

  public static Grammar DAGNodeToGrammar(DAGNode node) {
//    return Grammar.Or(ImmutableList<Grammar>(
//      node.closed
//      closed,
//      succl nonassoc succr,
//      (prefix | succl rightassoc)+ succr,
//      succl (suffix | succr rightassoc)+
//    ));
    throw new NotImplementedException();
  }

  public static void DAGToGrammar(DAGNode precedenceDAG) {
    
  }

  public static void RecursiveDescent(IEnumerable<Lexeme> e) {

  }
}