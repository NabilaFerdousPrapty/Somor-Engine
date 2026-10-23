using WarSim.Core.AI;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;
using Xunit;

namespace WarSim.Tests;

public class DeterminismTests
{
    private static string Log(int seed)
    {
        var engine = new SimulationEngine(new AggressiveStrategy(), new DefensiveStrategy(), new SeededRandomProvider(seed));
        var result = engine.Run(Scenario.OpenField());
        return string.Join("|", result.Events.Select(e => e.Description));
    }

    [Fact]
    public void Same_seed_gives_identical_event_log()
    {
        string first = Log(42);
        Assert.False(string.IsNullOrEmpty(first));
        Assert.Equal(first, Log(42));
    }

    [Fact]
    public void Different_seeds_give_different_logs()
    {
        var logs = Enumerable.Range(1, 5).Select(Log).ToHashSet();
        Assert.True(logs.Count > 1);
    }
}
