using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Immutable {
  public class PureImmutableEnumerator<T, U> : IImmutableEnumerator<U> {
    private T state;
    private IEqF<T, Option<Tuple<U, IImmutableEnumerator<U>>>> generator;
    private int hashCode;

    public PureImmutableEnumerator(T state, IEqF<T, Option<Tuple<U, IImmutableEnumerator<U>>>> generator) {
      this.state = state;
      this.generator = generator;
      this.hashCode = Equality.HashCode("PureImmutableEnumerator", state, generator);
    }

    public static bool operator ==(PureImmutableEnumerator<T, U> a, PureImmutableEnumerator<T, U> b)
      => Equality.Operator(a, b);
    public static bool operator !=(PureImmutableEnumerator<T, U> a, PureImmutableEnumerator<T, U> b)
      => !(a == b);
    public override bool Equals(object other)
      => Equality.Untyped(
        this,
        other,
        x => x as PureImmutableEnumerator<T, U>,
        x => x.hashCode,
        // Two immutable enumerators are equal if and only if
        // they have the same (immutable) state and use the same
        // generator lambda.
        x => x.state,
        x => x.generator);
    public bool Equals(IImmutableEnumerator<U> other)
      => Equality.Equatable<IImmutableEnumerator<U>>(this, other);
    public override int GetHashCode() => hashCode;
    public override string ToString() => "ImmutableEnumerator";

    public IEnumerator<U> GetEnumerator()
      => this.ToIEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
      => this.ToIEnumerable().GetEnumerator();

    void IDisposable.Dispose() { /* Nothing to do */ }

    public Option<IImmutableEnumeratorElement<U>> MoveNext()
      => generator.F(state).IfSome((first, rest) =>
           new PureImmutableEnumeratorElement<U>(first, rest));
  }
}