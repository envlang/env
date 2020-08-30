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
    // enumeratorElement = {
    //   current: U;
    //   next: lazylist;
    // }
    private sealed class ImmutableEnumeratorElement : IImmutableEnumeratorElement<U> {
      private readonly U current;     // immutable
      private readonly ImmutableEnumerator<U> rest;
      private readonly int hashCode;
    
      public ImmutableEnumeratorElement(U current, LazyList next, Last last) {
        this.current = current;
        this.rest = new ImmutableEnumerator<U>(next, last);
        this.hashCode = Equality.HashCode(current, rest);
      }

      public U First {
        get {
          if (this.rest.last.EXPLICITLY_DISPOSED) {
            throw new ObjectDisposedException("Cannot use an ImmutableEnumerator after it was explicitly disposed.");
          }
          return current;
        }
      }

      public IImmutableEnumerator<U> Rest { get => rest; }

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
        rest.last.LAST.CallDispose();
        rest.last.EXPLICITLY_DISPOSED = true;
      }

      public Option<IImmutableEnumeratorElement<U>> MoveNext()
        => rest.MoveNext();

      public static bool operator ==(ImmutableEnumeratorElement a, ImmutableEnumeratorElement b)
        => Equality.Operator(a, b);
      public static bool operator !=(ImmutableEnumeratorElement a, ImmutableEnumeratorElement b)
        => !(a == b);
      public override bool Equals(object other)
        => Equality.Untyped<ImmutableEnumeratorElement>(
          this,
          other,
          x => x as ImmutableEnumeratorElement,
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
          x => x.current,
          x => x.rest);
      public bool Equals(IImmutableEnumeratorElement<U> other)
        => Equality.Equatable<IImmutableEnumeratorElement<U>>(this, other);
      public override int GetHashCode() => hashCode;
      public override string ToString() => "ImmutableEnumeratorElement";
    }
  }
}