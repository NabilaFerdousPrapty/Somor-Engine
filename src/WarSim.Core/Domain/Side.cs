namespace WarSim.Core.Domain;

public enum Side { Blue, Red }

public static class SideExtensions
{
    public static Side Opponent(this Side side) => side == Side.Blue ? Side.Red : Side.Blue;
}
