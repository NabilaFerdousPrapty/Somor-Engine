using WarSim.Core.Common;

namespace WarSim.Tests;

/// <summary>Always returns the same value; 0.5 makes the combat random factor exactly 1.0.</summary>
internal sealed class FixedRandom : IRandomProvider
{
    private readonly double _value;
    public FixedRandom(double value = 0.5) => _value = value;
    public double NextDouble() => _value;
}
