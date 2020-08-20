CS := $(shell find . -not \( -path ./.git \) -not \( -name '*Generator.cs' \) -not \( -name '*Generated.cs' \) -name '*.cs')
META := $(shell find ./T4/ -name '*.cs')
GENERATORS := $(shell find . -not \( -path ./.git \) -not \( -path './T4/*' \) -name '*Generator.cs')
GENERATED := $(patsubst %Generator.cs,%Generated.cs,$(GENERATORS))

.PHONY: run
run: main.exe Makefile
	MONO_PATH=/usr/lib/mono/4.5/:/usr/lib/mono/4.5/Facades/ mono $<

main.exe: $(CS) $(GENERATED) Makefile
	mcs -out:$@ \
   /reference:/usr/lib/mono/fsharp/FSharp.Core.dll \
   /reference:/usr/lib/mono/4.5/System.Collections.Immutable.dll \
   /reference:/usr/lib/mono/4.5/Facades/netstandard.dll \
   $(filter-out Makefile, $^)

%Generated.cs: .%Generator.exe Makefile
	MONO_PATH=/usr/lib/mono/4.5/:/usr/lib/mono/4.5/Facades/ mono $(filter-out Makefile, $<)

.%Generator.exe: %Generator.cs $(META) Makefile
	mcs -out:$@ \
   /reference:/usr/lib/mono/4.5/System.Collections.Immutable.dll \
   /reference:/usr/lib/mono/4.5/Facades/netstandard.dll \
   $(filter-out Makefile, $^)


