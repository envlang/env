using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Immutable {
  public class PureImmutableEnumeratorElement<U> : IImmutableEnumeratorElement<U> {
    private U element;
    private IImmutableEnumerator<U> rest;
    private int hashCode;

    public PureImmutableEnumeratorElement(U element, IImmutableEnumerator<U> rest) {
      this.element = element;
      this.rest = rest;
      this.hashCode = Equality.HashCode("PureImmutableEnumeratorElement", element, rest);
    }

    public static bool operator ==(PureImmutableEnumeratorElement<U> a, PureImmutableEnumeratorElement<U> b)
      => Equality.Operator(a, b);
    public static bool operator !=(PureImmutableEnumeratorElement<U> a, PureImmutableEnumeratorElement<U> b)
      => !(a == b);
    public override bool Equals(object other)
      => Equality.Untyped<PureImmutableEnumeratorElement<U>>(
        this,
        other,
        x => x as PureImmutableEnumeratorElement<U>,
        x => x.hashCode,
        // Two immutable enumerators are equal if and only if
        // they have the same (immutable) state and use the same
        // generator lambda.
        x => x.element,
        x => x.rest);
    public bool Equals(IImmutableEnumeratorElement<U> other)
      => Equality.Equatable<IImmutableEnumeratorElement<U>>(this, other);
    public override int GetHashCode() => hashCode;
    public override string ToString() => "PureImmutableEnumeratorElement";

    public void Dispose() { /* Nothing to do */ }

    public U First { get => element; }

    public IImmutableEnumerator<U> Rest { get => rest; }

    public Option<IImmutableEnumeratorElement<U>> MoveNext()
      => rest.MoveNext();
  }
}