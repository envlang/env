// Code quality of this file: low.
// We need an annotation on lambdas to lift them to equatable singletons.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Immutable {
public static partial class ImmutableEnumeratorExtensionMethods {
    public static IImmutableEnumerator<T> ToImmutableEnumerator<T>(this IEnumerator<T> e)
      => ImmutableEnumerator<T>.Make(e);

    public static IImmutableEnumerator<T> GetImmutableEnumerator<T>(this IEnumerable<T> e)
      => e.GetEnumerator().ToImmutableEnumerator();

    public static Option<Tuple<T, IImmutableEnumerator<T>>> FirstAndRest<T>(this IImmutableEnumerator<T> e)
      => e.MoveNext()
          .Match(
            None: () =>
              Option.None<Tuple<T, IImmutableEnumerator<T>>>(),
            Some: elt =>
              new Tuple<T, IImmutableEnumerator<T>>(
                elt.First,
                elt.Rest
              ).Some()
      );

    public static IEnumerable<T> ToIEnumerable<T>(this IImmutableEnumerator<T> e) {
      var next = e.MoveNext();
      while (next.IsSome) {
        var elem = next.ElseThrow(new Exception("impossible"));
        yield return elem.First;
        next = elem.Rest.MoveNext();
      }
    }

    [F]
    private partial class TakeUntil_<T> {
      public Option<Tuple<T, IImmutableEnumerator<T>>> F(
        ValueTuple<IImmutableEnumerator<T> /*e*/, IEqF<IImmutableEnumerator<T>, bool> /*predicate*/> t
      )
      => t.Item2.F(t.Item1)
        ? Option.None<Tuple<T, IImmutableEnumerator<T>>>()
        : t.Item1.MoveNext().IfSome(next =>
          new Tuple<T, IImmutableEnumerator<T>>(
            next.First,
            TakeUntil<T>(next.Rest, t.Item2)));
    }


    public static IImmutableEnumerator<T> TakeUntil<T>(this IImmutableEnumerator<T> e, IEqF<IImmutableEnumerator<T>, bool> predicate)
      => new PureImmutableEnumerator<
          ValueTuple<
            IImmutableEnumerator<T>,
            IEqF<IImmutableEnumerator<T>, bool>
          >,
          T>(
        (e, predicate),
        TakeUntil_<T>.Eq);

    [F]
    private partial class Empty_<T> {
      public Option<Tuple<T, IImmutableEnumerator<T>>> F(
        Unit _
      )
      => Option.None<Tuple<T, IImmutableEnumerator<T>>>();
    }

    public static IImmutableEnumerator<T> Empty<T>()
      => new PureImmutableEnumerator<Unit, T>(
        Unit.unit,
        Empty_<T>.Eq);

    [F]
    private partial class ImSingleton_<T> {
      public Option<Tuple<T, IImmutableEnumerator<T>>> F(
        T value
      )
      => new Tuple<T, IImmutableEnumerator<T>>(
           value,
           Empty<T>()
         ).Some();
    }

    public static IImmutableEnumerator<T> ImSingleton<T>(this T value)
      => new PureImmutableEnumerator<T, T>(
        value,
        ImSingleton_<T>.Eq);

    [F]
    private partial class Concat_<T> {
      public Option<Tuple<T, IImmutableEnumerator<T>>> F(
        ValueTuple<IImmutableEnumerator<T>, /*e1*/ IImmutableEnumerator<T> /*e2*/> t
      )
      => t.Item1.MoveNext().Match(
        Some: element =>
          new Tuple<T, IImmutableEnumerator<T>>(
            element.First,
            element.Rest.Concat(t.Item2)
          ).Some(),
        None: () =>
          t.Item2.MoveNext().IfSome(element =>
            new Tuple<T, IImmutableEnumerator<T>>(
              element.First,
              element.Rest)));
    }

    public static IImmutableEnumerator<T> Concat<T>(this IImmutableEnumerator<T> e1, IImmutableEnumerator<T> e2)
      => new PureImmutableEnumerator<
          ValueTuple<
            IImmutableEnumerator<T> /*e1*/,
            IImmutableEnumerator<T> /*e2*/
          >,
          T>(
        (e1, e2),
        Concat_<T>.Eq); 

