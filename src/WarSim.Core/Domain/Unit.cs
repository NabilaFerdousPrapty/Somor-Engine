namespace WarSim.Core.Domain;

public abstract class Unit
{
    public int Id { get; }
    public string Name { get; }
    public Side Side { get; }
    public Position Position { get; set; }
    public int Health { get; private set; }
    public int MaxHealth { get; }
    public int Movement { get; }
    public int AttackRange { get; }
    public int Attack { get; }
    public int Defense { get; }

    public bool IsAlive => Health > 0;

    protected Unit(int id, string name, Side side, Position position,
                   int health, int movement, int attackRange, int attack, int defense)
    {
        Id = id;
        Name = name;
        Side = side;
        Position = position;
        Health = health;
        MaxHealth = health;
        Movement = movement;
        AttackRange = attackRange;
        Attack = attack;
        Defense = defense;
    }

    /// <summary>Defence bonus this unit gets on a terrain. Subclasses may override.</summary>
    public virtual double TerrainDefenceBonus(TerrainType terrain) => terrain.DefenceBonus();

    public void TakeDamage(int damage) => Health = Math.Max(0, Health - damage);
}
