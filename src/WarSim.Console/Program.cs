using WarSim.App;
using WarSim.Core.AI;
using WarSim.Core.Analysis;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;
using WarSim.Persistence;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintUsage();
    return 0;
}

string command = args[0].ToLowerInvariant();
var options = ParseOptions(args.Skip(1).ToArray());

try
{
    var scenario = Scenario.OpenField();
    var blue = StrategyFactory.Create(GetString("blue", "Defensive"));
    var red = StrategyFactory.Create(GetString("red", "Aggressive"));

    switch (command)
    {
        case "run":
        {
            int seed = GetSeed("seed", 42);
            int delay = GetInt("delay", 600);
            bool plainLog = options.ContainsKey("log");
            var repo = CreateRepository();
            var startedAt = DateTime.Now;

            var engine = new SimulationEngine(blue, red, new SeededRandomProvider(seed));
            var result = engine.Run(scenario);

            if (plainLog) ConsoleRenderer.PrintBattle(result, seed);
            else ConsoleRenderer.Replay(scenario, result, delay);

            Console.WriteLine($"Seed: {seed}");
            TrySave(repo, () =>
            {
                int id = repo.SaveRun(result, seed, startedAt);
                Console.WriteLine($"Saved to {repo.Name} as run #{id}.");
            });
            return 0;
        }
        case "montecarlo":
        {
            int runs = GetInt("runs", 1000);
            int seed = GetInt("seed", 1);
            var repo = CreateRepository();

            var summary = new MonteCarloRunner().Run(scenario, blue, red, runs, seed);
            ConsoleRenderer.PrintSummary(summary);

            TrySave(repo, () =>
            {
                repo.SaveMonteCarlo(summary, DateTime.Now);
                Console.WriteLine($"Saved batch to {repo.Name}.");
            });
            return 0;
        }
        case "compare":
        {
            int runs = GetInt("runs", 1000);
            int seed = GetInt("seed", 1);
            var repo = CreateRepository();

            var summaries = CompareCommand.Run(scenario, runs, seed);

            TrySave(repo, () =>
            {
                foreach (var s in summaries) repo.SaveMonteCarlo(s, DateTime.Now);
                Console.WriteLine($"Saved {summaries.Count} batches to {repo.Name}.");
            });
            return 0;
        }
        case "report":
        {
            int seed = GetSeed("seed", 42);
            int runs = GetInt("runs", 500);
            string path = GetString("out", "report.html");

            var result = new SimulationEngine(blue, red, new SeededRandomProvider(seed)).Run(scenario);

            var summaries = new List<MonteCarloSummary>();
            foreach (var b in new[] { "Aggressive", "Defensive" })
                foreach (var r in new[] { "Aggressive", "Defensive" })
                    summaries.Add(new MonteCarloRunner().Run(
                        scenario, StrategyFactory.Create(b), StrategyFactory.Create(r), runs, 1));

            HtmlReport.Write(path, scenario, result, seed, summaries);
            string fullPath = Path.GetFullPath(path);
            Console.WriteLine($"Report written to {fullPath}");
            if (options.ContainsKey("open")) TryOpen(fullPath);
            return 0;
        }
        case "play":
        {
            int seed = GetSeed("seed", 42);
            var repo = CreateRepository();
            var startedAt = DateTime.Now;

            var result = PlayCommand.Run(scenario, blue, red, seed);
            if (result is null)
            {
                Console.WriteLine("Battle not finished, nothing saved.");
                return 0;
            }

            Console.WriteLine($"Seed: {seed}");
            TrySave(repo, () =>
            {
                int id = repo.SaveRun(result, seed, startedAt);
                Console.WriteLine($"Saved to {repo.Name} as run #{id}.");
            });
            return 0;
        }
        case "list":
        {
            var repo = CreateRepository();
            var runs = repo.ListRuns(10);
            if (runs.Count == 0)
            {
                Console.WriteLine("No saved runs.");
                return 0;
            }
            Console.WriteLine(string.Format("{0,4} {1,-12} {2,6} {3,-11} {4,-11} {5,-7} {6,6}  {7}",
                "Id", "Scenario", "Seed", "Blue", "Red", "Winner", "Turns", "Started"));
            foreach (var r in runs)
                Console.WriteLine(string.Format("{0,4} {1,-12} {2,6} {3,-11} {4,-11} {5,-7} {6,6}  {7:yyyy-MM-dd HH:mm}",
                    r.Id, r.ScenarioName, r.Seed, r.BlueStrategy, r.RedStrategy, r.Winner, r.TurnsPlayed, r.StartedAt));
            return 0;
        }
        default:
            Console.WriteLine($"Unknown command '{command}'.");
            PrintUsage();
            return 1;
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

string GetString(string key, string fallback) => options.TryGetValue(key, out var v) && v.Length > 0 ? v : fallback;

int GetInt(string key, int fallback)
{
    if (!options.TryGetValue(key, out var v)) return fallback;
    if (int.TryParse(v, out int n)) return n;
    throw new ArgumentException($"--{key} must be a whole number.");
}

// --seed random picks a new seed each time (it is printed and saved, so the battle can be replayed).
int GetSeed(string key, int fallback)
{
    if (options.TryGetValue(key, out var v) && v.Equals("random", StringComparison.OrdinalIgnoreCase))
        return Random.Shared.Next(1, 1_000_000);
    return GetInt(key, fallback);
}

// Uses MySQL when WARSIM_CONNECTION is set and reachable; otherwise prints one message and falls back.
static IRunRepository CreateRepository()
{
    var connectionString = Environment.GetEnvironmentVariable("WARSIM_CONNECTION");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("Note: WARSIM_CONNECTION is not set, using in-memory storage (nothing is kept after exit).");
        return new InMemoryRunRepository();
    }

    try
    {
        var repo = new MySqlRunRepository(connectionString);
        repo.CheckConnection();
        return repo;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Note: could not connect to MySQL ({ex.Message}). Using in-memory storage instead.");
        return new InMemoryRunRepository();
    }
}

static void TrySave(IRunRepository repo, Action save)
{
    try
    {
        save();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: could not save to {repo.Name} ({ex.Message}).");
    }
}

static void TryOpen(string path)
{
    try
    {
        var psi = new System.Diagnostics.ProcessStartInfo("open") { UseShellExecute = false };
        psi.ArgumentList.Add(path);
        System.Diagnostics.Process.Start(psi);
    }
    catch
    {
        Console.WriteLine("Could not open the browser automatically. Open the file yourself.");
    }
}

static Dictionary<string, string> ParseOptions(string[] rest)
{
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < rest.Length; i++)
    {
        if (!rest[i].StartsWith("--")) continue;
        string key = rest[i][2..];
        string value = i + 1 < rest.Length && !rest[i + 1].StartsWith("--") ? rest[++i] : "";
        dict[key] = value;
    }
    return dict;
}

static void PrintUsage()
{
    Console.WriteLine("WarSim - turn-based war game simulation");
    Console.WriteLine();
    Console.WriteLine("  run         --seed 42 (or random) --blue Defensive --red Aggressive [--delay 600] [--log]");
    Console.WriteLine("  montecarlo  --runs 1000 --seed 1 --blue Defensive --red Aggressive");
    Console.WriteLine("  compare     --runs 1000 --seed 1     (all four strategy pairings in one table)");
    Console.WriteLine("  report      --seed 42 --runs 500 [--out report.html] [--open]   (HTML page for the browser)");
    Console.WriteLine("  play        --seed 42 --blue Defensive --red Aggressive   (you switch Blue's orders each turn)");
    Console.WriteLine("  list        show the last 10 saved runs");
    Console.WriteLine();
    Console.WriteLine("  --delay N   milliseconds between animation frames (0 = print all frames, no animation)");
    Console.WriteLine("  --log       print the plain text event log instead of the animation");
    Console.WriteLine();
    Console.WriteLine("Strategies: Aggressive, Defensive, Balanced");
    Console.WriteLine("Set WARSIM_CONNECTION to save to MySQL (otherwise in-memory is used).");
}