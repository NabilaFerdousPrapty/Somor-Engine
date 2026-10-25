# Somor Engine

A turn-based tactical war game **simulation engine** written in C# / .NET 10.
Two forces fight on a grid map with terrain. Each side is controlled by a swappable AI strategy,
every battle is reproducible from a seed, and a Monte Carlo runner repeats a scenario hundreds of
times to answer questions like *"which strategy wins more often on this terrain?"*

You can use it three ways: a **console app**, an **interactive web page** where you command the Blue
force and change its orders turn by turn, and a standalone **HTML report**. Results can be saved
to **MySQL**.

> Everything in this project is fictional: generic units on a made-up map. It does not model real
> equipment or real operations.

**Live demo:** `https://somor-engine.onrender.com` (the first load may take up to a minute on the free host)

![WarSim web page](docs/screenshot.png)

---

## What it does

- Builds a battle scenario (map, terrain, units, starting positions).
- Runs the battle **turn by turn**: each side decides, the engine resolves movement and combat, and every event is logged.
- Uses a **seeded random generator**, so the same seed always produces exactly the same battle.
- Runs the same scenario many times (**Monte Carlo**) and reports win rates, average casualties and average battle length.
- Lets a person **take command of Blue** and switch between Aggressive, Defensive and Balanced orders at any turn.
- Saves runs, events and Monte Carlo results to **MySQL** (or falls back to memory if no database is available).

## Features

| Area | Feature |
|---|---|
| Simulation | Grid map with Plains, Forest, Hills and Water; terrain affects movement cost and defence |
| Units | Infantry, Armor and Recon (abstract `Unit` base class, one subclass each) |
| AI | Aggressive, Defensive and Balanced strategies behind an `IAiStrategy` interface |
| Randomness | `IRandomProvider` with a seeded implementation: reproducible battles |
| Analysis | Monte Carlo runner, win rates, casualties, battle length (average and standard deviation), strategy comparison matrix |
| Console app | `run`, `play`, `montecarlo`, `compare`, `report`, `list` commands with a colored animated replay |
| Web page | Animated map, order buttons, play/pause/step controls, strategy comparison table |
| Database | MySQL storage with plain ADO.NET and parameterised queries, in-memory fallback |
| Tests | 14 xUnit tests covering movement, combat, determinism, AI and Monte Carlo |

---

## How the simulation works

### Map and terrain

| Terrain | Movement cost | Defence bonus |
|---|---|---|
| Plains | 1 | 0% |
| Forest | 2 | +25% |
| Hills | 2 | +40% |
| Water | impassable | n/a |

The built-in scenario "Open Field" is a 6x5 map with three units per side (Infantry, Armor, Recon).

### Units

| Unit | Health | Move | Range | Attack | Defense | Idea |
|---|---|---|---|---|---|---|
| Infantry | 100 | 3 | 1 | 20 | 10 | Slow, good defence, gets 50% extra benefit from forest and hills |
| Armor | 120 | 5 | 2 | 35 | 8 | Fast, strong attack, lower defence |
| Recon | 60 | 7 | 2 | 12 | 4 | Very fast, weak attack |

Distance is measured in grid steps (Manhattan distance).

### One turn

1. The side that moves first alternates every turn, so neither side always has the initiative.
2. For each living unit (in a fixed order by id, so results are deterministic) the side's strategy decides: where to move, and whether to attack.
3. **Movement:** the unit spends movement points stepping toward its target. Each step must reduce the distance to the target (greedy movement); terrain costs apply; water and occupied tiles block.
4. **Combat:** if an enemy is within attack range, the unit attacks the weakest one in range.
5. Dead units are removed and every action is recorded as a battle event.
6. **Victory check:** a side has no units left, or the turn limit (30) is reached. At the limit, the side with more total health wins, or it is a draw.

### Combat formula (simplified)

```
raw     = attacker.Attack * (1 + random in [-0.2, +0.2])
blocked = defender.Defense * (1 + terrain defence bonus)
damage  = max(1, round(raw - blocked))
```

The random factor comes from the seeded `IRandomProvider`. This is a simple model that has not been
calibrated against any real data.

### AI strategies

| Strategy | Behaviour |
|---|---|
| Aggressive | Every unit moves toward the nearest enemy and attacks when in range |
| Defensive | Units move to nearby forest or hills, hold position there, and attack only enemies that come into range |
| Balanced | Infantry act defensively; Armor and Recon act aggressively |

### Monte Carlo

`MonteCarloRunner` runs the scenario N times with seeds `baseSeed + i`. Each run has its own
state and random generator, so runs are independent. It reports wins per side and draws, win
rates, average casualties, and the average and standard deviation of battle length.

---

## Architecture

```
WarSim.Core         domain model, rules, engine, AI, Monte Carlo (no UI, no database)
WarSim.Persistence  MySQL repository + in-memory fallback
WarSim.Console      command-line app (depends on Core and Persistence)
WarSim.Web          ASP.NET Core minimal API + one HTML page (depends on Core)
WarSim.Tests        xUnit tests (depends on Core)
```

