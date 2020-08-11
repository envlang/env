public static class Global {
  public static File File(string str) => new File(str);
  public static Ext  Ext (string str) => new Ext (str);
  public static Dir  Dir (string str) => new Dir (str);
  public static Exe  Exe (string str) => new Exe (str);
  
  public static void Log (string str) => System.Console.WriteLine(str);
}