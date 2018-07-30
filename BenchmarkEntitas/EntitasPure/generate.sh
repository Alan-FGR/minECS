#!/bin/bash -e
msbuild EntitasPure.csproj
mono ../CodeGenerator/entitas.exe gen
