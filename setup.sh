#!/usr/bin/env bash
# Run once from the WarSim folder:  bash setup.sh
set -e
cd "$(dirname "$0")"

dotnet new sln -n WarSim
dotnet new gitignore
dotnet new xunit -n WarSim.Tests -o tests/WarSim.Tests   # adds the csproj + xunit packages
rm -f tests/WarSim.Tests/UnitTest1.cs
dotnet sln add src/WarSim.Core src/WarSim.App tests/WarSim.Tests
dotnet add tests/WarSim.Tests reference src/WarSim.Core

dotnet build
dotnet test
echo
echo "Try:  dotnet run --project src/WarSim.App -- run --seed 42"
echo "      dotnet run --project src/WarSim.App -- montecarlo --runs 1000"
