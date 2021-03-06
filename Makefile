CS := $(shell find . -not \( -path ./.git \) -not \( -name '*Generator.cs' \) -not \( -name '*Generated.cs' \) -name '*.cs')
META := $(shell find ./T4/ -name '*.cs')
GENERATORS := $(shell find . -not \( -path ./.git \) -not \( -path './T4/*' \) -name '*Generator.cs')
GENERATED := $(patsubst %Generator.cs,%Generated.cs,$(GENERATORS))
GENERATEDF := $(patsubst %.cs,%GeneratedF.cs,$(shell git grep -l '\[F\]' | grep '\.cs$$'))

.PHONY: run
run: main.exe Makefile
	MONO_PATH=/usr/lib/mono/4.5/:/usr/lib/mono/4.5/Facades/ mono $<

main.exe: $(CS) $(GENERATED) $(GENERATEDF) Makefile
	@echo 'Compiling…'
	@mcs -debug+ -out:$@ \
   /reference:/usr/lib/mono/4.5/System.Collections.Immutable.dll \
   /reference:/usr/lib/mono/4.5/Facades/netstandard.dll \
   $(filter-out Makefile, $^)

%Generated.cs: .%Generator.exe Makefile
	@echo 'Running code generator…'
	@MONO_PATH=/usr/lib/mono/4.5/:/usr/lib/mono/4.5/Facades/ \
	mono $<

.%Generator.exe: %Generator.cs $(META) Makefile
	@echo 'Compiling code generator…'
	@mcs -out:$@ \
   /reference:/usr/lib/mono/4.5/System.Collections.Immutable.dll \
   /reference:/usr/lib/mono/4.5/Facades/netstandard.dll \
   $(filter-out Makefile, $^)

%GeneratedF.cs: %.cs F.sed Makefile
	@echo 'Running code generator…'
	@sed -n -f F.sed $< > $@

