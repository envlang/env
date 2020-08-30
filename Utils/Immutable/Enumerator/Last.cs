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
    private class Last {
      // This is one of the three mutable fields in this file.
      // It is used to update the pointer to the only
      // NotUnfoldedYet object for a given underlying enumerator.
      // This allows a call on .Dispose() to clean up after the
      // underlying enumerator.
      public LazyList LAST;
      // This is one of the three mutable fields in this file.
      // It is used to indicate that the .Dispose() method has
      // been called and that it is therefore unsafe to continue
      // using the immutable enumerator for this underlying
      // enumerator.
      public bool EXPLICITLY_DISPOSED = false;
    }
  }
}