The main design point is that **Core has no database or UI references**, so the same engine sits
behind the console app, the web API and the tests without changes.

### Design patterns

| Pattern | Where | Why |
|---|---|---|
| Strategy | `IAiStrategy` with Aggressive, Defensive, Balanced | Swap behaviour without changing the engine |
| Factory | `UnitFactory`, `StrategyFactory` | Create units and strategies from names |
| Wrapper | `SwitchableStrategy` | Lets the player change Blue's strategy between turns; the engine still just sees an `IAiStrategy` |
| Dependency inversion | `IRandomProvider`, `IRunRepository` | Reproducible randomness; MySQL can be replaced by the in-memory version |
| Inheritance / polymorphism | `Unit` and its subclasses | Each unit type has its own stats and rules |

### Project structure

```
WarSim/
├── README.md
├── Dockerfile
├── .dockerignore
├── WarSim.slnx
├── database/
│   └── schema.sql
├── src/
│   ├── WarSim.Core/
│   │   ├── Domain/       Position, TerrainType, Tile, GameMap, Side, Unit, Infantry, Armor, Recon, UnitFactory, Scenario
│   │   ├── AI/           IAiStrategy, Aggressive/Defensive/Balanced strategies, SwitchableStrategy, StrategyFactory
│   │   ├── Engine/       SimulationEngine, CombatResolver, MovementService, BattleEvent, BattleResult, TurnSnapshot
│   │   ├── Analysis/     MonteCarloRunner, MonteCarloSummary
│   │   └── Common/       IRandomProvider, SeededRandomProvider
│   ├── WarSim.Persistence/   IRunRepository, MySqlRunRepository, InMemoryRunRepository
│   ├── WarSim.Console/       Program.cs, ConsoleRenderer, PlayCommand, CompareCommand, HtmlReport
│   └── WarSim.Web/           Program.cs (API), wwwroot/index.html
└── tests/
    └── WarSim.Tests/         Movement, CombatResolver, Determinism, AiStrategy, MonteCarlo tests
```

---

## Setup

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (check with `dotnet --version`)
- Git
- MySQL (optional: only needed to save results; the app works without it)
- Docker (optional: only for testing the container locally)

### Get the code, build and test

```bash
git clone https://github.com/YOUR_USERNAME/WarSim.git
cd WarSim
dotnet build
dotnet test
```

### Run the console app

```bash
# Watch one battle as a colored animated replay (keys: Space pause, arrows step, +/- speed, R restart, Q quit)
dotnet run --project src/WarSim.Console -- run --seed 42 --blue Defensive --red Aggressive

# Interactive mode: you command Blue and switch orders each turn (A / D / B, Enter = next turn)
dotnet run --project src/WarSim.Console -- play --seed 42

# Run 1,000 simulations and print statistics
dotnet run --project src/WarSim.Console -- montecarlo --runs 1000 --blue Defensive --red Aggressive

# Compare all strategy pairings in one table
dotnet run --project src/WarSim.Console -- compare --runs 1000

# Write a self-contained HTML report (replay + charts) and open it in the browser
dotnet run --project src/WarSim.Console -- report --seed 42 --open

# List the last 10 saved runs
dotnet run --project src/WarSim.Console -- list
```

Useful options: `--seed N` (or `--seed random`), `--delay MS` (animation speed, `0` prints every frame),
`--log` (plain text event log instead of the animation).

### Run the web page

```bash
dotnet run --project src/WarSim.Web
```

Open **http://localhost:5080**. On the page you can:

- choose Blue's orders (Aggressive, Defensive, Balanced) at any turn: buttons, or keys **A / D / B**
- play, pause, step forward and back, scrub the slider, change speed (**Space**, **arrow keys**)
- pick the enemy strategy and a seed, or press **Random seed**
- run the strategy comparison table and click a cell to watch that matchup

The server exposes a small API: `GET /health`, `GET /api/battle`, `GET /api/montecarlo` and
`GET /api/compare`. The same seed and the same order changes always give the same battle, so the page
can safely ask the server to replay a battle after you change your orders.

### Save results to MySQL (optional)

```bash
# 1. Create the database and tables
mysql -u root -p < database/schema.sql

# 2. Tell the app how to connect (keep the password out of the code and out of Git)
export WARSIM_CONNECTION="Server=localhost;Database=warsim;User=root;Password=YOUR_PASSWORD;"

# 3. Run something and look at the saved data
dotnet run --project src/WarSim.Console -- run --seed 42
dotnet run --project src/WarSim.Console -- list
```

If `WARSIM_CONNECTION` is not set, or MySQL is not reachable, the app prints one clear message and
falls back to in-memory storage, so it always still runs. (With the in-memory fallback every run is
numbered `#1`, because nothing is kept after the program exits.)

To keep the setting between terminal sessions, add the `export` line to `~/.zshrc`.

**Database tables:** `simulation_runs` (one row per battle), `battle_events` (every move, attack and
death), `monte_carlo_batches` (one row per Monte Carlo run). See `database/schema.sql`.

Example queries:

