using WarSim.Core.Analysis;
using WarSim.Core.Engine;

namespace WarSim.Persistence;

/// <summary>Fallback used when no database is configured. Data lives only while the program runs.</summary>
public class InMemoryRunRepository : IRunRepository
{
    private readonly List<SavedRun> _runs = new();
    private readonly List<MonteCarloSummary> _batches = new();
    private int _nextId = 1;

    public string Name => "in-memory";

    public int SaveRun(BattleResult result, int seed, DateTime startedAt)
    {
        int id = _nextId++;
        _runs.Add(new SavedRun(id, result.ScenarioName, seed, result.BlueStrategy, result.RedStrategy,
                               result.Winner?.ToString() ?? "Draw", result.TurnsPlayed, startedAt));
        return id;
    }

    public void SaveMonteCarlo(MonteCarloSummary summary, DateTime createdAt) => _batches.Add(summary);

    public IReadOnlyList<SavedRun> ListRuns(int limit) =>
        _runs.OrderByDescending(r => r.StartedAt).ThenByDescending(r => r.Id).Take(limit).ToList();
}
