using System;

public abstract class UserErrorException : Exception {
  public UserErrorException(string e) : base(e) {}
}

public class ParserErrorException : UserErrorException {
  public ParserErrorException(string e) : base("Parser error: " + e) {}
}

public class ParserExtensionException : UserErrorException {
  public ParserExtensionException(string e) : base("Parser extension error: " + e) {}
}

public class LexerErrorException : UserErrorException {
  public LexerErrorException(string e) : base("Lexer error: " + e) {}
}

public class RuntimeErrorException : UserErrorException {
  public RuntimeErrorException(string e) : base("Runtime error: " + e) {}
}

public class TestFailedException : UserErrorException {
  public TestFailedException(string e) : base("Test failed: " + e) {}
}