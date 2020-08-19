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

  public static int HashCode(Object o1) => System.HashCode.Combine(o1);
  public static int HashCode(Object o1, Object o2) => System.HashCode.Combine(o1, o2);
  public static int HashCode(Object o1, Object o2, Object o3) => System.HashCode.Combine(o1, o2, o3);
  public static int HashCode(Object o1, Object o2, Object o3, Object o4) => System.HashCode.Combine(o1, o2, o3, o4);
  public static int HashCode(Object o1, Object o2, Object o3, Object o4, Object o5) => System.HashCode.Combine(o1, o2, o3, o4, o5);
  public static int HashCode(Object o1, Object o2, Object o3, Object o4, Object o5, Object o6) => System.HashCode.Combine(o1, o2, o3, o4, o5, o6);
  public static int HashCode(Object o1, Object o2, Object o3, Object o4, Object o5, Object o6, Object o7) => System.HashCode.Combine(o1, o2, o3, o4, o5, o6, o7);
  public static int HashCode(Object o1, Object o2, Object o3, Object o4, Object o5, Object o6, Object o7, Object o8) => System.HashCode.Combine(o1, o2, o3, o4, o5, o6, o7, o8);

  public static int HashCode(Object o1, Object o2, Object o3, Object o4, Object o5, Object o6, Object o7, Object o8, params Object[] objects) {
    var hash = System.HashCode.Combine(o1, o2, o3, o4, o5, o6, o7, o8);foreach (var o in objects) {
      hash = System.HashCode.Combine(hash, o);
    }
    return hash;
  }
}