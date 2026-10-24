using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

public record UnitSnapshot(string Name, Side Side, string UnitType, Position Position, int Health, int MaxHealth);

/// <summary>The state of the battlefield at the end of a turn (Turn 0 = starting positions).</summary>
public record TurnSnapshot(int Turn, IReadOnlyList<UnitSnapshot> Units);
