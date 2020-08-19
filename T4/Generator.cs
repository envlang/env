// Code quality of this file: low.

using System;
using System.Collections.Generic;
using System.Linq;

public enum Kind {
  Record,
  Variant,
}
public static class Generator {
  public static void WriteVariant(this System.IO.StreamWriter o, string header, string footer, string qualifier, string name, Dictionary<string, string> variant) {
    o.WriteLine($"{header}");
    o.WriteLine("");

    o.WriteLine($"  /* To match against an instance of {name}, write:");
    o.WriteLine($"     x.Match(");
    o.WriteLine(String.Join(",\n", variant.Select(@case => 
                $"       {@case.Key}: {@case.Value == null ? "()" : "value"} => throw new NotImplementedYetException(),")));
    o.WriteLine($"     )");
    o.WriteLine($"  */");

    o.WriteLine($"  public abstract class {name} : IEquatable<{name}> {{");
    o.WriteLine($"    public abstract T Match_<T>(Visitor<T> c);");
    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      o.WriteLine($"    public static {name} {C}{Ty == null ? $" = new {C}()" : $"({Ty} value) => new {C}(value)"};");
    }

    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;
      o.WriteLine($"    public virtual Immutable.Option<{C}> As{C}() => Immutable.Option.None<{C}>();");
    }

    o.WriteLine($"    private string GetTag() {{");
    o.WriteLine($"      return this.Match(");
    o.WriteLine(String.Join(",\n", variant.Select(@case =>
                $"        {@case.Key}: {@case.Value == null ? "()" : "value"} => \"{@case.Key}\"")));
    o.WriteLine($"      );");
    o.WriteLine($"    }}");
    o.WriteLine("");
    o.WriteLine($"    public abstract bool Equals(Object other);");
    o.WriteLine($"    public abstract bool Equals({name} other);");
    o.WriteLine("");
    o.WriteLine($"  }}");
    o.WriteLine("");

    foreach (var @case in variant) {
      var C = @case.Key;
      var Ty = @case.Value;

      o.WriteLine(
        $"  public partial class Visitor<T> {{"
        + $" public Func<{Ty == null ? "" : $"{Ty}, "}T> {C} {{ get; set; }} "
        + @"}");
      o.WriteLine("");

      o.WriteLine($"  public sealed class {C} : {name} {{");
      if (Ty != null) {
        o.WriteLine($"    public readonly {Ty} value;");
      }
      o.WriteLine($"    public {C}({Ty == null ? "" : $"{Ty} value"}) {{ {Ty == null ? "" : $"this.value = value; "}}}");
      o.WriteLine($"    public override T Match_<T>(Visitor<T> c) => c.{C}({Ty == null ? "" : "value"});");
      o.WriteLine($"    public override Immutable.Option<{C}> As{C}() => Immutable.Option.Some<{C}>(this);");
      o.WriteLine($"    public static bool operator ==({C} a, {C} b)");
      o.WriteLine($"      => Equality.Operator(a, b);");
      o.WriteLine($"    public static bool operator !=({C} a, {C} b)");
      o.WriteLine($"      => !(a == b);");
      o.WriteLine($"    public override bool Equals(object other)");
      if (Ty == null) {
        o.WriteLine($"      => Equality.Untyped<{C}>(this, other, x => x as {C});");
      } else {
        o.WriteLine($"      => Equality.Untyped<{C}>(this, other, x => x as {C}, x => x.value);");
      }
      o.WriteLine($"    public override bool Equals({name} other)");
      o.WriteLine($"      => Equality.Equatable<{name}>(this, other);");
      o.WriteLine($"    public override int GetHashCode()");
      if (Ty == null) {
        o.WriteLine($"      => HashCode.Combine(\"{C}\");");
      } else {
        o.WriteLine($"      => HashCode.Combine(\"{C}\", this.value);");
      }
      o.WriteLine("");
      o.WriteLine($"    public override string ToString() => \"{C}\";");
      o.WriteLine($"  }}");
      o.WriteLine("");
    }

    o.WriteLine($"}}");

    o.WriteLine($"public static class {name}ExtensionMethods {{");
    o.WriteLine($"  public static T Match<T>(");
    o.WriteLine($"      this {qualifier}{name} e,");
    o.WriteLine(String.Join(",\n", variant.Select(c =>
                $"      Func<{c.Value == null ? "" : $"{c.Value}, "}T> {c.Key}")));
    o.WriteLine($"    ) {{");
    o.WriteLine($"    return e.Match_(new {qualifier}Visitor<T> {{");
    o.WriteLine(String.Join(",\n", variant.Select(c =>
                $"      {c.Key} = {c.Key}")));
    o.WriteLine($"    }});");
    o.WriteLine($"  }}");
    o.WriteLine($"{footer}");
  }

  public static void WriteRecord(this System.IO.StreamWriter o, string header, string footer, string qualifier, string name, Dictionary<string, string> record) {
    o.WriteLine($"{header}");
    o.WriteLine("");
    o.WriteLine($"  public class {name} : IEquatable<{name}> {{");
    foreach (var @field in record) {
      var F = @field.Key;
      var Ty = @field.Value;
      o.WriteLine($"    public readonly {Ty} {F};");
    }
    o.WriteLine($"    public {name}(");
    o.WriteLine(String.Join(",\n", record.Select(@field =>
                $"        {@field.Value} {@field.Key}")));
    o.WriteLine($"      ) {{");
    foreach (var @field in record) {
      var F = @field.Key;
      var Ty = @field.Value;
      o.WriteLine($"    this.{F} = {F};");
    }
    o.WriteLine($"    }}");
    o.WriteLine($"");
    o.WriteLine($"    public static bool operator ==({name} a, {name} b)");
    o.WriteLine($"      => Equality.Operator(a, b);");
    o.WriteLine($"    public static bool operator !=({name} a, {name} b)");
    o.WriteLine($"      => !(a == b);");
    o.WriteLine($"    public override bool Equals(object other)");
    o.WriteLine($"      => Equality.Untyped<{name}>(this, other, x => x as {name},");
    o.WriteLine(String.Join(",\n", record.Select(@field =>
                $"          x => x.{@field.Key}")));
    o.WriteLine($"        );");
    o.WriteLine($"    public bool Equals({name} other)");
    o.WriteLine($"      => Equality.Equatable<{name}>(this, other);");
    o.WriteLine($"    public override int GetHashCode()");
    o.WriteLine($"      => HashCode.Combine(\"{name}\",");
    o.WriteLine(String.Join(",\n", record.Select(@field =>
                $"          this.{@field.Key}")));
    o.WriteLine($"        );");
    o.WriteLine($"  }}");
    o.WriteLine($"{footer}");
  }

  public static void Generate(string outputFile, string singleHeader, string header, string footer, string qualifier, Dictionary<string, Tuple<Kind, Dictionary<string, string>>> types) {
    using (var o = new System.IO.StreamWriter(outputFile)) {
      o.WriteLine("// This file was generated by Generator.cs");
      o.WriteLine("");

      o.WriteLine("using System;");
      o.WriteLine($"{singleHeader}");
      o.WriteLine("");
      foreach (var type in types) {
        var name = type.Key;
        var kind = type.Value.Item1;
        var components = type.Value.Item2;
        switch (kind) {
          case Kind.Record:
            o.WriteRecord(header, footer, qualifier, name, @components);
            break;
          case Kind.Variant:
            o.WriteVariant(header, footer, qualifier, name, @components);
            break;
        }
        o.WriteLine("");
      }
    }
  }

  // Below are shorthands for making the last argument to Generate().
  public static Dictionary<string, Tuple<Kind, Dictionary<string, string>>> Types(params Tuple<string, Tuple<Kind, Dictionary<string, string>>>[] types)
    => types.ToDictionary(t => t.Item1, t => t.Item2);

  public static Tuple<string, Tuple<Kind, Dictionary<string, string>>> Record(string name, params Tuple<string, string>[] fields)
    => new Tuple<string, Tuple<Kind, Dictionary<string, string>>>(
         name,
         new Tuple<Kind, Dictionary<string, string>>(
           Kind.Record,
           fields.ToDictionary(t => t.Item1, t => t.Item2)));

  public static Tuple<string, Tuple<Kind, Dictionary<string, string>>> Variant(string name, params Tuple<string, string>[] cases)
    => new Tuple<string, Tuple<Kind, Dictionary<string, string>>>(
         name,
         new Tuple<Kind, Dictionary<string, string>>(
           Kind.Variant,
           cases.ToDictionary(t => t.Item1, t => t.Item2)));

  public static Tuple<string, string> Field(string type, string name)
    => new Tuple<string, string>(name, type);

  public static Tuple<string, string> Case(string type, string name)
    => new Tuple<string, string>(name, type);

  public static Tuple<string, string> Case(string name)
    => new Tuple<string, string>(name, null);
}