using WarSim.Core.Domain;

namespace WarSim.Core.AI;

/// <summary>Infantry hold good ground (defensive); Armor and Recon hunt the enemy (aggressive).</summary>
public class BalancedStrategy : IAiStrategy
{
    private readonly AggressiveStrategy _aggressive = new();
    private readonly DefensiveStrategy _defensive = new();

    public string Name => "Balanced";

    public UnitOrder Decide(Unit unit, BattleState state) =>
        unit is Infantry ? _defensive.Decide(unit, state) : _aggressive.Decide(unit, state);
}