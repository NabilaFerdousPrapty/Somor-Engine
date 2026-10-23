using WarSim.Core.Domain;

namespace WarSim.Core.AI;

public record BattleState(GameMap Map, IReadOnlyList<Unit> Units, int Turn);

/// <summary>What a unit wants to do this turn. MoveTarget null = stay put.</summary>
public record UnitOrder(Position? MoveTarget, int StopDistance, bool Attack);
