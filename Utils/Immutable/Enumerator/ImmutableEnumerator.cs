using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// enumerator = { next: lazylist }
// enumeratorElement = {
//   current: U;
//   next: lazylist;
// }
// state = AlreadyUnfoldedEnd
//       | AlreadyUnfolded of ImmutableEnumeratorElement<U>
//       | NotUnfoldedYet of IEnumerator<U>
// lazylist = state ref

namespace Immutable {
  // enumerator = { next: lazylist }
  public partial class ImmutableEnumerator<U> : IImmutableEnumerator<U> {
    private readonly LazyList next; // readonly
    private readonly Last last; // readonly
    private readonly int hashCode;

    private ImmutableEnumerator(LazyList next, Last last) {
      this.next = next;
      this.last = last;
      // Use the default hashCode on the single mutable LazyList
      // instance for this position in the enumerator.
      this.hashCode = next.GetHashCode();
    }

    public void Dispose() {
      // Calling this method on any copy of any immutable
      // enumerator, including via a using(){} directive
      // is DANGEROUS: it will make it impossible to enumerate
      // past the current position of the underlying enumerator,
      // when starting from any copy of any immutable enumerator
      // for that underlying enumerator.
      // As a precaution, and to catch bugs early, we make it
      // impossible to use any of the copies of any immutable
      // enumerator for that underlying enumerator once this method
      // has been called.
      last.LAST.CallDispose();
      last.EXPLICITLY_DISPOSED = true;
    }

    public static ImmutableEnumerator<U> Make(IEnumerator<U> e) {
      var last = new Last { LAST = null };
      var lst = new LazyList {
        NEXT = new State.NotUnfoldedYet(e, last)
      };
      last.LAST = lst;
      return new ImmutableEnumerator<U>(lst, last);
    }

    public Option<IImmutableEnumeratorElement<U>> MoveNext() {
      if (this.last.EXPLICITLY_DISPOSED) {
        throw new ObjectDisposedException("Cannot use an ImmutableEnumerator after it was explicitly disposed.");
      }
      return next.NEXT.Match(
        AlreadyUnfoldedEnd: () =>
          Option.None<IImmutableEnumeratorElement<U>>(),
        AlreadyUnfolded: element =>
          element.Some(),
        NotUnfoldedYet: (e, last) => {
          if (e.MoveNext()) {
            var lst = new LazyList {
              NEXT = new State.NotUnfoldedYet(e, last)
            };
            last.LAST = lst;
            var elem = new ImmutableEnumeratorElement(
              current: e.Current,
              next: lst,
              last : last);
            next.NEXT = new State.AlreadyUnfolded(elem);
            return elem.Some();
          } else {
            next.NEXT = new State.AlreadyUnfoldedEnd();
            // Call .Dispose() on the underlying enumerator
            // because we have read all its elements.
            e.Dispose();
            return Option.None<IImmutableEnumeratorElement<U>>();
          }
        }
      );
    }

    public static bool operator ==(ImmutableEnumerator<U> a, ImmutableEnumerator<U> b)
      => Equality.Operator(a, b);
    public static bool operator !=(ImmutableEnumerator<U> a, ImmutableEnumerator<U> b)
      => !(a == b);
    public override bool Equals(object other)
      => Equality.Untyped<ImmutableEnumerator<U>>(
        this,
        other,
        x => x as ImmutableEnumerator<U>,
        x => x.hashCode,
        // Two immutable enumerators are equal if and only if
        // they are at the same position and use the same
        // underlying enumerable. In that case they are guaranteed
        // to behave identically to an outside observer (except for
        // side-effects caused by the iteration of the underlying
        // enumerator, which only occur on the first .MoveNext()
        // call, if it is called on several equal immutable
        // enumerators). This is also true for the
        // ImmutableEnumeratorElement subclass, because if two of
        // these have the same underlying generator, their current
        // field are necessarily one and the same.
        (x, y) => Object.ReferenceEquals(x.next, y.next));
    public bool Equals(IImmutableEnumerator<U> other)
      => Equality.Equatable<IImmutableEnumerator<U>>(this, other);
    public override int GetHashCode() => hashCode;
    public override string ToString() => "ImmutableEnumerator";

    public IEnumerator<U> GetEnumerator()
      => this.ToIEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
      => this.ToIEnumerable().GetEnumerator();
  }
}