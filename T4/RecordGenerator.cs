// Code quality of this file: medium.

using System;
using System.Collections.Generic;
using System.Linq;
using Record = System.Collections.Immutable.ImmutableDictionary<string, string>;

public static class RecordGenerator {
  private static void NewExampleComment(this Action<string> w, string qualifier, string name, Record record) {
    w($"  /* To create an instance of {name}, write:");
    w($"     new {name}(");
    w(String.Join(",\n", record.Select(@field => 
                $"       {@field.Key}: new {@field.Value}(…)")));
    w($"     )");
    w($"  */");
  }

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
    w($"      this.hashCode = Equality.HashCode(\"{name}\",");
    w(String.Join(",\n", record.Select(@field =>
                $"          this.{@field.Key}")));
    w($"      );");
    w($"    }}");
  }

  private static void Equality(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public static bool operator ==({name} a, {name} b)");
    w($"      => Equality.Operator(a, b);");
    w($"    public static bool operator !=({name} a, {name} b)");
    w($"      => !(a == b);");
    w($"    public override bool Equals(object other)");
    w($"      => Equality.Untyped<{name}>(this, other, x => x as {name}, x => x.hashCode,");
    w(String.Join(",\n", record.Select(@field =>
                $"          x => x.{@field.Key}")));
    w($"        );");
    w($"    public bool Equals({name} other)");
    w($"      => Equality.Equatable<{name}>(this, other);");
    w($"    private readonly int hashCode;");
    w($"      public override int GetHashCode() => hashCode;");
  }

  private static void StringConversion(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public override string ToString()");
    w($"      => this.CustomToString();");
    w($"    private string CustomToString(params Immutable.Uninstantiatable[] _)");
    w($"      => $\"{name}(\\n{String.Join(",\\n", record.Select(@field => $"  {@field.Key}: {{{@field.Key}.Str<{@field.Value}>()}}"))})\";");
    w($"    public string Str() => ToString();");
  }

  private static void With(this Action<string> w, string qualifier, string name, Record record) {
    foreach (var @field in record) {
      var F = @field.Key;
      var noAtF = F.StartsWith("@") ? F.Substring(1) : F;
      var caseF = Char.ToUpper(noAtF[0]) + noAtF.Substring(1);
      var Ty = @field.Value;
      w($"    public {name} With{caseF}({Ty} {F})");
      w($"      => new {name}("
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
    w($"      private readonly {name} oldHole;");
    w($"");
    w($"      public {name} value {{ get => oldHole; }}");
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
    w($"      public Whole Update(Func<{name}, {name}> update)");
    w($"        => wrap(update(oldHole));");
    w($"    }}");
  }

  private static void RecordClass(this Action<string> w, string qualifier, string name, Record record) {
    w($"  public sealed partial class {name} : IEquatable<{name}>, IString {{");
    w.Fields(qualifier, name, record);
    w($"");
    w.Constructor(qualifier, name, record);
    w($"");
    w.Equality(qualifier, name, record);
    w($"");
    w.StringConversion(qualifier, name, record);
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

  private static void LensExtensionMethods(this Action<string> w, string qualifier, string name, Record record) {
    w($"    public static class {name}LensExtensionMethods {{");
    foreach (var @field in record) {
      var F = @field.Key;
      var noAtF = F.StartsWith("@") ? F.Substring(1) : F;
      var caseF = Char.ToUpper(noAtF[0]) + noAtF.Substring(1);
      var Ty = @field.Value;
      // same as {name}.Lens but as extension mehtods (should
      // be extension properties once C# supports those) to
      // be applied to instances of ILens<{name}, Whole>
      w($"      public static ILens<{Ty}, Whole>");
      w($"        {caseF}<Whole>(");
      w($"          this ILens<{qualifier}{name}, Whole> self)");
      w($"        => self.value.{F}.ChainLens(");
      w($"          value => self.Update(oldHole => oldHole.With{caseF}(value)));");
    }
    w($"    }}");
  }

  public static void Record(this Action<string> w, string header, string footer, string qualifier, string name, Record record) {
    w($"{header}");
    w("");
    w.NewExampleComment(qualifier, name, record);
    w("");
    w.RecordClass(qualifier, name, record);
    w($"{footer}");
    w.LensExtensionMethods(qualifier, name, record);
  }

  private static void QualifierAliases(this Action<string> w, string qualifier, string name, Record record) {
    if (qualifier != "") {
      w($"using {name} = {qualifier}{name};");
    }
  }

  public static void RecordUsing(this Action<string> w, string header, string footer, string qualifier, string name, Record record) {
    w.QualifierAliases(qualifier, name, record);
  }
}
