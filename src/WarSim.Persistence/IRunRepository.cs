using WarSim.Core.Analysis;
using WarSim.Core.Engine;

namespace WarSim.Persistence;

public record SavedRun(
    int Id, string ScenarioName, int Seed, string BlueStrategy, string RedStrategy,
    string Winner, int TurnsPlayed, DateTime StartedAt);

public interface IRunRepository
{
    /// <summary>Short label for messages, e.g. "MySQL" or "in-memory".</summary>
    string Name { get; }

    /// <summary>Saves one battle and all its events. Returns the new run id.</summary>
    int SaveRun(BattleResult result, int seed, DateTime startedAt);

    void SaveMonteCarlo(MonteCarloSummary summary, DateTime createdAt);

    /// <summary>Most recent runs first.</summary>
    IReadOnlyList<SavedRun> ListRuns(int limit);
}
