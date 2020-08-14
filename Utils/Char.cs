using System;

public static class CharExtensionMethods {
  public static bool IsHighSurrogate(this Char c)
    => Char.IsHighSurrogate(c);

  public static bool IsLowSurrogate(this Char c)
    => Char.IsLowSurrogate(c);
}