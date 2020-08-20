// Code quality of this file: medium.

using System;
using System.Collections.Generic;
using System.Linq;
using Variant = System.Collections.Immutable.ImmutableDictionary<string, string>;

public static class VariantGenerator {
  private static void MatchExampleComment(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"  /* To match against an instance of {name}, write:");
    w($"     x.Match(");
    w(String.Join(",\n", variant.Select(@case => 
                $"       {@case.Key}: {@case.Value == null ? "()" : "value"} => throw new NotImplementedException()")));
    w($"     )");
    w($"  */");
  }

  private static void PrivateConstructor(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    private {name}() {{}}");
  }

  private static void Visitor(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public class Visitor<T> {{");
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;

      w($"      public Func<{Ty == null ? "" : $"{Ty}, "}T> {C} {{ get; set; }} ");
    }
    w($"    }}");
  }

  private static void Match_(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public abstract T Match_<T>(Visitor<T> c);");
  }

  private static void CaseShorthands(this Action<string> w, string qualifier, string name, Variant variant) {
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      w($"    public static {name} {C}{Ty == null
                ? $" = new Cases.{C}()"
                : $"({Ty} value) => new Cases.{C}(value)"};");
    }
  }

  private static void As(this Action<string> w, string qualifier, string name, Variant variant) {
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      w($"    public virtual Immutable.Option<{Ty == null ? "Immutable.Unit" : Ty}> As{C}() => Immutable.Option.None<{Ty == null ? "Immutable.Unit" : Ty}>();");
    }
  }

  private static void Lens(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public LeafLens<{name}> Lens {{ get => ChainLens(x => x); }}");
  }

  private static void ChainLens(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public LeafLens<{name}, Whole> ChainLens<Whole>(System.Func<{name}, Whole> wrap) => new LeafLens<{name}, Whole>(wrap: wrap, oldHole: this);");
  }

  private static void GetTag(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    private string GetTag() {{");
    w($"      return this.Match(");
    w(String.Join(",\n", variant.Select(@case =>
                $"        {@case.Key}: {@case.Value == null ? "()" : "value"} => \"{@case.Key}\"")));
    w($"      );");
    w($"    }}");
  }

  private static void Equality(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public static bool operator ==({name} a, {name} b)");
    w($"      => Equality.Operator(a, b);");
    w($"    public static bool operator !=({name} a, {name} b)");
    w($"      => !(a == b);");
    w($"    public override abstract bool Equals(Object other);");
    w($"    public abstract bool Equals({name} other);");
    w($"");
    w($"    public override abstract int GetHashCode();");
  }

  private static void CaseValue(this Action<string> w, string qualifier, string name, string C, string Ty) {
    if (Ty != null) {
      w($"         public readonly {Ty} value;");
      w($"");
    }
  }

  private static void CaseConstructor(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public {C}({Ty == null ? "" : $"{Ty} value"}) {{ {Ty == null ? "" : $"this.value = value; "}}}");
  }

  private static void CaseMatch_(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public override T Match_<T>(Visitor<T> c) => c.{C}({Ty == null ? "" : "value"});");
  }

  private static void CaseAs(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public override Immutable.Option<{Ty == null ? "Immutable.Unit" : Ty}> As{C}() => Immutable.Option.Some<{Ty == null ? "Immutable.Unit" : Ty}>({Ty == null ? "Immutable.Unit.unit" : "this.value"});");
  }

  private static void CaseEquality(this Action<string> w, string qualifier, string name, string C, string Ty) {
      w($"         public static bool operator ==({C} a, {C} b)");
      w($"           => Equality.Operator(a, b);");
      w($"         public static bool operator !=({C} a, {C} b)");
      w($"           => !(a == b);");
      w($"         public override bool Equals(object other)");
    if (Ty == null) {
      w($"           => Equality.Untyped<{C}>(this, other, x => x as {C});");
    } else {
      w($"           => Equality.Untyped<{C}>(this, other, x => x as {C}, x => x.value);");
    }
    w($"         public override bool Equals({name} other)");
    w($"           => Equality.Equatable<{name}>(this, other);");
    w($"         public override int GetHashCode()");
    if (Ty == null) {
      w($"           => HashCode.Combine(\"{C}\");");
    } else {
      w($"           => HashCode.Combine(\"{C}\", this.value);");
    }
  }

  private static void CaseToString(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"        public override string ToString() => \"{C}\";");
  }

  private static void Cases(this Action<string> w, string qualifier, string name, Variant variant) {
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;

      w($"      public sealed class {C} : {name} {{");
      w.CaseValue(qualifier, name, C, Ty);
      w.CaseConstructor(qualifier, name, C, Ty);
      w($"");
      w.CaseMatch_(qualifier, name, C, Ty);
      w($"");
      w.CaseAs(qualifier, name, C, Ty);
      w($"");
      w.CaseEquality(qualifier, name, C, Ty);
      w($"");
      w.CaseToString(qualifier, name, C, Ty);
      w($"      }}");
    }
  }

  private static void VariantClass(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"  public abstract class {name} : IEquatable<{name}> {{");
    w.PrivateConstructor(qualifier, name, variant);
    w($"");
    w.Visitor(qualifier, name, variant);
    w($"");
    w.Match_(qualifier, name, variant);
    w($"");
    w.CaseShorthands(qualifier, name, variant);
    w($"");
    w.As(qualifier, name, variant);
    w($"");
    w.ChainLens(qualifier, name, variant);
    w($"");
    w.Equality(qualifier, name, variant);
    w($"    public static class Cases {{");
    w.Cases(qualifier, name, variant);
    w($"    }}");
    w($"  }}");
    w("");
    w($"}}");
  }

  private static void ExtensionMethods(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"public static class {name}ExtensionMethods {{");
    w($"  public static T Match<T>(");
    w($"      this {qualifier}{name} e,");
    w(String.Join(",\n", variant.Select(c =>
                $"      Func<{c.Value == null ? "" : $"{c.Value}, "}T> {c.Key}")));
    w($"    ) {{");
    w($"    return e.Match_(new {qualifier}{name}.Visitor<T> {{");
    w(String.Join(",\n", variant.Select(c =>
                $"      {c.Key} = {c.Key}")));
    w($"    }});");
    w($"  }}");
  }

  public static void Variant(this Action<string> w, string header, string footer, string qualifier, string name, Variant variant) {
    w($"{header}");
    w($"");
    w.MatchExampleComment(qualifier, name, variant);
    w($"");
    w.VariantClass(qualifier, name, variant);
    w($"");
    w.ExtensionMethods(qualifier, name, variant);
    w($"{footer}");
  }

  private static void QualifierAliases(this Action<string> w, string qualifier, string name, Variant variant) {
    if (qualifier != "") {
      w($"using {name} = {qualifier}{name};");
    }
  }

  public static void VariantUsing(this Action<string> w, string header, string footer, string qualifier, string name, Variant variant) {
    w.QualifierAliases(qualifier, name, variant);
  }
}