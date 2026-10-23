using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

/// <summary>Winner is null for a draw.</summary>
public record BattleResult(
    string ScenarioName,
    string BlueStrategy,
    string RedStrategy,
    Side? Winner,
    int TurnsPlayed,
    int BlueCasualties,
    int RedCasualties,
    IReadOnlyList<BattleEvent> Events);
