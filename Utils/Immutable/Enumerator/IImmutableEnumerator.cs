using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Immutable {
  public interface IImmutableEnumerator<T> : IEquatable<IImmutableEnumerator<T>>, IEnumerable<T>, IDisposable {
    Option<IImmutableEnumeratorElement<T>> MoveNext();
  }
}