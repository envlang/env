// Code quality of this file: medium.

using System;
using System.Collections.Generic;
using System.Linq;
using Record = System.Collections.Immutable.ImmutableDictionary<string, string>;

public static class RecordGenerator {
  private static void Fields(this Action<string> w, string qualifier, string name, Record record) {
    foreach (var @field in record) {
      var F = @field.Key;
      var Ty = @field.Value;
      w($"    public readonly {Ty} {F};");
    }
  }

  private static void Constructor(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public {name}(");
    w(String.Join(",\n", record.Select(@field =>
                $"        {@field.Value} {@field.Key}")));
    w($"      ) {{");
    foreach (var @field in record) {
      var F = @field.Key;
      var Ty = @field.Value;
      w($"    this.{F} = {F};");
    }
    w($"    }}");
  }

  private static void Equality(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public static bool operator ==({name} a, {name} b)");
    w($"      => Equality.Operator(a, b);");
    w($"    public static bool operator !=({name} a, {name} b)");
    w($"      => !(a == b);");
    w($"    public override bool Equals(object other)");
    w($"      => Equality.Untyped<{name}>(this, other, x => x as {name},");
    w(String.Join(",\n", record.Select(@field =>
                $"          x => x.{@field.Key}")));
    w($"        );");
    w($"    public bool Equals({name} other)");
    w($"      => Equality.Equatable<{name}>(this, other);");
    w($"    public override int GetHashCode()");
    w($"      => Equality.HashCode(\"{name}\",");
    w(String.Join(",\n", record.Select(@field =>
                $"          this.{@field.Key}")));
    w($"        );");
  }

  private static void With(this Action<string> w, string qualifier, string name, Record record) {
    foreach (var @field in record) {
      var F = @field.Key;
      var noAtF = F.StartsWith("@") ? F.Substring(1) : F;
      var caseF = Char.ToUpper(noAtF[0]) + noAtF.Substring(1);
      var Ty = @field.Value;
      w($"    public {name} With{caseF}({Ty} {F}) => new {name}("
        + String.Join(", ", record.Select(@f => $"{f.Key}: {f.Key}"))
        + ");");
    }
  }

  private static void Lens(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public Lens<{name}> lens {{ get => ChainLens(x => x); }}");
  }

  private static void ChainLens(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public Lens<Whole> ChainLens<Whole>(System.Func<{name}, Whole> wrap) => new Lens<Whole>(wrap: wrap, oldHole: this);");
  }

  private static void Lenses(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public sealed class Lens<Whole> : ILens<{name}, Whole> {{");
    w($"      public readonly System.Func<{name}, Whole> wrap;");
    w($"      public readonly {name} oldHole;");
    w($"");
    w($"      public Lens(System.Func<{name}, Whole> wrap, {name} oldHole) {{");
    w($"        this.wrap = wrap;");
    w($"        this.oldHole = oldHole;");
    w($"      }}");
    foreach (var @field in record) {
      var F = @field.Key;
      var noAtF = F.StartsWith("@") ? F.Substring(1) : F;
      var caseF = Char.ToUpper(noAtF[0]) + noAtF.Substring(1);
      var Ty = @field.Value;
      w($"      public ILens<{Ty},Whole> {F}");
      w($"        => oldHole.{F}.ChainLens(");
      w($"          value => wrap(oldHole.With{caseF}(value)));");
    }
    w($"      public Whole Update(Func<{name}, {name}> update) => wrap(update(oldHole));");
    w($"    }}");
  }

  private static void RecordClass(this Action<string> w, string qualifier, string name, Record record) {
    w($"  public sealed class {name} : IEquatable<{name}> {{");
    w.Fields(qualifier, name, record);
    w($"");
    w.Constructor(qualifier, name, record);
    w($"");
    w.Equality(qualifier, name, record);
    w($"");
    w.With(qualifier, name, record);
    w($"");
    w.Lens(qualifier, name, record);
    w($"");
    w.ChainLens(qualifier, name, record);
    w($"");
    w.Lenses(qualifier, name, record);
    w($"  }}");
  }

  public static void Record(this Action<string> w, string header, string footer, string qualifier, string name, Record record) {
    w($"{header}");
    w("");
    w.RecordClass(qualifier, name, record);
    w($"{footer}");
  }
}
