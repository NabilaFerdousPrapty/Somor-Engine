# WarSim

Turn-based tactical war game simulation engine in C# / .NET with a console front end.
Two forces fight on a grid map with terrain; each side is driven by a swappable AI strategy.
A seeded random generator makes every battle reproducible, and a Monte Carlo runner repeats
a scenario many times to compare strategies.

## Run
```
bash setup.sh        # first time only: creates the solution, builds, runs tests
dotnet run --project src/WarSim.Console -- run --seed 42 --blue Defensive --red Aggressive
dotnet run --project src/WarSim.Console -- run --seed 42 --blue Defensive --red Aggressive
dotnet run --project src/WarSim.Console -- montecarlo --runs 1000 --blue Defensive --red Aggressive
dotnet test
```

(More sections to come: sample output, MySQL, architecture, limitations.)
