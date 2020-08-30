using static Generator;

public static class EnumeratorGenerator {
  public static void Main() {
    Generate(
      "Utils/Immutable/EnumeratorGenerated.cs",
      "",
      "namespace Immutable {",
      "}",
      "Immutable.",
      Types(
// Our boilerplate generator does not support
// defining generic types for now.
/*
        Record("PureImmutableGenerator<T, U>",
          Field("T", "state"),
          Field("Func<T, Option<Tuple<U, IImmutableEnumerator<U>>>>", "generator"))
          */));
  }
}
