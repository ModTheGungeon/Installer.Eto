#!/usr/bin/sh

if ! type mono &> /dev/null; then
	echo "ERROR: Missing Mono! Please install it: http://www.mono-project.com/" 1>&2
	exit 1
fi

cd bin
mono MTGInstallerEto.exe
