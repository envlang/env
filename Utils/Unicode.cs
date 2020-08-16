using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public struct GraphemeCluster {
  public readonly bool endOfFile;
  public readonly string str;
  public readonly IEnumerable<string> codePoints;
  public GraphemeCluster(bool endOfFile, string str, IEnumerable<string> codePoints) {
    this.endOfFile = endOfFile;
    this.str = str;
    this.codePoints = codePoints;
  }
}

public static class UnicodeExtensionMethods {
  public static IEnumerable<string> SplitOnSurrogatePairs(this IEnumerable<char> s) {
    var e = s.GetEnumerator();
    while (e.MoveNext()) {
      var firstOfPossiblePair = e.Current;
      if (firstOfPossiblePair.IsHighSurrogate()) {
        e.MoveNext();
        if (e.Current.IsLowSurrogate()) {
          yield return firstOfPossiblePair.ToString()
                     + e.Current.ToString();
        } else {
          throw new ArgumentException("This UTF-16 string seems malformed: found a high surrogate at the end of the input.");
        }
      } else {
        yield return firstOfPossiblePair.ToString();
      }
    }
  }

  public static int ToUtf32(this string s, int pos)
    => Char.ConvertToUtf32(s, pos);

  public static IEnumerable<GraphemeCluster> TextElements(this string s) {
    // in: "1\u22152e\u0301\u0327a"
    // out: [["1"], ["\u2215"], ["2"], [e, "\u0301", "\u0327"], "a"]]
    // i.e. "1∕2ȩ́a"
    // becomes [["1"], ["∕"], ["2"], ["e", "◌́", "◌̧̧"̧], ["a"]]
    // TODO: also groups flag emojis based on unicode "tags" as a single element
    var e = StringInfo.GetTextElementEnumerator(s);
    var alreadyMoved = false;
    while (alreadyMoved || e.MoveNext()) {
      alreadyMoved = false;
      // TODO: check whether UTF-16 allows for different
      // encodings for the same code point and if so how
      // to compare them correctly.
      var te = e.Current.ToString();
      var wavingBlackFlag = 0x1F3F4;
      // TODO: check the role of "begin" for tag sequences.
      var begin           = 0xE0001;
      // All the characters between sp and cancelTag are valid tag characters
      var sp              = 0xE0020;
      var cancelTag       = 0xE007F;
      var first = te.ToUtf32(0);
      // TODO: te.length is hardcoded as 2 because the tag
      // code points all require a surrogate pair (i.e. don't
      // fit in a single UTF-16 element).
      if (te.Length == 2 && first == wavingBlackFlag || first == begin) {
        while (e.MoveNext()) {
          var te2 = e.Current.ToString();
          var first2 = te2.ToUtf32(0);
          if (te2.Length == 2 && first2 >= sp && first2 <= cancelTag) {
            te += te2;
            if (first2 == cancelTag) {
              alreadyMoved = false;
              break;
            }
          } else {
            alreadyMoved = true;
          }
        }
      }
      yield return new GraphemeCluster(
        false,
        te,
        te.SplitOnSurrogatePairs()
      );
    }
    yield return new GraphemeCluster(
      true,
      "",
      Enumerable.Empty<string>()
    );
  }

  public static UnicodeCategory UnicodeCategory(this string s, int startIndex)
    => Char.GetUnicodeCategory(s, startIndex);

  public static string Description(this GraphemeCluster gc) {
    if (gc.endOfFile) {
      return "end of file";
    } else if (gc.str == "\"") {
      return "'\"'";
    } else {
      return $"\"{gc.str}\"";
    }
  }
}