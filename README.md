# WarSim

A console-based, grid-driven turn simulation written in C# (.NET). Two teams of units move on a 10x10 grid, capture objective cells, and are limited by terrain. Each run and its events are saved to a MySQL database.

I built this project to practise object-oriented design, rule-based logic, exception handling, SQL and unit testing in C#.

## Features

- Grid map with blocked (terrain) cells and objective cells
- Unit types with different movement limits: `Scout`, `Vehicle`, `Infantry`
- Turn-based simulation: units move toward the nearest objective, and teams earn points by capturing objectives
- Rule checks for grid bounds, blocked cells and maximum step size, with exceptions for invalid moves
- Event log for every turn (moves, captures, blocked moves)
- Results saved to MySQL using parameterised queries
- The simulation still runs if the database is unavailable
- Unit tests with xUnit

> Edit this list so it matches only what you have actually built.

## Sample output

```
Turn 1
. . . . . . . . . .
. S . . . # . . . .
. . . . . . . . . .
...
Red: 0   Blue: 0

...

Final score: Red 3 - Blue 2
```

> Replace this with real output from your program, or add a screenshot.

## Class overview

| Class / type | Purpose |
|---|---|
| `IMovable` | Interface with `Move(int dx, int dy)` |
| `Unit` (abstract) | Base class with name, team, position, health and an abstract `MaxStep` |
| `Scout`, `Vehicle`, `Infantry` | Concrete unit types that override `MaxStep` |
| `Cell` | One grid cell: terrain, objective flag, owner |
| `Grid` | Holds the cells, checks bounds and blocked cells, prints the map |
| `Simulation` | Runs turns, applies rules, tracks scores and events |
| `SimulationEvent` | A record of something that happened in a turn |
| `SimulationRepository` | All MySQL code (save a run, save events, read results) |

## Requirements

- .NET SDK (this project was built with .NET 10; replace with the version in your `.csproj`)
- MySQL Server (optional: the simulation runs without it, but results are not saved)
- NuGet package: `MySqlConnector`

## Setup

1. Clone the repository:
   ```
   git clone <your-repo-url>
   cd WarSim
   ```
2. Create the database and tables:
   ```sql
   CREATE DATABASE warsim;
   USE warsim;
   SOURCE Database/schema.sql;
   ```
3. Set the connection string as an environment variable (do not commit passwords):
   ```
   export WARSIM_CONNECTION="Server=localhost;Database=warsim;User=<user>;Password=<password>;"
   ```

## Run

```
dotnet run --project WarSim
```

## Test

```
dotnet test
```

## Database

Two tables are used: `runs` (one row per simulation) and `events` (many rows per run, linked by `run_id`). See `Database/schema.sql`.

Example queries:

```sql
-- Last 5 runs
SELECT id, started_at, turns, red_score, blue_score
FROM runs
ORDER BY started_at DESC
LIMIT 5;

-- Event counts by type for one run
SELECT e.event_type, COUNT(*) AS total
FROM events e
JOIN runs r ON r.id = e.run_id
WHERE r.id = 1
GROUP BY e.event_type;
```

All inserts and lookups in C# use parameterised queries (`@name` parameters) to avoid SQL injection.

## Project structure

```
WarSim/                          (repo root, push this to GitHub)
├── README.md
├── .gitignore
├── WarSim.sln
├── Database/
│   └── schema.sql
├── WarSim/                      (console app project)
│   ├── WarSim.csproj
│   ├── Program.cs
│   ├── Models/
│   │   ├── Team.cs              enum Team { Red, Blue }
│   │   ├── EventType.cs         enum EventType { Moved, Captured, Blocked, Collision }
│   │   ├── IMovable.cs          interface
│   │   ├── Unit.cs              abstract base class
│   │   ├── Scout.cs
│   │   ├── Vehicle.cs
│   │   ├── Infantry.cs
│   │   └── Cell.cs
│   ├── Core/
│   │   ├── Grid.cs
│   │   ├── Simulation.cs
│   │   ├── SimulationEvent.cs
│   │   └── OutOfBoundsException.cs
│   └── Data/
│       └── SimulationRepository.cs   all MySQL code
└── WarSim.Tests/                (xUnit test project)
    ├── WarSim.Tests.csproj
    ├── GridTests.cs
    └── SimulationTests.csS
```

## What I would improve next

- Smarter movement (pathfinding around blocked cells)
- Configurable grid size, teams and turn count
- More unit types and rules
- Integration tests against a test database

## Author

Nabila Ferdous
B.Sc. in Electrical and Computer Engineering, RUET
