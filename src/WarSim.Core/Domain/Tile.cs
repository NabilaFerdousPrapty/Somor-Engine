namespace WarSim.Core.Domain;

public class Tile
{
    public TerrainType Terrain { get; }

    public Tile(TerrainType terrain) => Terrain = terrain;
}
