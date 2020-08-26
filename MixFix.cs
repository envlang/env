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

public static partial class MixFix {
  public partial class Grammar {
    public static Grammar Sequence(params Grammar[] xs)
      => Sequence(xs.ToImmutableList());

    public static Grammar Or(params Grammar[] xs)
      => Or(xs.ToImmutableList());

    public static Grammar Sequence(IEnumerable<Grammar> xs) {
      var filteredXs = xs.Where(x => !x.IsEmpty);
      if (filteredXs.Count() == 1) {
        return filteredXs.Single().ElseThrow(() => new Exception("TODO: use an either to prove that this is safe."));
      } else {
        return new Grammar.Cases.Sequence(filteredXs);
      }
    }

    public static Grammar Or(IEnumerable<Grammar> xs) {
      var filteredXs = xs.Where(x => !x.IsEmpty);
      if (filteredXs.Count() == 1) {
        return filteredXs.Single().ElseThrow(() => new Exception("TODO: use an either to prove that this is safe."));
      } else {
        return new Grammar.Cases.Or(filteredXs);
      }
    }

    public static Grammar RepeatOnePlus(Grammar g)
      => g.IsEmpty
        ? Grammar.Empty
        : new Grammar.Cases.RepeatOnePlus(g);

    public static Grammar Empty = new Grammar.Cases.Or();

    public bool IsEmpty {
      get => this.Match(
        Or: l => l.Count() == 0,
        Sequence: l => l.Count() == 0,
        RepeatOnePlus: g => g.IsEmpty,
        Terminal: t => false,
        Rule: r => false
      );
    }

    public static Grammar operator |(Grammar a, Grammar b)
      => Or(a, b);

    public static implicit operator Grammar((Grammar a, Grammar b) gs)
      => Sequence(gs.a, gs.b);

    public static implicit operator Grammar((Grammar a, Grammar b, Grammar c) gs)
      => Sequence(gs.a, gs.b, gs.c);

    public static bool operator true(Grammar g) => !g.IsEmpty;
    public static bool operator false(Grammar g) => g.IsEmpty;

    public Grammar this[string multiplicity] {
      get {
        if (multiplicity != "+") {
          throw new Exception("Unexpected multiplicity");
        } else {
          if (this.IsEmpty) {
            return Or();
          } else {
            return RepeatOnePlus(this);
          }
        }
      }
    }

    private string Paren(bool paren, string s)
      => paren ? $"({s})" : s;

    string CustomToString()
      => this.Match<string>(
        Or: l => l.Count() == 0
          ? "Or(Empty)"
          : Paren(l.Count() != 1, l.Select(x => x.Str()).JoinWith(" | ")),
        Sequence: l => l.Count() == 0
          ? "Sequence(Empty)"
          : Paren(l.Count() != 1, l.Select(x => x.Str()).JoinWith(",")),
        RepeatOnePlus: g => $"({g.Str()})+",
        Terminal: t => t.Str(),
        Rule: r => r
      );
  }

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

    // TODO: does this need any caching?
    public IEnumerable<Part> internalParts {
      get => parts.SkipWhile(part => part.IsHole)
                  .SkipLastWhile(part => part.IsHole);
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

    // TODO: cache this for improved complexity
    // TODO: find a better name
    public ImmutableHashSet<string> leftmostHole_ {
      get => leftmostHole.Else(ImmutableHashSet<string>.Empty);
    }

    // TODO: cache this for improved complexity
    // TODO: find a better name
    public ImmutableHashSet<string> rightmostHole_ {
      get => rightmostHole.Else(ImmutableHashSet<string>.Empty);
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
          throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, its {name} hole ({newHole.ToString()}) must either be absent or else use the same precedence groups as the existing operators in {@operator.precedenceGroup}, i.e. {existingHole}.");
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
    // TODO: check that each name part isn't used elsewhere in a way that adding this operator would could cause ambiguity (use in a different namespace which is bracketed is okay, etc.). Probably something to do with paths reachable from the root.
  }

  public static void CheckNotAlias(PrecedenceDAG precedenceDAG, Operator @operator) {
    if (@operator.parts.Single().Bind(part => part.AsHole).IsSome) {
      throw new ParserExtensionException($"Cannot extend parser with operator {@operator}, it only contains a single hole. Aliases built like this are not supported for now.");
    }
  }

  public static void CheckAcyclic(PrecedenceDAG precedenceDAG, Operator @operator) {
    // TODO: check that the DAG stays acyclic after adding this operator
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
    CheckAcyclic(precedenceDAG, @operator);
    return precedenceDAG.lens[@operator.precedenceGroup].Add(@operator);
  }

  public static PrecedenceDAG WithOperator(this PrecedenceDAG precedenceDAG, string precedenceGroup, Associativity associativity, params Part[] parts)
    => precedenceDAG.With(
      new Operator(
        precedenceGroup: precedenceGroup,
        associativity: associativity,
        parts: parts.ToImmutableList()));

  public static Grammar ToGrammar(this Hole precedenceGroups)
    => Grammar.Or(
         precedenceGroups.Select(precedenceGroup =>
           Grammar.Rule(precedenceGroup)));

  public static Grammar ToGrammar(this Part part)
    => part.Match(
      Name: name => Grammar.Terminal(name), 
      Hole: precedenceGroups => precedenceGroups.ToGrammar());

  public static Grammar ToGrammar(this Operator @operator)
    => Grammar.Sequence(
      @operator.internalParts.Select(part => part.ToGrammar()));

  public static Grammar ToGrammar(this IEnumerable<Operator> operators)
    => Grammar.Or(
      operators.Select(@operator => @operator.ToGrammar()));

  public static Grammar ToGrammar(this DAGNode node) {
    var lsucc = node.leftmostHole_.ToGrammar();
    var rsucc = node.rightmostHole_.ToGrammar();
    var closed = node.closed.ToGrammar();
    var nonAssoc = node.infixNonAssociative.ToGrammar();
    var prefix = node.prefix.ToGrammar();
    var postfix = node.postfix.ToGrammar();
    var infixl = node.infixLeftAssociative.ToGrammar();
    var infixr = node.infixRightAssociative.ToGrammar();

    // TODO: BUG: only include these parts if there are
    // any operators with that fixity.
    return
        closed
      | (nonAssoc ? (lsucc, nonAssoc, rsucc) : Grammar.Empty)
      // TODO: post-processsing of the leftassoc list.
      | ((prefix || infixr) ? ((prefix | (lsucc, infixr))["+"], rsucc) : Grammar.Empty)
      // TODO: post-processsing of the leftassoc list.
      | ((postfix || infixl) ? (lsucc, (postfix | (infixl, rsucc))["+"]) : Grammar.Empty);
  }

  public static ImmutableDictionary<string, Grammar> DAGToGrammar(PrecedenceDAG precedenceDAG)
    => precedenceDAG.ToImmutableDictionary(
      node => node.Key,
      node => node.Value.ToGrammar());
}