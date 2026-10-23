namespace WarSim.Core.Domain;

public class GameMap
{
    private readonly Tile[,] _tiles;

    public int Width { get; }
    public int Height { get; }

    public GameMap(int width, int height)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Map size must be positive.");
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile(TerrainType.Plains);
    }

    /// <summary>Builds a map from rows of letters: P plains, F forest, H hills, W water.</summary>
    public static GameMap FromRows(IReadOnlyList<string> rows)
    {
        if (rows.Count == 0) throw new ArgumentException("At least one row is required.");
        int width = rows[0].Length;
        var map = new GameMap(width, rows.Count);
        for (int y = 0; y < rows.Count; y++)
        {
            if (rows[y].Length != width) throw new ArgumentException($"Row {y} has a different length.");
            for (int x = 0; x < width; x++)
                map._tiles[x, y] = new Tile(TerrainRules.FromLetter(rows[y][x]));
        }
        return map;
    }

    public bool InBounds(Position p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

    public TerrainType TerrainAt(Position p) => _tiles[p.X, p.Y].Terrain;
}
