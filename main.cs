using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SearchOption = System.IO.SearchOption;
using Compiler = System.Func<Ast.AstNode, string>;
using static Global;

public static class MainClass {
  public static readonly Dir tests = Dir("Tests/");
  public static readonly Dir tests_results = Dir("tests_results/");

  public static void CompileToFile (Compiler compile, File source, File dest) {
    source
      .Read()
      .Pipe(Parser.Parse)
      .Pipe(compile)
      .Pipe(c => dest.Write(c));
  }

  public static bool RunTest(string toolchainName, Compiler compile, Exe runner, File source) {
    var destPath = tests_results.Combine(source);
    var sourcePath = tests.Combine(source);
    var expected = sourcePath.DropExtension().Combine(Ext(".o"));

    Console.Write($"\x1b[KRunning test {source} ({toolchainName}) ");
    
    destPath.DirName().Create();

    UserErrorException exception = null;
    try {
      CompileToFile(compile, sourcePath, destPath);
    } catch (UserErrorException e) {
      exception = e;
    }

    if (exception != null) {
      Console.WriteLine("");
      Console.WriteLine("\x1b[1;31mFail\x1b[m");
      Console.WriteLine($"\x1b[1;33m{exception.Message}\x1b[m\n");
      return false;
    } else {
      var actualStr = runner.Run(destPath);
      var expectedStr = expected.Read();
      if (actualStr != expectedStr) {
        Console.WriteLine("\x1b[1;31mFail\x1b[m");
        Console.WriteLine($"\x1b[1;33m{source}: expected {expectedStr} but got {actualStr}.\x1b[m\n");
        return false;
      } else {
        Console.Write("\x1b[1;32mOK\x1b[m\n"); // \r at the end for quiet
        return true;
      }
    }
  }

  public static void RunTests() {
    // Circumvent bug with collection initializers, tuples and
    // first-class functions by using repeated .Add()
    // See https://repl.it/@suzannesoy/WarlikeWorstTraining#main.cs
    var compilers = ImmutableList<Tuple<string, Compiler, Exe>>.Empty
      .Add(" js ", Compilers.JS.Compile, Exe("node"))
      .Add("eval", Evaluator.Evaluate,   Exe("cat"));

    var total = 0;
    var passed = 0;
    var failed = 0;
    foreach (var t in Dir("Tests/").GetFiles("*.e", SearchOption.AllDirectories).OrderBy(f => f.ToString())) {
      foreach (var compiler in compilers) {
        if (RunTest(compiler.Item1, compiler.Item2, compiler.Item3, t)) {
          passed++;
        } else {
          failed++;
        }
        total++;
      }
    }
    Console.WriteLine($"\x1b[K{passed}/{total} tests passed, {failed} failed.");
    if (failed != 0) {
      Environment.Exit(1);
    }
  }

  public static void Main (string[] args) {
    try {
      if (args.Length != 1) {
        Console.WriteLine("Usage: mono main.exe path/to/file.e");
        Console.WriteLine("");
        Console.WriteLine("Language syntax:");
        Console.WriteLine("");
        Console.WriteLine("  Expression =");
        Console.WriteLine("    Int");
        Console.WriteLine("  | String");
        Console.WriteLine("  | Variable");
        Console.WriteLine("  | Pattern \"->\" Expression");
        Console.WriteLine("");
        Console.WriteLine("I'll run the tests for you in the meanwhile.");
        Console.WriteLine("");
        RunTests();
      } else {
        var source = args[0].File();
        var destPrefix = source.DropExtension();
        CompileToFile(Compilers.JS.Compile, source, destPrefix.Combine(Ext(".js")));
        CompileToFile(Evaluator.Evaluate,   source, destPrefix.Combine(Ext(".txt")));
        Console.Write(destPrefix.Combine(Ext(".txt")).Read());
      }
    } catch (UserErrorException e) {
      Console.WriteLine("");
      Console.WriteLine($"\x1b[1;33m{e.Message}\x1b[m\n");
      Environment.Exit(1);
    }
  }
}