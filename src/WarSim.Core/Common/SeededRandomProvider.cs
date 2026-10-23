namespace WarSim.Core.Common;

public sealed class SeededRandomProvider : IRandomProvider
{
    private readonly Random _random;

    public SeededRandomProvider(int seed) => _random = new Random(seed);

    public double NextDouble() => _random.NextDouble();
}
