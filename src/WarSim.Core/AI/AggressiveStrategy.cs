using WarSim.Core.Domain;

namespace WarSim.Core.AI;

/// <summary>Every unit moves toward the nearest enemy and attacks when in range.</summary>
public class AggressiveStrategy : IAiStrategy
{
    public string Name => "Aggressive";

    public UnitOrder Decide(Unit unit, BattleState state)
    {
        var enemy = AiHelpers.NearestEnemy(unit, state);
        if (enemy is null) return new UnitOrder(null, 0, false);
        return new UnitOrder(enemy.Position, unit.AttackRange, true);
    }
}
