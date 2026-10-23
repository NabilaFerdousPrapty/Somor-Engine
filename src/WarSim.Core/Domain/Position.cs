namespace WarSim.Core.Domain;

public readonly record struct Position(int X, int Y)
{
    /// <summary>Manhattan distance (grid moves are 4-directional).</summary>
    public int DistanceTo(Position other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public override string ToString() => $"({X},{Y})";
}
