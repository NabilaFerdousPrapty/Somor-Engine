namespace WarSim.Core.Domain;

/// <summary>Fast, strong attack, lower defence.</summary>
public class Armor : Unit
{
    public Armor(int id, string name, Side side, Position position)
        : base(id, name, side, position, health: 120, movement: 5, attackRange: 2, attack: 35, defense: 8) { }
}
