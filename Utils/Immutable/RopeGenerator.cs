using static Generator;

public static class RopeGenerator {
  public static void Main() {
    Generator.Generate(
      "Utils/Immutable/RopeGenerated.cs",
      "",
      "namespace Immutable {",
      "}",
      "Immutable.",
      Types(
        Variant("Rope",
          Case("string", "Leaf"),
          Case("Immutable.Node", "Node")),
        Record("Node",
          Field("Rope", "a"),
          Field("Rope", "b"))));
  }
}