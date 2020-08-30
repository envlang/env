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
    // state = AlreadyUnfoldedEnd
    //       | AlreadyUnfolded of ImmutableEnumeratorElement<U>
    //       | NotUnfoldedYet of IEnumerator<U>
    private abstract class State {
      public abstract T Match<T>(
        Func<ImmutableEnumeratorElement, T> AlreadyUnfolded,
        Func<T> AlreadyUnfoldedEnd,
        Func<IEnumerator<U>, Last, T> NotUnfoldedYet);

      public class AlreadyUnfolded : State {
        private readonly ImmutableEnumeratorElement value;
        public AlreadyUnfolded(ImmutableEnumeratorElement value) {
          this.value = value;
        }
        public override T Match<T>(
          Func<ImmutableEnumeratorElement, T> AlreadyUnfolded,
          Func<T> AlreadyUnfoldedEnd,
          Func<IEnumerator<U>, Last, T> NotUnfoldedYet)
          => AlreadyUnfolded(value);
      }

      public class AlreadyUnfoldedEnd : State {
        public AlreadyUnfoldedEnd() {}
        public override T Match<T>(
          Func<ImmutableEnumeratorElement, T> AlreadyUnfolded,
          Func<T> AlreadyUnfoldedEnd,
          Func<IEnumerator<U>, Last, T> NotUnfoldedYet)
          => AlreadyUnfoldedEnd();
      }

      public class NotUnfoldedYet : State {
        private readonly IEnumerator<U> enumerator;
        private readonly Last last;            // readonly
        public NotUnfoldedYet(IEnumerator<U> enumerator, Last last) {
          this.enumerator = enumerator;
          this.last = last;
        }
        public override T Match<T>(
          Func<ImmutableEnumeratorElement, T> AlreadyUnfolded,
          Func<T> AlreadyUnfoldedEnd,
          Func<IEnumerator<U>, Last, T> NotUnfoldedYet)
          => NotUnfoldedYet(enumerator, last);
      }
    }
  }
}