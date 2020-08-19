CS := $(shell find . -not \( -path ./.git \) -not \( -name '*Generator.cs' \) -name '*.cs')
GENERATORS := $(shell find . -not \( -path ./.git \) -not \( -path ./T4/Generator.cs \) -name '*Generator.cs')
GENERATED := $(patsubst %Generator.cs,%Generated.cs,$(GENERATORS))

.PHONY: run
run: main.exe
	mono main.exe

main.exe: $(sort $(CS) $(GENERATED))
	mcs -out:$@ /reference:/usr/lib/mono/fsharp/FSharp.Core.dll $^

%Generated.cs: .%Generator.exe
	mono $<

.%Generator.exe: %Generator.cs T4/Generator.cs
	mcs -out:$@ $^


