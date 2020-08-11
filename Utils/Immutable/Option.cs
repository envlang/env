/*
namespace Immutable {
  using System;

  public interface Option<out T> {
    U Match_<U>(Func<T, U> Some, Func<U> None);
  }

  public static class Option {
    public static Option<T> Some<T>(T value) => new Types.Some<T>(value);
    public static Option<T> None<T>() => new Types.None<T>();

    private static class Types {
      public class Some<T> : Option<T> {
        public readonly T value;

        public Some(T value) { this.value = value; }

        public U Match_<U>(Func<T, U> Some, Func<U> None) => Some(value);
      }

      public class None<T> : Option<T> {
        public None() { }

        public U Match_<U>(Func<T, U> Some, Func<U> None) => None();
      }
    }
  }

  public static class OptionExtensionMethods {
    public static Option<T> Some<T>(this T value) => Option.Some<T>(value);
    public static U Match<T, U>(this Option<T> o, Func<T, U> Some, Func<U> None)
      => o.Match_(Some, None);
 }
}
*/