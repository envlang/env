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
    // lazylist = state ref
    private class LazyList {
      // This is one of the three mutable fields in this file.
      // There is one LazyList object created each time
      // the underlying enumerator's MoveNext() method
      // is called, except for the last failed MoveNext()
      // call. There is also one initial LazyList element
      // created when wrapping the underlying enumerator
      // to create an immutable enumerator.
      public State NEXT;           // mutable

      ~LazyList() { this.CallDispose(); }
      public void CallDispose() {
        NEXT.Match(
          AlreadyUnfoldedEnd: () => Unit.unit,
          AlreadyUnfolded:    _e => Unit.unit,
          // Since at any one time there is only one LazyList
          // whose state is NotUnfoldedYet (except during the
          // unfolding, when there two such lists manipulated for
          // a brief time by the same lambda), the enumerator
          // should be disposed of when the destructor of this
          // class is called.
          NotUnfoldedYet:     (r, last) => {
            r.Dispose();
            return Unit.unit;
          }
        );
      }
    }
  }
}