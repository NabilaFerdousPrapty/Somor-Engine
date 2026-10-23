namespace WarSim.Core.Domain;

public enum TerrainType { Plains, Forest, Hills, Water }

public static class TerrainRules
{
    public static bool IsPassable(this TerrainType t) => t != TerrainType.Water;

    public static int MovementCost(this TerrainType t) => t switch
    {
        TerrainType.Plains => 1,
        TerrainType.Forest => 2,
        TerrainType.Hills => 2,
        _ => int.MaxValue
    };

    public static double DefenceBonus(this TerrainType t) => t switch
    {
        TerrainType.Forest => 0.25,
        TerrainType.Hills => 0.40,
        _ => 0.0
    };

    public static TerrainType FromLetter(char c) => char.ToUpperInvariant(c) switch
    {
        'P' => TerrainType.Plains,
        'F' => TerrainType.Forest,
        'H' => TerrainType.Hills,
        'W' => TerrainType.Water,
        _ => throw new ArgumentException($"Unknown terrain letter '{c}'.")
    };

    public static char ToLetter(this TerrainType t) => t switch
    {
        TerrainType.Plains => 'P',
        TerrainType.Forest => 'F',
        TerrainType.Hills => 'H',
        _ => 'W'
    };
}
