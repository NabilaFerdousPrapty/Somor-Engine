using WarSim.Core.Domain;

namespace WarSim.Core.AI;

public interface IAiStrategy
{
    string Name { get; }
    UnitOrder Decide(Unit unit, BattleState state);
}
