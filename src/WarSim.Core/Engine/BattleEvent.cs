namespace WarSim.Core.Engine;

public record BattleEvent(int Turn, string EventType, string UnitName, string? TargetName, int? Damage, string Description);
