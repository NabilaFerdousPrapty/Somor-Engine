namespace WarSim.Core.Domain;

/// <summary>Very fast, long movement, weak attack.</summary>
public class Recon : Unit
{
    public Recon(int id, string name, Side side, Position position)
        : base(id, name, side, position, health: 60, movement: 7, attackRange: 2, attack: 12, defense: 4) { }
}
