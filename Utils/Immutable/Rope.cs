using System.Text;
using System.Collections.Immutable;

namespace Immutable {
  public partial class Node {
    private string CustomToString() {
      var sb = new StringBuilder();
      var stack = ImmutableStack<Rope>.Empty;
      while (!stack.IsEmpty) {
        var e = stack.Peek();
        stack = stack.Pop();
        e.Match(
          Leaf: s => {
            sb.Append(s);
            return Unit.unit;
          },
          Node: x => {
            stack = stack.Push(x.a).Push(x.b);
            return Unit.unit;
          });
      }
      return sb.ToString();
    }

    // TODO: this is not used by the generated code
    private int CustomHashCode(Node a, Node b)
      => a.GetHashCode() ^ b.GetHashCode();

    private bool CustomEquals(Node a, Node b)
      // TODO: a faster implementation of equality
      => a.ToString().Equals(b.ToString());
  }
}