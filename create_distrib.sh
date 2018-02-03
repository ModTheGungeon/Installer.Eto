#!/bin/sh

mkdir -p distrib/bin
msbuild /p:Configuration=Release /p:Platform=x86
cp -r MTGInstallerEto/bin/Release/* distrib/bin
cd distrib
zip -r MTGInstaller.zip *
