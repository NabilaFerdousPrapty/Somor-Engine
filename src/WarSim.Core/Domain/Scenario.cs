namespace WarSim.Core.Domain;

public record UnitSpawn(string Type, Side Side, Position Position, int Count = 1);

public class Scenario
{
    public string Name { get; }
    public GameMap Map { get; }
    public int TurnLimit { get; }
    public IReadOnlyList<UnitSpawn> Spawns { get; }

    public Scenario(string name, GameMap map, int turnLimit, IReadOnlyList<UnitSpawn> spawns)
    {
        Name = name;
        Map = map;
        TurnLimit = turnLimit;
        Spawns = spawns;
    }

    /// <summary>Creates a fresh set of units, so every run starts from the same clean state.</summary>
    public List<Unit> CreateUnits()
    {
        var units = new List<Unit>();
        var occupied = new HashSet<Position>();
        int id = 1;
        foreach (var spawn in Spawns)
        {
            for (int i = 0; i < spawn.Count; i++)
            {
                var pos = FindFreeTile(spawn.Position, occupied);
                occupied.Add(pos);
                units.Add(UnitFactory.Create(spawn.Type, id++, spawn.Side, pos));
            }
        }
        return units;
    }

    private Position FindFreeTile(Position start, HashSet<Position> occupied)
    {
        int maxRadius = Math.Max(Map.Width, Map.Height);
        for (int r = 0; r <= maxRadius; r++)
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                    var p = new Position(start.X + dx, start.Y + dy);
                    if (Map.InBounds(p) && Map.TerrainAt(p).IsPassable() && !occupied.Contains(p))
                        return p;
                }
        throw new InvalidOperationException("No free tile available for a unit.");
    }

    /// <summary>The 6x5 map from the project diagram: 3 units per side.</summary>
    public static Scenario OpenField() => new(
        "Open Field",
        GameMap.FromRows(new[]
        {
            "PPPWPP",
            "PPFWPP",
            "PPFPPP",
            "PPPHPP",
            "PPPHPP"
        }),
        turnLimit: 30,
        spawns: new[]
        {
            new UnitSpawn("Infantry", Side.Blue, new Position(0, 1)),
            new UnitSpawn("Armor",    Side.Blue, new Position(1, 2)),
            new UnitSpawn("Recon",    Side.Blue, new Position(0, 3)),
            new UnitSpawn("Infantry", Side.Red,  new Position(5, 1)),
            new UnitSpawn("Armor",    Side.Red,  new Position(4, 2)),
            new UnitSpawn("Recon",    Side.Red,  new Position(5, 3)),
        });
}
