using System;
using System.Linq;

public static class Equality {
  // TODO: values returned by fieldAccessors should implement IEquatable.

  // T can be any supertype of the instances passed to the == operator.
  public static bool Operator(Object a, Object b) {
    if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else if (Object.ReferenceEquals(b, null)) {
      return false;
    } else {
      return ((Object)a).Equals(b);
    }
  }

  // T must be the exact type of the receiver object whose
  // Object.Equals(Object other) method is invoking
  // Untyped(this, other).
  public static bool Untyped<T>(T a, Object b, Func<Object, T> cast, params Func<T, Object>[] fieldAccessors) {
    if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else if (Object.ReferenceEquals(b, null)) {
      return false;
    } else {
      var castB = cast(b);
      if (Object.ReferenceEquals(castB, null)) {
        return false;
      } else {
        foreach (var accessor in fieldAccessors) {
          var aFieldValue = accessor(a);
          var bFieldValue = accessor(castB);
          if (Object.ReferenceEquals(aFieldValue, null)) {
            return Object.ReferenceEquals(bFieldValue, null);
          } else if (Object.ReferenceEquals(bFieldValue, null)) {
            return false;
          } else {
            return aFieldValue.Equals(bFieldValue);
          }
        }
        return true;
      }
    }
  }

  // T must be the exact type of the receiver object whose
  // IEquatable<U>.Equals(U other) method is invoking
  // Equatable(this, other).
  public static bool Equatable<T>(T a, Object b) where T : IEquatable<T> {
    if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else if (Object.ReferenceEquals(b, null)) {
      return false;
    } else {
      return ((Object)a).Equals(b);
    }
  }
}