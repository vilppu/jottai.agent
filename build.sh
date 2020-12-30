#!/bin/bash
set -ev
dotnet restore ./Tests
dotnet test ./Tests/Tests.fsproj -c Release
dotnet publish ./HttpApi -c Release -r debian.8-x64 -o Published --no-build
zip -r jottai.agent.zip ./Published
