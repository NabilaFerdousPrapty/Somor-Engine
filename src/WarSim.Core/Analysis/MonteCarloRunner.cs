using WarSim.Core.AI;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

namespace WarSim.Core.Analysis;

public class MonteCarloRunner
{
    /// <summary>Runs the scenario 'runs' times with seeds baseSeed + i. Each run is fully independent.</summary>
    public MonteCarloSummary Run(Scenario scenario, IAiStrategy blue, IAiStrategy red, int runs, int baseSeed)
    {
        if (runs <= 0) throw new ArgumentOutOfRangeException(nameof(runs), "Runs must be at least 1.");

        int blueWins = 0, redWins = 0, draws = 0;
        long blueCasualties = 0, redCasualties = 0;
        var turns = new List<int>(runs);

        for (int i = 0; i < runs; i++)
        {
            var engine = new SimulationEngine(blue, red, new SeededRandomProvider(baseSeed + i));
            var result = engine.Run(scenario);

            if (result.Winner == Side.Blue) blueWins++;
            else if (result.Winner == Side.Red) redWins++;
            else draws++;

            blueCasualties += result.BlueCasualties;
            redCasualties += result.RedCasualties;
            turns.Add(result.TurnsPlayed);
        }

        double avgTurns = turns.Average();
        double variance = turns.Sum(t => (t - avgTurns) * (t - avgTurns)) / runs;

        return new MonteCarloSummary(
            scenario.Name, runs, baseSeed, blue.Name, red.Name,
            blueWins, redWins, draws,
            (double)blueCasualties / runs, (double)redCasualties / runs,
            avgTurns, Math.Sqrt(variance));
    }
}
