DESTDIR?=
PREFIX=/usr/local

all:
	mcs Main.cs UnixShell.cs
	echo '#!/bin/sh' > ytm
	echo 'cd $(PWD)' >> ytm
	echo 'mono Main.exe $$@' >> ytm
	chmod +x ytm
	#mono Main.exe

install:
	ln -fs $(PWD)/ytm $(DESTDIR)$(PREFIX)/bin/ytm

clean:
	rm Main.exe*
