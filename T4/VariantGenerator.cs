// Code quality of this file: low.

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
                : $"({Ty} value, params Immutable.Uninstantiatable[] _) => new Cases.{C}(value)"};");
    }
  }

  private static void As(this Action<string> w, string qualifier, string name, Variant variant) {
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      // This method is overloaded by the corresponding case's class.
      w($"    public virtual Immutable.Option<{Ty == null ? "Immutable.Unit" : Ty}> As{C} {{");
      w($"      get => Immutable.Option.None<{Ty == null ? "Immutable.Unit" : Ty}>();");
      w($"    }}");
    }
  }

  private static void Is(this Action<string> w, string qualifier, string name, Variant variant) {
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      w($"    public virtual bool Is{C} {{ get => As{C}.IsSome; }}");
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

  private static void GetValue(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    private Immutable.Option<string> GetValue() {{");
    w($"      return this.Match(");
    w(String.Join(",\n", variant.Select(@case =>
                $"        {@case.Key}: {
                  @case.Value == null
                  ? $"() => Immutable.Option.None<string>()"
                  : $"value => value.Str<{@case.Value}>().Some()"
                }")));
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

  private static void StringConversion(this Action<string> w, string qualifier, string name, Variant variant) {
    w($"    public override string ToString()");
    w($"      => this.CustomToString();");
    w($"    private string CustomToString(params Immutable.Uninstantiatable[] _)");
    w($"      => GetTag() + GetValue().Match(Some: x => $\"({{x}})\", None: () => \"\");");
    w($"    public string Str() => this.ToString();");
  }

  private static void CaseValue(this Action<string> w, string qualifier, string name, string C, string Ty) {
    if (Ty != null) {
      w($"         public readonly {Ty} value;");
      w($"");
    }
  }

  private static void CaseConstructor(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public {C}({Ty == null ? "" : $"{Ty} value"}) {{");
    w($"           {Ty == null ? "" : $"this.value = value; "}");
    if (Ty == null) {
      w($"           this.hashCode = HashCode.Combine(\"{C}\");");
    } else {
      w($"           this.hashCode = HashCode.Combine(\"{C}\", this.value);");
    }
    w($"         }}");
  }

  private static void CaseMatch_(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public override T Match_<T>(Visitor<T> c) => c.{C}({Ty == null ? "" : "value"});");
  }

  private static void CaseAs(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public override Immutable.Option<{Ty == null ? "Immutable.Unit" : Ty}> As{C} {{");
    w($"           get => Immutable.Option.Some<{Ty == null ? "Immutable.Unit" : Ty}>({Ty == null ? "Immutable.Unit.unit" : "this.value"});");
    w($"         }}");
  }

  private static void CaseEquality(this Action<string> w, string qualifier, string name, string C, string Ty) {
    w($"         public static bool operator ==({C} a, {C} b)");
    w($"           => Equality.Operator(a, b);");
    w($"         public static bool operator !=({C} a, {C} b)");
    w($"           => !(a == b);");
    w($"         public override bool Equals(object other)");
    w($"           => Equality.Untyped<{C}>(this, other, x => x as {C}, x => x.hashCode{(Ty == null) ? "" : ", x => x.value"});");
    w($"         public override bool Equals({name} other)");
    w($"           => Equality.Equatable<{name}>(this, other);");
    w($"         private readonly int hashCode;");
    w($"         public override int GetHashCode() => hashCode;");
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
      w($"      }}");
    }
  }

  private static void VariantClass(this Action<string> w, string qualifier, string name, Variant variant) {
    // Mark as partial to allow defining implicit conversions
    // and other operators. It would be cleaner to directly
    // specify these and keep the class impossible to extend.
    w($"  public abstract partial class {name} : IEquatable<{name}>, IString {{");
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
    w.Is(qualifier, name, variant);
    w($"");
    w.ChainLens(qualifier, name, variant);
    w($"");
    w.GetTag(qualifier, name, variant);
    w($"");
    w.GetValue(qualifier, name, variant);
    w($"");
    w.Equality(qualifier, name, variant);
    w($"");
    w.StringConversion(qualifier, name, variant);
    w($"");
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