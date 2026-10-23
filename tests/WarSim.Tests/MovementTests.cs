using WarSim.Core.Domain;
using WarSim.Core.Engine;
using Xunit;

namespace WarSim.Tests;

public class MovementTests
{
    private static int Move(Unit unit, Position target, GameMap map)
    {
        var occupied = new HashSet<Position> { unit.Position };
        return new MovementService().MoveToward(unit, target, 0, map, occupied);
    }

    [Fact]
    public void Unit_cannot_move_onto_water()
    {
        var map = GameMap.FromRows(new[] { "PWP" });
        var unit = new Infantry(1, "I", Side.Blue, new Position(0, 0));

        int steps = Move(unit, new Position(2, 0), map);

        Assert.Equal(0, steps);
        Assert.Equal(new Position(0, 0), unit.Position);
    }

    [Fact]
    public void Unit_cannot_move_off_the_map()
    {
        var map = GameMap.FromRows(new[] { "PPP" });
        var unit = new Infantry(1, "I", Side.Blue, new Position(0, 0));

        int steps = Move(unit, new Position(-5, 0), map);

        Assert.Equal(0, steps);
        Assert.Equal(new Position(0, 0), unit.Position);
    }

    [Fact]
    public void Movement_points_are_respected()
    {
        var map = GameMap.FromRows(new[] { "PPPPPPPPPP" });
        var unit = new Infantry(1, "I", Side.Blue, new Position(0, 0)); // movement 3

        int steps = Move(unit, new Position(9, 0), map);

        Assert.Equal(3, steps);
        Assert.Equal(new Position(3, 0), unit.Position);
    }

    [Fact]
    public void Forest_costs_more_than_plains()
    {
        var plains = GameMap.FromRows(new[] { "PPPPP" });
        var forest = GameMap.FromRows(new[] { "FFFFF" });
        var a = new Infantry(1, "A", Side.Blue, new Position(0, 0));
        var b = new Infantry(2, "B", Side.Blue, new Position(0, 0));

        int plainsSteps = Move(a, new Position(4, 0), plains);
        int forestSteps = Move(b, new Position(4, 0), forest);

        Assert.Equal(3, plainsSteps);
        Assert.Equal(1, forestSteps); // cost 2 per tile, only 3 points
    }
}
