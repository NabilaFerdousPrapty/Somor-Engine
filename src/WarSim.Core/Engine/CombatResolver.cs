using WarSim.Core.Common;
using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

public class CombatResolver
{
    private readonly IRandomProvider _random;

    public CombatResolver(IRandomProvider random) => _random = random;

    public bool CanAttack(Unit attacker, Unit defender) =>
        attacker.IsAlive && defender.IsAlive
        && attacker.Side != defender.Side
        && attacker.Position.DistanceTo(defender.Position) <= attacker.AttackRange;

    /// <summary>
    /// Simplified model:
    ///   raw     = attack * (1 + random in [-0.2, +0.2])
    ///   blocked = defense * (1 + terrain defence bonus)
    ///   damage  = max(1, round(raw - blocked))
    /// </summary>
    public int CalculateDamage(Unit attacker, Unit defender, GameMap map)
    {
        double raw = attacker.Attack * (1 + (_random.NextDouble() * 0.4 - 0.2));
        double blocked = defender.Defense * (1 + defender.TerrainDefenceBonus(map.TerrainAt(defender.Position)));
        return Math.Max(1, (int)Math.Round(raw - blocked));
    }

    /// <summary>Returns false (and changes nothing) if the attack is not allowed.</summary>
    public bool TryAttack(Unit attacker, Unit defender, GameMap map, out int damage)
    {
        damage = 0;
        if (!CanAttack(attacker, defender)) return false;
        damage = CalculateDamage(attacker, defender, map);
        defender.TakeDamage(damage);
        return true;
    }
}
