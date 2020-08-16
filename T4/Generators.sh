#!/bin/sh

# Run with: sh ./T4/Generators.sh

set -e
set -x

mcs -out:T4.exe T4/Generator.cs T4/Generators.cs LexerGenerator.cs AstGenerator.cs T4/T4.compileTime && mono T4.exe
