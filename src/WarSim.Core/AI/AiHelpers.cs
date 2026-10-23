using WarSim.Core.Domain;

namespace WarSim.Core.AI;

internal static class AiHelpers
{
    public static Unit? NearestEnemy(Unit unit, BattleState state) =>
        state.Units
            .Where(u => u.IsAlive && u.Side != unit.Side)
            .OrderBy(u => unit.Position.DistanceTo(u.Position))
            .ThenBy(u => u.Id)
            .FirstOrDefault();
}
