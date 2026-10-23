namespace WarSim.Core.AI;

public static class StrategyFactory
{
    public static IAiStrategy Create(string name) => name.Trim().ToLowerInvariant() switch
    {
        "aggressive" => new AggressiveStrategy(),
        "defensive" => new DefensiveStrategy(),
        _ => throw new ArgumentException($"Unknown strategy '{name}'. Use Aggressive or Defensive.")
    };
}
