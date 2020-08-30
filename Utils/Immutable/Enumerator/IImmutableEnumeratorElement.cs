using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Immutable {
  public interface IImmutableEnumeratorElement<T> : 
  IEquatable<IImmutableEnumeratorElement<T>>, IDisposable {
    T First { get; }
    IImmutableEnumerator<T> Rest { get; }
  }
}