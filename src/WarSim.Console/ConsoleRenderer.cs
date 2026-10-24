using WarSim.Core.Analysis;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

namespace WarSim.App;

public static class ConsoleRenderer
{
    private const string BlueUnit = "1;97;44";   // bold white on blue
    private const string RedUnit = "1;97;41";    // bold white on red

    private static bool UseColor =>
        !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") is null;

    private static string C(string text, string code) =>
        UseColor ? $"\u001b[{code}m{text}\u001b[0m" : text;

    private static string Bar(double fraction, int width)
    {
        int filled = (int)Math.Round(Math.Clamp(fraction, 0, 1) * width);
        return new string('█', filled) + new string('░', width - filled);
    }

    private static string HealthBar(int health, int max, int width)
    {
        int filled = health <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling((double)width * health / max));
        return new string('█', filled) + new string('░', width - filled);
    }

    private static string Letter(string unitType) => unitType switch
    {
        "Infantry" => "I",
        "Armor" => "A",
        "Recon" => "S",
        _ => "?"
    };

    private static string TerrainCell(TerrainType t) => t switch
    {
        TerrainType.Forest => C(" ♣ ", "32"),
        TerrainType.Hills => C(" ▲ ", "33"),
        TerrainType.Water => C(" ≈ ", "36"),
        _ => C(" · ", "90")
    };

    private static List<BattleEvent> EventsFor(BattleResult result, int turn) =>
        result.Events.Where(e => e.Turn == turn).ToList();

    private static string ControlsHint(bool paused, int delay) =>
        $"Speed {delay} ms   Space pause/resume   Left/Right step   +/- speed   R restart   Q quit" +
        (paused ? "   [PAUSED]" : "");

    private static void PrintFooter(BattleResult result, HashSet<int> activeTurns)
    {
        int lastActive = activeTurns.Count == 0 ? 0 : activeTurns.Max();
        Console.WriteLine();
        if (result.TurnsPlayed > lastActive)
            Console.WriteLine(C($"No more action after turn {lastActive}. The battle ran to the turn limit, so the winner is decided by remaining health.", "90"));

        string winner = result.Winner?.ToString() ?? "Nobody (draw)";
        string winnerColor = result.Winner == Side.Blue ? "1;34" : result.Winner == Side.Red ? "1;31" : "1";
        Console.WriteLine(C($"Winner: {winner}", winnerColor) +
                          $"   after {result.TurnsPlayed} turns   (Blue lost {result.BlueCasualties}, Red lost {result.RedCasualties})");
    }

    /// <summary>
    /// Plays the battle back frame by frame with keyboard controls.
    /// delayMs = 0 (or redirected output) prints every frame one after another with no controls.
    /// </summary>
    public static void Replay(Scenario scenario, BattleResult result, int delayMs)
    {
        // Skip turns where nothing happened, so a stalemate does not flood the screen.
        var activeTurns = result.Events.Select(e => e.Turn).ToHashSet();
        var frames = result.Snapshots.Where(s => s.Turn == 0 || activeTurns.Contains(s.Turn)).ToList();
        bool interactive = delayMs > 0 && !Console.IsOutputRedirected;

        if (!interactive)
        {
            foreach (var snap in frames)
            {
                DrawFrame(scenario, snap, EventsFor(result, snap.Turn), result);
                Console.WriteLine();
            }
            PrintFooter(result, activeTurns);
            return;
        }

        int index = 0;
        int delay = delayMs;
        bool paused = false;
        bool redraw = true;

        while (true)
        {
            bool atEnd = index == frames.Count - 1;

            if (redraw)
            {
                Console.Clear();
                DrawFrame(scenario, frames[index], EventsFor(result, frames[index].Turn), result);
                if (atEnd) PrintFooter(result, activeTurns);
                Console.WriteLine();
                Console.WriteLine(C(ControlsHint(paused, delay), "90"));
                redraw = false;
            }

            int step = 0;
            var waitStart = DateTime.UtcNow;

            while (step == 0)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    char ch = char.ToLowerInvariant(key.KeyChar);

                    if (key.Key == ConsoleKey.Spacebar) { paused = !paused; redraw = true; break; }
                    if (key.Key == ConsoleKey.RightArrow || ch == 'n') { paused = true; step = 1; }
                    else if (key.Key == ConsoleKey.LeftArrow || ch == 'b') { paused = true; step = -1; }
                    else if (ch == '+' || ch == '=') { delay = Math.Max(100, delay - 100); redraw = true; break; }
                    else if (ch == '-' || ch == '_') { delay += 100; redraw = true; break; }
                    else if (ch == 'r') { index = 0; paused = false; redraw = true; break; }
                    else if (ch == 'q' || key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Enter) return;
                }
                else if (!paused && !atEnd && (DateTime.UtcNow - waitStart).TotalMilliseconds >= delay)
                {
                    step = 1;
                }
                else
                {
                    Thread.Sleep(15);
                }
            }

            if (step != 0)
            {
                int next = Math.Clamp(index + step, 0, frames.Count - 1);
                if (next != index)
                {
                    index = next;
                    redraw = true;
                }
            }
        }
    }

    private static void DrawFrame(Scenario scenario, TurnSnapshot snap, List<BattleEvent> turnEvents, BattleResult result)
    {
        var map = scenario.Map;

        Console.WriteLine(C($" {scenario.Name}  |  Turn {snap.Turn}/{scenario.TurnLimit}  |  Blue {result.BlueStrategy} vs Red {result.RedStrategy}", "1"));
        Console.WriteLine();

        Console.WriteLine("┌" + new string('─', map.Width * 3) + "┐");
        for (int y = 0; y < map.Height; y++)
        {
            Console.Write("│");
            for (int x = 0; x < map.Width; x++)
            {
                var p = new Position(x, y);
                var unit = snap.Units.FirstOrDefault(u => u.Position == p);
                if (unit is not null)
                    Console.Write(C($" {Letter(unit.UnitType)} ", unit.Side == Side.Blue ? BlueUnit : RedUnit));
                else
                    Console.Write(TerrainCell(map.TerrainAt(p)));
            }
            Console.WriteLine("│");
        }
        Console.WriteLine("└" + new string('─', map.Width * 3) + "┘");
        Console.WriteLine(C("I infantry  A armor  S recon   ♣ forest  ▲ hills  ≈ water", "90"));
        Console.WriteLine();

        var start = result.Snapshots[0];
        foreach (var side in new[] { Side.Blue, Side.Red })
        {
            string nameColor = side == Side.Blue ? "1;34" : "1;31";
            foreach (var u in start.Units.Where(u => u.Side == side))
            {
                var now = snap.Units.FirstOrDefault(x => x.Name == u.Name);
                string label = C($"{Letter(u.UnitType)} {u.Name.PadRight(16)}", nameColor);
                if (now is null)
                {
                    Console.WriteLine($"  {label} " + C("✖ destroyed", "90"));
                }
                else
                {
                    double ratio = (double)now.Health / now.MaxHealth;
                    string barColor = ratio > 0.5 ? "32" : ratio > 0.25 ? "33" : "31";
                    Console.WriteLine($"  {label} " + C(HealthBar(now.Health, now.MaxHealth, 12), barColor) +
                                      $" {now.Health,3}/{now.MaxHealth}");
                }
            }
        }

        Console.WriteLine();
        if (snap.Turn == 0)
        {
            Console.WriteLine(C("  Start of battle", "90"));
            return;
        }
        foreach (var e in turnEvents.Take(14))
        {
            string code = e.EventType == "Death" ? "1;31" : e.EventType == "Move" ? "90" : e.EventType == "Order" ? "36" : "0";
            Console.WriteLine("  " + C(e.Description, code));
        }
        if (turnEvents.Count > 14)
            Console.WriteLine(C($"  ... and {turnEvents.Count - 14} more", "90"));
    }

    /// <summary>Draws the current state of a battle in progress (used by the interactive play command).</summary>
    public static void DrawLive(Scenario scenario, SimulationEngine engine)
    {
        var result = engine.BuildResult();
        var snap = result.Snapshots[result.Snapshots.Count - 1];
        DrawFrame(scenario, snap, EventsFor(result, snap.Turn), result);
        if (engine.IsFinished)
            PrintFooter(result, result.Events.Select(e => e.Turn).ToHashSet());
    }

    /// <summary>Plain text log of every event (use with --log).</summary>
    public static void PrintBattle(BattleResult result, int seed)
    {
        Console.WriteLine($"Battle: {result.ScenarioName}  seed {seed}  Blue {result.BlueStrategy} vs Red {result.RedStrategy}");
        Console.WriteLine();

        int currentTurn = 0;
        foreach (var e in result.Events)
        {
            if (e.Turn != currentTurn)
            {
                currentTurn = e.Turn;
                Console.WriteLine($"--- Turn {currentTurn} ---");
            }
            Console.WriteLine("  " + e.Description);
        }

        Console.WriteLine();
        string winner = result.Winner?.ToString() ?? "Draw";
        Console.WriteLine($"Result: {winner} after {result.TurnsPlayed} turns " +
                          $"(Blue lost {result.BlueCasualties}, Red lost {result.RedCasualties})");
    }

    public static void PrintSummary(MonteCarloSummary s)
    {
        Console.WriteLine(C($"Scenario: {s.ScenarioName}   Runs: {s.Runs}   Base seed: {s.BaseSeed}", "1"));
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10} {4,16}", "Side", "Strategy", "Wins", "Win rate", "Avg casualties"));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1} {4,16:F1}", "Blue", s.BlueStrategy, s.BlueWins, s.BlueWinRate, s.AvgBlueCasualties));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1} {4,16:F1}", "Red", s.RedStrategy, s.RedWins, s.RedWinRate, s.AvgRedCasualties));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1}", "Draw", "", s.Draws, s.DrawRate));
        Console.WriteLine();
        Console.WriteLine("Blue " + C(Bar(s.BlueWinRate, 30), "34") + $" {s.BlueWinRate:P1}");
        Console.WriteLine("Red  " + C(Bar(s.RedWinRate, 30), "31") + $" {s.RedWinRate:P1}");
        if (s.Draws > 0)
            Console.WriteLine("Draw " + C(Bar(s.DrawRate, 30), "90") + $" {s.DrawRate:P1}");
        Console.WriteLine();
        Console.WriteLine($"Average battle length: {s.AvgTurns:F1} turns (std dev {s.StdDevTurns:F1})");
    }
}