using System;
using System.Collections.Generic;
using System.Linq;
using SearchOption = System.IO.SearchOption;
using Compiler = System.Func<Ast.Expr, string>;
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

  public static void RunTest(string toolchainName, Compiler compile, Exe runner, File source) {
    var destPath = tests_results.Combine(source);
    var sourcePath = tests.Combine(source);
    var expected = sourcePath.DropExtension().Combine(Ext(".o"));
    
    Console.Write($"Running test {source} ({toolchainName}) ");
    
    destPath.DirName().Create();

    CompileToFile(compile, sourcePath, destPath);
    
    var actualStr = runner.Run(destPath);
    var expectedStr = expected.Read();
    if (actualStr != expectedStr) {
      Console.WriteLine("\x1b[1;31mFail\x1b[m");
      throw new Exception($"Test failed {source}: expected {expectedStr} but got {actualStr}.");
    } else {
      Console.WriteLine("\x1b[1;32mOK\x1b[m");
    }
  }

  public static void RunTests() {
    // Circumvent bug with collection initializers, tuples and
    // first-class functions by using repeated .Add()
    // See https://repl.it/@suzannesoy/WarlikeWorstTraining#main.cs
    var compilers = new List<Tuple<string, Compiler, Exe>>()
      .Cons(" js ", Compilers.JS.Compile, Exe("node"))
      .Cons("eval", Evaluator.Evaluate,   Exe("cat"));

    foreach (var t in Dir("Tests/").GetFiles("*.e", SearchOption.AllDirectories)) {
      foreach (var compiler in compilers) {
        RunTest(compiler.Item1, compiler.Item2, compiler.Item3, t);
      }
    }
  }

  public static void Main (string[] args) {
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
  }
}