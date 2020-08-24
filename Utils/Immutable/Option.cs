namespace Immutable {
  using System;

  public interface Option<out T> : System.Collections.Generic.IEnumerable<T> {
    U Match_<U>(Func<T, U> some, Func<U> none);
    bool IsSome { get; }
    bool IsNone { get; }
  }

  public static class Option {
    public static Option<T> Some<T>(T value) => new Types.Some<T>(value);
    public static Option<T> None<T>() => new Types.None<T>();

    private static class Types {
      public class Some<T> : Option<T>, System.Collections.IEnumerable {
        public readonly T value;

        public Some(T value) { this.value = value; }

        public U Match_<U>(Func<T, U> Some, Func<U> None) => Some(value);

        public bool IsSome { get => true; }
        public bool IsNone { get => false; }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
          => value.Singleton().GetEnumerator();
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
          => this.GetEnumerator();
      }

      public class None<T> : Option<T>, System.Collections.IEnumerable {
        public None() { }

        public U Match_<U>(Func<T, U> Some, Func<U> None) => None();

        public bool IsSome { get => false; }
        public bool IsNone { get => true; }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
          => System.Linq.Enumerable.Empty<T>().GetEnumerator();
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
          => this.GetEnumerator();
      }
    }
  }

  public static class OptionExtensionMethods {
    public static Option<T> Some<T>(this T value) => Option.Some<T>(value);
    
    public static U Match<T, U>(this Option<T> o, Func<T, U> some, Func<U> none)
      => o.Match_(some, none);

    public static U Match<T, U>(this Option<T> o, Func<T, U> some, U none)
      => o.Match_(some, () => none);

    public static Option<U> Map<T, U>(this Option<T> o, Func<T, U> some)
      => o.Match_(value => some(value).Some(), () => Option.None<U>());

    public static Option<U> IfSome<T, U>(this Option<T> o, Func<T, U> some)
      => o.Map(some);

    public static Option<U> Bind<T, U>(this Option<T> o, Func<T, Option<U>> f)
      => o.Match_(some => f(some), () => Option.None<U>());

    public static T Else<T>(this Option<T> o, Func<T> none)
      => o.Match_(some => some, none);

    public static T Else<T>(this Option<T> o, T none)
      => o.Match_(some => some, () => none);

    public static Option<T> Else<T>(this Option<T> o, Func<Option<T>> none)
      => o.Match_(value => value.Some(), none);

    public static T ElseThrow<T>(this Option<T> o, Func<Exception> none)
      => o.Match_(value => value, () => throw none());
  }
}