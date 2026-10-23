namespace WarSim.Core.Domain;

/// <summary>Factory pattern: creates a unit from a type name (also used by the JSON loader later).</summary>
public static class UnitFactory
{
    public static Unit Create(string type, int id, Side side, Position position)
    {
        switch (type.Trim().ToLowerInvariant())
        {
            case "infantry": return new Infantry(id, $"{side} Infantry {id}", side, position);
            case "armor": return new Armor(id, $"{side} Armor {id}", side, position);
            case "recon": return new Recon(id, $"{side} Recon {id}", side, position);
            default: throw new ArgumentException($"Unknown unit type '{type}'. Use Infantry, Armor or Recon.");
        }
    }
}