```sql
-- Last 5 runs
SELECT id, scenario_name, winner, turns_played, started_at
FROM simulation_runs ORDER BY started_at DESC LIMIT 5;

-- Win count per winner for each strategy pairing
SELECT blue_strategy, red_strategy, winner, COUNT(*) AS total
FROM simulation_runs
GROUP BY blue_strategy, red_strategy, winner;

-- Event counts by type for one run
SELECT event_type, COUNT(*) AS total
FROM battle_events
WHERE run_id = 1
GROUP BY event_type;

-- Longest battles
SELECT id, scenario_name, turns_played
FROM simulation_runs ORDER BY turns_played DESC LIMIT 5;
```

All SQL in the app uses parameters (`@name`) and `using` blocks for connections.

---

## Sample output

Monte Carlo, 1,000 battles, Blue Defensive vs Red Aggressive (base seed 1):

```
Scenario: Open Field   Runs: 1000   Base seed: 1

Side   Strategy       Wins   Win rate   Avg casualties
Blue   Defensive       857      85.7%              1.3
Red    Aggressive      143      14.3%              1.9
Draw                     0       0.0%

Average battle length: 27.5 turns (std dev 6.2)
```

Start of a single battle (seed 42):

```
--- Turn 1 ---
  Blue Infantry 1 moved (0,1) -> (2,1)
  Blue Armor 2 moved (1,2) -> (2,2)
  Blue Armor 2 hit Red Armor 5 for 29 (health 91)
  ...
```

---

## Deployment

### Self-contained program (no .NET needed to run it)

```bash
# Apple-silicon Mac (use osx-x64 for Intel, win-x64 for Windows, linux-x64 for Linux)
dotnet publish src/WarSim.Console -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o publish/osx-arm64
```

Zip the output folder and attach it to a GitHub Release (`git tag v1.0.0`, `git push origin v1.0.0`,
then **Releases** → **Draft a new release** on GitHub).

### Web page on Render (Docker)

1. Push the repo to GitHub. The `Dockerfile` must be in the repo root, next to `src`.
2. On [render.com](https://render.com): **New** → **Web Service** → pick the repo.
3. Language **Docker**, instance type **Free**, leave **Root Directory** empty, set **Health Check Path** to `/health`.
4. Click **Create Web Service**. When the build finishes you get a public link.

The live site uses no database. The app reads the port from the `PORT` environment variable, which hosts set automatically.
Free services usually go to sleep when idle, so the first request after a break can take up to a minute.

Optional local test of the container: `docker build -t warsim .` then
`docker run --rm -p 8080:8080 warsim` and open http://localhost:8080/health.

---

## Testing

```bash
dotnet test
```

| Area | What is tested |
|---|---|
| Movement | cannot enter water; cannot leave the map; movement points are respected; forest costs more than plains |
| Combat | out-of-range attack is rejected; terrain defence reduces damage; damage is at least 1 |
| Determinism | same seed gives an identical event log; different seeds give different logs |
| AI | aggressive moves toward the nearest enemy; defensive holds good terrain; defensive seeks good terrain |
| Monte Carlo | wins plus draws equal the number of runs; results are reproducible for a given base seed |

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `command not found: dotnet` | Install the .NET 10 SDK and open a new terminal |
| Every save says `run #1` | MySQL is not connected, so the in-memory fallback is used. Check `echo $WARSIM_CONNECTION` and that MySQL is running |
| `play` or `run` controls do not respond | Run from a real terminal window (Terminal app or the VS Code Terminal tab), not the VS Code Debug Console |
| Web page shows raw JSON | You opened an `/api/...` link. Open the plain address (`http://localhost:5080/`) |
| Render says it cannot find the Dockerfile | Make sure `Dockerfile` is committed in the repo root and Root Directory is empty |

---

## Limitations

- **Simplified combat model.** The formula is a simple model and has not been calibrated against any real data. No real military data is used.
- **Greedy movement.** Units only take steps that reduce distance to their target, so they can get stuck behind water or other blockers. Some battles stall and end at the turn limit (average battle length is about 27 of 30 turns), where the winner is decided by remaining health.
- **One built-in map.** Scenarios are defined in code.
- **The live site is stateless.** It does not save anything and uses no database.

## What I would add next

- A* pathfinding so units can route around obstacles
- Fog of war and line of sight
- JSON scenario files and more maps
- More unit types and a richer strategy set
- Parallel Monte Carlo (`Parallel.For`) and batched database writes
- CSV export and GitHub Actions CI with a build badge
- Per-unit orders for the player

---

## What I learned / talking points

- **Why separate Core from Persistence and the UI?** The engine has no framework dependencies, so it is easy to test and can be reused behind a console app or a web API.
- **Why inject `IRandomProvider`?** Reproducible runs, deterministic tests and fair Monte Carlo comparisons.
- **Where is polymorphism used?** The `Unit` subclasses and the `IAiStrategy` implementations.
- **How would it scale?** Run Monte Carlo in parallel, batch database writes, put an API in front, and use proper pathfinding.
- **Trade-offs:** the grid is a simplification, movement is greedy rather than optimal, and the combat formula is not validated.