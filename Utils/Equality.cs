using System;
using System.Linq;

public static class Equality {
  // TODO: values returned by fieldAccessors should implement IEquatable.

  // T can be any supertype of the instances passed to the == operator.
  public static bool Operator(Object a, Object b) {
    if (Object.ReferenceEquals(a, b)) {
      return true;
    } else if (Object.ReferenceEquals(a, null)) {
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
  public static bool Untyped<T>(T a, Object b, Func<Object, T> cast, Func<T, int> hashCode, params Func<T, T, bool>[] comparers) {
    if (Object.ReferenceEquals(a, b)) {
      // Short path when the two references are the same.
      return true;
    } else if (Object.ReferenceEquals(a, null)) {
      return Object.ReferenceEquals(b, null);
    } else if (Object.ReferenceEquals(b, null)) {
      return false;
    } else {
      var castB = cast(b);
      if (Object.ReferenceEquals(castB, null)) {
        return false;
      } else {
        if (hashCode(a) != hashCode(castB)) {
          return false;
        } else {
          foreach (var comparer in comparers) {
            if (!comparer(a, castB)) {
              return false;
            } else {
              // continue.
            }
          }
          return true;
        }
      }
    }
  }

  public static bool Untyped<T>(T a, Object b, Func<Object, T> cast, Func<T, int> hashCode, params Func<T, Object>[] fieldAccessors)
    => Untyped<T>(
         a,
         b,
         cast,
         hashCode,
         (aa, bb) =>
           fieldAccessors.All(
             accessor =>
               Equality.Operator(
                 accessor(aa),
                 accessor(bb))));

  // common method when there are no fields or comparers.
  public static bool Untyped<T>(T a, Object b, Func<Object, T> cast, Func<T, int> hashCode)
    => Untyped<T>(a, b, cast, hashCode, (aa, bb) => true);

  // T must be the exact type of the receiver object whose
  // IEquatable<U>.Equals(U other) method is invoking
  // Equatable(this, other).
  public static bool Equatable<T>(T a, Object b) where T : IEquatable<T> {
    if (Object.ReferenceEquals(a, b)) {
      // Short path when the two references are the same.
      return true;
    } else if (Object.ReferenceEquals(a, null)) {
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