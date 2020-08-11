/*
namespace Immutable {
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class ListI<T> : IEnumerable<T> {
    private readonly Option< (T, ListI<T>) > l;
    public ListI() { this.l = Option.None< (T, ListI<T>) >(); }
    public ListI(T hd, ListI<T> tl) { this.l = (hd, tl).Some(); }
    public ListI<T> Add(T x) => new ListI<T>(x, this);
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    private class Enumerator : IEnumerator<T>, IEnumerator {
      private List<T> l;
      private List<T> next = null;
      private ListI<T> reset;
      private T Current_()
        => l.l.Match<(T, ListI<T>), T>(
            Some: l => l.Item1,
            None: (() => throw new Exception("oops"))
          );
      public T Current { get => Current_(); }
      object IEnumerator.Current { get => Current_(); }
      public Enumerator(ListI<T> l) { this.l = l; }
      public bool MoveNext() {
        if (first) { this.first = false; } else { this.l = l.Item2; }
        l.l.Match<(T, ListI<T>), bool>(
          Some: l => true,
          None: (() => false)
        );
      }
      public void Dispose() {}
      public void Reset() { this.l = reset; }
    }
  }
}
*/