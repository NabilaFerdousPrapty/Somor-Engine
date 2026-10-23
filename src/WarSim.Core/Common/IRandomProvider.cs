namespace WarSim.Core.Common;

/// <summary>All randomness in the engine goes through this interface so battles are reproducible.</summary>
public interface IRandomProvider
{
    /// <summary>Returns a number in [0.0, 1.0).</summary>
    double NextDouble();
}
