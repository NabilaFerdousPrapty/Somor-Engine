using WarSim.Core.Domain;

namespace WarSim.Core.AI;

/// <summary>Units hold position on good terrain and only attack enemies that come into range.</summary>
public class DefensiveStrategy : IAiStrategy
{
    private const int SearchRadius = 6;

    public string Name => "Defensive";

    public UnitOrder Decide(Unit unit, BattleState state)
    {
        var enemy = AiHelpers.NearestEnemy(unit, state);
        if (enemy is not null && unit.Position.DistanceTo(enemy.Position) <= unit.AttackRange)
            return new UnitOrder(null, 0, true);          // enemy in range: stand and fight

        var here = state.Map.TerrainAt(unit.Position);
        if (here == TerrainType.Forest || here == TerrainType.Hills)
            return new UnitOrder(null, 0, true);          // already on good ground: hold

        var occupied = state.Units.Where(u => u.IsAlive).Select(u => u.Position).ToHashSet();
        Position? best = null;
        int bestDist = int.MaxValue;
        for (int y = 0; y < state.Map.Height; y++)
            for (int x = 0; x < state.Map.Width; x++)
            {
                var p = new Position(x, y);
                var t = state.Map.TerrainAt(p);
                if (t != TerrainType.Forest && t != TerrainType.Hills) continue;
                if (occupied.Contains(p)) continue;
                int d = unit.Position.DistanceTo(p);
                if (d > SearchRadius || d >= bestDist) continue;   // strict '<' keeps scan order deterministic
                best = p;
                bestDist = d;
            }

        return best is null
            ? new UnitOrder(null, 0, true)
            : new UnitOrder(best, 0, true);
    }
}
