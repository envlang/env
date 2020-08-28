using System;

namespace Immutable {
  public sealed class Unit : IEquatable<Unit> {
    public static readonly Unit unit = new Unit();
    private Unit() {}
    public static bool operator ==(Unit a, Unit b)
      => Equality.Operator(a, b);
    public static bool operator !=(Unit a, Unit b)
      => !(a == b);
    public override bool Equals(object other)
      => Equality.Untyped<Unit>(this, other, x => x as Unit, x => x.hashCode);
    public bool Equals(Unit other)
      => Equality.Equatable<Unit>(this, other);
    private int hashCode = HashCode.Combine("Unit");
    public override int GetHashCode()
      => hashCode;
    public override string ToString() => "Unit";
  }
}