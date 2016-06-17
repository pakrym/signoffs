#!/bin/bash
FXVER=1.0.0
FXNAME=CustomPlatform
FXOUT=output/$FXNAME/$FXVER
FXPKG=$FXNAME.$FXVER.nupkg
FXCOPYTO=$1
echo "===== Cleanup"
rm -vrf output
rm -v ../../local/$FXPKG
if [[ $FXCOPYTO ]]; then
	rm -rfv "$FXCOPYTO/$FXNAME"
fi
rm -vrf ~/.nuget/packages/$FXNAME


echo "===== Building framework"
dotnet restore
dotnet publish -o $FXOUT
rm -v $FXOUT/$FXNAME.exe
rm -v $FXOUT/$FXNAME.dll
rm -v $FXOUT/$FXNAME.pdb
rm -v $FXOUT/$FXNAME.runtimeconfig.*
FXDEPS=$FXOUT/$FXNAME.deps.json

mv $FXDEPS $FXDEPS.old
head -n -1 $FXDEPS.old > $FXDEPS
cat Fallbacks.txt >> $FXDEPS
echo "}" >> $FXDEPS
rm $FXDEPS.old

echo "===== Building packages"
dotnet pack -o output

echo "===== Deploying things"
cp -v output/$FXPKG ../../local

if [[ $FXCOPYTO ]]; then
	cp -rfv output/$FXNAME "$FXCOPYTO"
fi
