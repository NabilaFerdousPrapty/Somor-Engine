using WarSim.Core.AI;
using WarSim.Core.Analysis;
using WarSim.Core.Domain;
using Xunit;

namespace WarSim.Tests;

public class MonteCarloTests
{
    private static MonteCarloSummary Run(int runs, int baseSeed) =>
        new MonteCarloRunner().Run(Scenario.OpenField(), new DefensiveStrategy(), new AggressiveStrategy(), runs, baseSeed);

    [Fact]
    public void Wins_plus_draws_equal_number_of_runs()
    {
        var s = Run(50, 1);
        Assert.Equal(50, s.BlueWins + s.RedWins + s.Draws);
    }

    [Fact]
    public void Results_are_reproducible_for_a_given_base_seed()
    {
        Assert.Equal(Run(30, 7), Run(30, 7));
    }
}
