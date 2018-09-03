#!/bin/bash

rm -fr ~/.nuget/packages
rm -fr packages
nuget update -self
nuget locals all -clear
nuget restore
msbuild /p:Configuration=Release /t:Clean,Build
cd QuantoAgent/bin/Release
cd ../../../
