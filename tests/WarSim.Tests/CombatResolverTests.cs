using WarSim.Core.Domain;
using WarSim.Core.Engine;
using Xunit;

namespace WarSim.Tests;

public class CombatResolverTests
{
    [Fact]
    public void Out_of_range_attack_is_rejected()
    {
        var map = GameMap.FromRows(new[] { "PPPPPPPPPP" });
        var attacker = new Armor(1, "A", Side.Blue, new Position(0, 0));   // range 2
        var defender = new Infantry(2, "D", Side.Red, new Position(5, 0));
        var resolver = new CombatResolver(new FixedRandom());

        bool attacked = resolver.TryAttack(attacker, defender, map, out int damage);

        Assert.False(attacked);
        Assert.Equal(0, damage);
        Assert.Equal(100, defender.Health);
    }

    [Fact]
    public void Terrain_defence_bonus_reduces_damage()
    {
        var map = GameMap.FromRows(new[] { "PH" });
        var armor = new Armor(1, "A", Side.Blue, new Position(1, 0));
        var recon = new Recon(2, "R", Side.Red, new Position(0, 0));       // on plains
        var resolver = new CombatResolver(new FixedRandom());

        int onPlains = resolver.CalculateDamage(armor, recon, map);

        armor.Position = new Position(0, 0);
        recon.Position = new Position(1, 0);                                // now on hills
        int onHills = resolver.CalculateDamage(armor, recon, map);

        Assert.True(onHills < onPlains, $"hills {onHills} should be less than plains {onPlains}");
    }

    [Fact]
    public void Damage_is_at_least_one()
    {
        var map = GameMap.FromRows(new[] { "PH" });
        var recon = new Recon(1, "R", Side.Blue, new Position(0, 0));       // attack 12
        var infantry = new Infantry(2, "I", Side.Red, new Position(1, 0));  // hills: defence 10 * 1.6 = 16
        var resolver = new CombatResolver(new FixedRandom());

        Assert.Equal(1, resolver.CalculateDamage(recon, infantry, map));
    }
}
