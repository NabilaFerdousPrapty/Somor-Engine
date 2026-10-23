namespace WarSim.Core.Domain;

/// <summary>Slow, good defence, gets 50% extra benefit from forest and hills.</summary>
public class Infantry : Unit
{
    public Infantry(int id, string name, Side side, Position position)
        : base(id, name, side, position, health: 100, movement: 3, attackRange: 1, attack: 20, defense: 10) { }

    public override double TerrainDefenceBonus(TerrainType terrain) => base.TerrainDefenceBonus(terrain) * 1.5;
}
