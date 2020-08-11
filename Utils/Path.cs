using System;
using System.Collections.Generic;
using System.Linq;
using IO = System.IO;

public abstract class Path {
  public readonly string str;
  
  protected Path(string str) { this.str = str; }
  
  public static implicit operator string(Path p) => p.str;
  
  public override string ToString() => str;
}

public class File : Path { public File(string str) : base(str) {} }
public class Ext : Path { public Ext(string str) : base(str) {} }
public class Exe : Path { public Exe(string str) : base(str) {} }

public class Dir : Path { public Dir(string str) : base(str) {}
  public static Dir GetCurrentDirectory()
    => IO.Directory.GetCurrentDirectory().Dir();
}

public static class ExtensionMethods {
  public static File File(this string str) => new File(str);
  public static Ext  Ext (this string str) => new Ext (str);
  public static Dir  Dir (this string str) => new Dir (str);
  public static Exe  Exe (this string str) => new Exe (str);

  public static File Write(this File f, string s) {
    IO.File.WriteAllText(f, s);
    return f;
  }

  public static string Read(this File f) {
    return IO.File.ReadAllText(f);
  }

  public static string WriteTo(this string s, File f) {
    IO.File.WriteAllText(f, s);
    return s;
  }

  public static Dir Create(this Dir d) {
    IO.Directory.CreateDirectory(d);
    return d;
  }

  public static IEnumerable<File> GetFiles(this Dir d, string pattern, IO.SearchOption searchOption) {
    var prefixLen = d.ToString().Length;
    if (!(   d.ToString().EndsWith("" + IO.Path.DirectorySeparatorChar)
          || d.ToString().EndsWith("" + IO.Path.AltDirectorySeparatorChar))) {
      prefixLen++;
    }
    // TODO: test if it also returns dirs.
    return IO.Directory
      .GetFiles(d, pattern, searchOption)
      .Select(x => x.Substring(prefixLen))
      .Select(x => x.File());
  }

  public static File Combine(this Dir a, File b)
    => IO.Path.Combine(a, b).File();

  public static File Combine(this File a, Ext b)
    => new File(a + b);

  public static Dir Combine(this Dir a, Dir b)
    => IO.Path.Combine(a, b).Dir();

  public static Dir DirName(this Path p)
    => IO.Path.GetDirectoryName(p).Dir();

  public static File DropExtension(this File f)
    => f.DirName().Combine(IO.Path.GetFileNameWithoutExtension(f).File());

  public static string Run(this Exe e, string args) {
    var p = System.Diagnostics.Process.Start(
      new System.Diagnostics.ProcessStartInfo {
        FileName = e,
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true
      }
    );
    var stdout = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
    return stdout;
  }
}
