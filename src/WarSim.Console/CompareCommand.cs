using WarSim.Core.AI;
using WarSim.Core.Analysis;
using WarSim.Core.Domain;

namespace WarSim.App;

/// <summary>Runs a Monte Carlo for every strategy pairing and prints one comparison table.</summary>
public static class CompareCommand
{
    public static List<MonteCarloSummary> Run(Scenario scenario, int runs, int baseSeed)
    {
        string[] strategies = { "Aggressive", "Defensive" };
        var summaries = new List<MonteCarloSummary>();

        Console.WriteLine($"Scenario: {scenario.Name}   Runs per pairing: {runs}   Base seed: {baseSeed}");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,-12} {1,-12} {2,10} {3,10} {4,7} {5,12} {6,11}",
            "Blue", "Red", "Blue wins", "Red wins", "Draws", "Blue win %", "Avg turns"));

        foreach (var blueName in strategies)
        {
            foreach (var redName in strategies)
            {
                var summary = new MonteCarloRunner().Run(
                    scenario,
                    StrategyFactory.Create(blueName),
                    StrategyFactory.Create(redName),
                    runs, baseSeed);
                summaries.Add(summary);

                Console.WriteLine(string.Format("{0,-12} {1,-12} {2,10} {3,10} {4,7} {5,12:P1} {6,11:F1}",
                    blueName, redName, summary.BlueWins, summary.RedWins, summary.Draws,
                    summary.BlueWinRate, summary.AvgTurns));
            }
        }

        return summaries;
    }
}