#!/usr/bin/sh

mkdir -p distrib/bin
msbuild /p:Configuration=Release
cp -r MTGInstallerEto/bin/Release/* distrib/bin
cd distrib
zip -r MTGInstaller.zip *
