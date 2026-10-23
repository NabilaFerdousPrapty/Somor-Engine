using WarSim.Core.Analysis;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

namespace WarSim.App;

public static class ConsoleRenderer
{
    public static void PrintMap(Scenario scenario)
    {
        var units = scenario.CreateUnits();
        Console.WriteLine($"Map: {scenario.Name}  ({scenario.Map.Width}x{scenario.Map.Height})");
        Console.WriteLine("Legend: B/R = Blue/Red unit, F forest, H hills, W water, . plains");
        for (int y = 0; y < scenario.Map.Height; y++)
        {
            for (int x = 0; x < scenario.Map.Width; x++)
            {
                var p = new Position(x, y);
                var unit = units.FirstOrDefault(u => u.Position == p);
                char c = unit is not null
                    ? (unit.Side == Side.Blue ? 'B' : 'R')
                    : scenario.Map.TerrainAt(p) == TerrainType.Plains ? '.' : scenario.Map.TerrainAt(p).ToLetter();
                Console.Write(c + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

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
        Console.WriteLine($"Scenario: {s.ScenarioName}   Runs: {s.Runs}   Base seed: {s.BaseSeed}");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10} {4,16}", "Side", "Strategy", "Wins", "Win rate", "Avg casualties"));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1} {4,16:F1}", "Blue", s.BlueStrategy, s.BlueWins, s.BlueWinRate, s.AvgBlueCasualties));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1} {4,16:F1}", "Red", s.RedStrategy, s.RedWins, s.RedWinRate, s.AvgRedCasualties));
        Console.WriteLine(string.Format("{0,-6} {1,-12} {2,6} {3,10:P1}", "Draw", "", s.Draws, s.DrawRate));
        Console.WriteLine();
        Console.WriteLine($"Average battle length: {s.AvgTurns:F1} turns (std dev {s.StdDevTurns:F1})");
    }
}
