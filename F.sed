#!/usr/bin/env sed -nf

/^[^ ].*/p

/\[F\]/,/^ *)/{
  s/\( *\)private partial class \(.*\) {/\1\2/;
  t classHeader;
  b notClassHeader;
  :classHeader
  h;
  s/\( *\)\(.*\)/\1private partial class \2 : IEqF</p;
  b next;
  :notClassHeader
  /\[F\]/! {
    s/^ *)/&/;
    t end;
    s/^ *public \(.*\) F(.*$/\1/;
    t skip;
    s/ *\([^ ].*\) [^ ][^ ]*$/    \1,/p;
    s/^/    /;
    H;
    b next;
    :skip H;
    b next;
    :end
    x;

#    s/\( *\)\([^\n<]*\)\([^\n]*\)\n\([^\n]*\)\n\(.*\)$/\1  \4\n  > {\n\1  private \2() {}\n\1  public static readonly \2\3 Eq = new \2\3();\n\1  public static bool operator ==(\2\3 a, \2\3 b)\n\1    => Equality.Operator(a, b);\n\1  public static bool operator !=(\2\3 a, \2\3 b)\n\1    => !(a == b);\n\1  public override bool Equals(object other)\n\1    => Equality.Untyped<\2\3>(\n\1      this,\n\1      other,\n\1      x => x as \2\3,\n\1      x => x.hashCode);\n\1  public bool Equals(IEqF<\n\5\n\1      \4\n\1    > other)\n\1    => Equality.Equatable<IEqF<\n\5\n\1      \4\n\1    >>(this, other);\n\1  private int hashCode = HashCode.Combine("\2\3");\n\1  public override int GetHashCode() => hashCode;\n\1  public override string ToString() => "Equatable function \2\3()";\n\1}\n/;

    s/\( *\)\([^\n<]*\)\([^\n]*\)\n\([^\n]*\)\n\(.*\)$/\1  \4\n  >, IEquatable<\2\3> {\n\1  private \2() {}\n\1  public static readonly \2\3 Eq = new \2\3();\n\1  public static bool operator ==(\2\3 a, \2\3 b)\n\1    => Equality.Operator(a, b);\n\1  public static bool operator !=(\2\3 a, \2\3 b)\n\1    => !(a == b);\n\1  public override bool Equals(object other)\n\1    => Equality.Untyped<\2\3>(\n\1      this,\n\1      other,\n\1      x => x as \2\3,\n\1      x => x.hashCode);\n\1  public bool Equals(\2\3 other)\n\1    => Equality.Equatable<\2\3>(this, other);\n\1  private int hashCode = HashCode.Combine("\2\3");\n\1  public override int GetHashCode() => hashCode;\n\1  public override string ToString() => "Equatable function \2\3()";\n\1}\n/;

    p;
    # Clear hold space
    s/.*//;
    h;
    :next
  }
}