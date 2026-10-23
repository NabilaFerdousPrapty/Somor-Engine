namespace WarSim.Core.Analysis;

public record MonteCarloSummary(
    string ScenarioName,
    int Runs,
    int BaseSeed,
    string BlueStrategy,
    string RedStrategy,
    int BlueWins,
    int RedWins,
    int Draws,
    double AvgBlueCasualties,
    double AvgRedCasualties,
    double AvgTurns,
    double StdDevTurns)
{
    public double BlueWinRate => Runs == 0 ? 0 : (double)BlueWins / Runs;
    public double RedWinRate => Runs == 0 ? 0 : (double)RedWins / Runs;
    public double DrawRate => Runs == 0 ? 0 : (double)Draws / Runs;
}
