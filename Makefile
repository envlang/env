CS := $(shell find . -not \( -path ./.git \) -not \( -name '*Generator.cs' \) -name '*.cs')
GENERATORS := $(shell find . -not \( -path ./.git \) -not \( -path ./T4/Generator.cs \) -name '*Generator.cs')
GENERATED := $(patsubst %Generator.cs,%Generated.cs,$(GENERATORS))

.PHONY: run
run: main.exe
	mono main.exe

main.exe: $(sort $(CS) $(GENERATED))
	mcs -out:$@ $^

%Generated.cs: .%Generator.exe
	mono $<

.%Generator.exe: %Generator.cs
	mcs -out:$@ T4/Generator.cs $<


