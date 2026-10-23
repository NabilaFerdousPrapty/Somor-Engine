using WarSim.App;
using WarSim.Core.AI;
using WarSim.Core.Analysis;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

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
            int seed = GetInt("seed", 42);
            ConsoleRenderer.PrintMap(scenario);
            var engine = new SimulationEngine(blue, red, new SeededRandomProvider(seed));
            ConsoleRenderer.PrintBattle(engine.Run(scenario), seed);
            return 0;
        }
        case "montecarlo":
        {
            int runs = GetInt("runs", 1000);
            int seed = GetInt("seed", 1);
            var summary = new MonteCarloRunner().Run(scenario, blue, red, runs, seed);
            ConsoleRenderer.PrintSummary(summary);
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
    Console.WriteLine("  run         --seed 42 --blue Defensive --red Aggressive");
    Console.WriteLine("  montecarlo  --runs 1000 --seed 1 --blue Defensive --red Aggressive");
    Console.WriteLine();
    Console.WriteLine("Strategies: Aggressive, Defensive");
}