    [F]
    private partial class Lazy_<T, U> {
      public Option<Tuple<U, IImmutableEnumerator<U>>> F(
        ValueTuple<T, /*e*/ IEqF<T, IImmutableEnumerator<U>> /*f*/> t
      )
      => t.Item2.F(t.Item1).MoveNext().IfSome(element =>
          new Tuple<U, IImmutableEnumerator<U>>(
            element.First,
            element.Rest
          ));
    }

    // Apply a transformation to an immutable enumerator.
    // The transformation function is only called when the
    // result is stepped. It should only step its input
    // enough to produce one element, but not more.
    public static IImmutableEnumerator<U> Lazy<T, U>(
      this IImmutableEnumerator<T> e,
      IEqF<IImmutableEnumerator<T>, IImmutableEnumerator<U>> f)
      => new PureImmutableEnumerator<
          ValueTuple<
            IImmutableEnumerator<T> /*e*/,
            IEqF<IImmutableEnumerator<T>, IImmutableEnumerator<U>> /*f*/
          >,
          U>(
        (e, f),
        Lazy_<IImmutableEnumerator<T>, U>.Eq);

    public static IImmutableEnumerator<U> Lazy<T, U, V>(
      this IImmutableEnumerator<T> e,
      IEqF<
        ValueTuple<IImmutableEnumerator<T> /*e*/, V /*v*/>,
        IImmutableEnumerator<U>
      > f,
      V v)
      => new PureImmutableEnumerator<
          ValueTuple<
            ValueTuple<IImmutableEnumerator<T> /*e*/, V /*v*/>,
            IEqF<
              ValueTuple<IImmutableEnumerator<T> /*e*/,
                         V /*v*/>,
              IImmutableEnumerator<U>> /*f*/
          >,
          U>(
        ((e, v), f),
        Lazy_<ValueTuple<IImmutableEnumerator<T>, V>, U>.Eq);

    [F]
    private partial class Flatten_<T> {
      public IImmutableEnumerator<T> F(
          IImmutableEnumerator<IImmutableEnumerator<T>> e
        )
        => e.MoveNext().Match(
          Some: element =>
            element.First.Concat(element.Rest.Flatten()),
          None: () =>
            Empty<T>());
    }

    public static IImmutableEnumerator<T> Flatten<T>(this IImmutableEnumerator<IImmutableEnumerator<T>> e)
      => e.Lazy(Flatten_<T>.Eq);

    [F]
    private partial class Select_<T, U> {
      public IImmutableEnumerator<U> F(
          ValueTuple<IImmutableEnumerator<T> /*e*/, IEqF<ValueTuple<T, IImmutableEnumerator<T>>, U> /*f*/> t
        )
        => t.Item1.MoveNext().Match(
          Some: element =>
            t.Item2.F((element.First, t.Item1)).ImSingleton().Concat(
              element.Rest.Select(t.Item2)),
          None: () =>
            Empty<U>());
    }

    public static IImmutableEnumerator<U> Select<T, U>(this IImmutableEnumerator<T> e, IEqF<ValueTuple<T, IImmutableEnumerator<T>>, U> f)
      => Lazy(e, Select_<T, U>.Eq, f);

    [F]
    private partial class SelectAggregate_<A, T, U> {
      public IImmutableEnumerator<U> F(
          ValueTuple<IImmutableEnumerator<T> /*e*/, ValueTuple<A, IEqF<A, ValueTuple<T, IImmutableEnumerator<T>>, ValueTuple<A, U>> /*f*/>> t
        )
        {
          var (e, accf) = t;
          var (acc, f) = accf;
          return e.MoveNext().Match(
          Some: element => {
            var res = f.F(acc, (element.First, e));
            var newAcc = res.Item1;
            var result = res.Item2;
            return result.ImSingleton().Concat<U>(
              element.Rest.SelectAggregate(newAcc, f));
          },
          None: () =>
            Empty<U>());
        }
    }

    public static IImmutableEnumerator<U> SelectAggregate<A, T, U>(this IImmutableEnumerator<T> e, A acc, IEqF<A, ValueTuple<T, IImmutableEnumerator<T>>, ValueTuple<A, U>> f)
      => Lazy(e, SelectAggregate_<A, T, U>.Eq, (acc, f));
}
}