using System.Text.Json.Serialization;
using WarSim.Core.AI;
using WarSim.Core.Analysis;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

// Make 'dotnet run --project src/WarSim.Web' find wwwroot no matter which folder you run it from.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = FindProjectDir()
});

// Hosts such as Render set PORT. Locally we listen on http://localhost:5080
string? portFromHost = Environment.GetEnvironmentVariable("PORT");
string port = portFromHost ?? "5080";
string host = portFromHost is null ? "localhost" : "0.0.0.0";
builder.WebHost.UseUrls($"http://{host}:{port}");

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// One battle. 'switches' lets the player change Blue's orders: "3:Aggressive,8:Balanced"
// means Blue follows 'blue' until turn 3, Aggressive from turn 3, Balanced from turn 8.
// The same seed and switches always give the same battle, so the page can re-ask freely.
app.MapGet("/api/battle", (int? seed, string? blue, string? red, string? switches) =>
{
    try
    {
        var scenario = Scenario.OpenField();
        int battleSeed = seed ?? 42;
        var plan = ParseSwitches(switches);

        var blueControl = new SwitchableStrategy(StrategyFactory.Create(blue ?? "Defensive"));
        var redStrategy = StrategyFactory.Create(red ?? "Aggressive");
        var engine = new SimulationEngine(blueControl, redStrategy, new SeededRandomProvider(battleSeed));
        engine.Start(scenario);

        int turn = 0;
        while (!engine.IsFinished)
        {
            turn++;
            if (plan.TryGetValue(turn, out var newOrders))
            {
                blueControl.Current = StrategyFactory.Create(newOrders);
                engine.AddNote($"Blue commander changed orders to {newOrders}");
            }
            engine.PlayTurn();
        }

        var result = engine.BuildResult();
        return Results.Ok(new
        {
            scenario = scenario.Name,
            seed = battleSeed,
            turnLimit = scenario.TurnLimit,
            map = MapRows(scenario.Map),
            winner = result.Winner?.ToString() ?? "Draw",
            turnsPlayed = result.TurnsPlayed,
            blueCasualties = result.BlueCasualties,
            redCasualties = result.RedCasualties,
            blueStrategy = result.BlueStrategy,
            redStrategy = result.RedStrategy,
            snapshots = result.Snapshots,
            events = result.Events
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Monte Carlo for one pairing.
app.MapGet("/api/montecarlo", (int? runs, int? seed, string? blue, string? red) =>
{
    try
    {
        int n = Math.Clamp(runs ?? 500, 1, 2000);
        var summary = new MonteCarloRunner().Run(
            Scenario.OpenField(),
            StrategyFactory.Create(blue ?? "Defensive"),
            StrategyFactory.Create(red ?? "Aggressive"),
            n, seed ?? 1);
        return Results.Ok(ToDto(summary));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Monte Carlo for every pairing of Aggressive / Defensive / Balanced.
app.MapGet("/api/compare", (int? runs, int? seed) =>
{
    int n = Math.Clamp(runs ?? 300, 1, 2000);
    int baseSeed = seed ?? 1;
    string[] names = { "Aggressive", "Defensive", "Balanced" };
    var pairs = names.SelectMany(b => names.Select(r => (Blue: b, Red: r))).ToArray();
    var scenario = Scenario.OpenField();
    var results = new MonteCarloSummary[pairs.Length];

    Parallel.For(0, pairs.Length, i =>
    {
        results[i] = new MonteCarloRunner().Run(
            scenario,
            StrategyFactory.Create(pairs[i].Blue),
            StrategyFactory.Create(pairs[i].Red),
            n, baseSeed);
    });

    return Results.Ok(results.Select(ToDto));
});

app.Run();

static object ToDto(MonteCarloSummary s) => new
{
    blue = s.BlueStrategy,
    red = s.RedStrategy,
    runs = s.Runs,
    blueWins = s.BlueWins,
    redWins = s.RedWins,
    draws = s.Draws,
    blueWinRate = s.BlueWinRate,
    redWinRate = s.RedWinRate,
    avgTurns = s.AvgTurns,
    stdDevTurns = s.StdDevTurns
};

static List<string> MapRows(GameMap map)
{
    var rows = new List<string>();
    for (int y = 0; y < map.Height; y++)
    {
        var chars = new char[map.Width];
        for (int x = 0; x < map.Width; x++)
            chars[x] = map.TerrainAt(new Position(x, y)).ToLetter();
        rows.Add(new string(chars));
    }
    return rows;
}

static Dictionary<int, string> ParseSwitches(string? raw)
{
    var plan = new Dictionary<int, string>();
    if (string.IsNullOrWhiteSpace(raw)) return plan;

    foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        var bits = part.Split(':');
        if (bits.Length != 2 || !int.TryParse(bits[0], out int turn) || turn < 1 || turn > 200)
            throw new ArgumentException($"Bad switch '{part}'. Use turn:Strategy, for example 3:Aggressive.");
        plan[turn] = StrategyFactory.Create(bits[1]).Name;   // also validates the name
    }
    return plan;
}

static string? FindProjectDir()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
    {
        if (File.Exists(Path.Combine(dir.FullName, "WarSim.Web.csproj")))
            return dir.FullName;
    }
    return null;
}