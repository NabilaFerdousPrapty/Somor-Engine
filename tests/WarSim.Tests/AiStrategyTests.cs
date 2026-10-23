using WarSim.Core.AI;
using WarSim.Core.Domain;
using Xunit;

namespace WarSim.Tests;

public class AiStrategyTests
{
    [Fact]
    public void Aggressive_moves_toward_nearest_enemy()
    {
        var map = GameMap.FromRows(new[] { "PPPPPPPPPP" });
        var blue = new Infantry(1, "B1", Side.Blue, new Position(0, 0));
        var near = new Infantry(2, "R1", Side.Red, new Position(5, 0));
        var far = new Infantry(3, "R2", Side.Red, new Position(9, 0));
        var state = new BattleState(map, new List<Unit> { blue, near, far }, 1);

        var order = new AggressiveStrategy().Decide(blue, state);

        Assert.NotNull(order.MoveTarget);
        Assert.Equal(new Position(5, 0), order.MoveTarget.Value);
    }

    [Fact]
    public void Defensive_holds_position_on_good_terrain_when_no_enemy_in_range()
    {
        var map = GameMap.FromRows(new[] { "FPPPPPPPPP" });
        var blue = new Infantry(1, "B1", Side.Blue, new Position(0, 0));   // standing in forest
        var red = new Infantry(2, "R1", Side.Red, new Position(9, 0));
        var state = new BattleState(map, new List<Unit> { blue, red }, 1);

        var order = new DefensiveStrategy().Decide(blue, state);

        Assert.Null(order.MoveTarget);
    }

    [Fact]
    public void Defensive_moves_to_nearby_good_terrain_when_on_plains()
    {
        var map = GameMap.FromRows(new[] { "PFPPPPPPPP" });
        var blue = new Infantry(1, "B1", Side.Blue, new Position(0, 0));
        var red = new Infantry(2, "R1", Side.Red, new Position(9, 0));
        var state = new BattleState(map, new List<Unit> { blue, red }, 1);

        var order = new DefensiveStrategy().Decide(blue, state);

        Assert.NotNull(order.MoveTarget);
        Assert.Equal(new Position(1, 0), order.MoveTarget.Value);
    }
}
