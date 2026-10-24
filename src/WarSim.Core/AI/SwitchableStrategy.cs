using WarSim.Core.Domain;

namespace WarSim.Core.AI;

/// <summary>
/// Wraps another strategy so it can be swapped between turns (the player changes Blue's orders).
/// The engine does not know or care: it still just sees an IAiStrategy.
/// </summary>
public class SwitchableStrategy : IAiStrategy
{
    public IAiStrategy Current { get; set; }

    public SwitchableStrategy(IAiStrategy initial) => Current = initial;

    public string Name => Current.Name;

    public UnitOrder Decide(Unit unit, BattleState state) => Current.Decide(unit, state);
}