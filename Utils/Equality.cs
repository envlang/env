using System;
using System.Linq;

public static class Equality {
  public static bool Test<T>(T a, T b, Func<T, T, bool> compare) {
    if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else {
      return compare(a, b);
    }
  }


  public static bool Field<T, U>(T a, T b, Func<T, U> getField, Func<U, U, bool> compareField) {
    if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else {
      return compareField(getField(a), getField(b));
    }
  }
}