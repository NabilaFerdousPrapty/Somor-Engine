using MySqlConnector;
using WarSim.Core.Analysis;
using WarSim.Core.Engine;

namespace WarSim.Persistence;

/// <summary>All MySQL code lives here. Plain ADO.NET, parameterised queries only.</summary>
public class MySqlRunRepository : IRunRepository
{
    private readonly string _connectionString;

    public MySqlRunRepository(string connectionString) => _connectionString = connectionString;

    public string Name => "MySQL";

    /// <summary>Throws if the database cannot be reached.</summary>
    public void CheckConnection()
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
    }

    public int SaveRun(BattleResult result, int seed, DateTime startedAt)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        long runId;
        using (var cmd = new MySqlCommand(
            @"INSERT INTO simulation_runs
                (scenario_name, seed, blue_strategy, red_strategy, winner, turns_played, started_at)
              VALUES (@scenario, @seed, @blue, @red, @winner, @turns, @started)", conn, tx))
        {
            cmd.Parameters.AddWithValue("@scenario", result.ScenarioName);
            cmd.Parameters.AddWithValue("@seed", seed);
            cmd.Parameters.AddWithValue("@blue", result.BlueStrategy);
            cmd.Parameters.AddWithValue("@red", result.RedStrategy);
            cmd.Parameters.AddWithValue("@winner", result.Winner?.ToString() ?? "Draw");
            cmd.Parameters.AddWithValue("@turns", result.TurnsPlayed);
            cmd.Parameters.AddWithValue("@started", startedAt);
            cmd.ExecuteNonQuery();
            runId = cmd.LastInsertedId;
        }

        foreach (var e in result.Events)
        {
            using var cmd = new MySqlCommand(
                @"INSERT INTO battle_events
                    (run_id, turn, event_type, unit_name, target_name, damage, description)
                  VALUES (@run, @turn, @type, @unit, @target, @damage, @desc)", conn, tx);
            cmd.Parameters.AddWithValue("@run", runId);
            cmd.Parameters.AddWithValue("@turn", e.Turn);
            cmd.Parameters.AddWithValue("@type", e.EventType);
            cmd.Parameters.AddWithValue("@unit", e.UnitName);
            cmd.Parameters.AddWithValue("@target", (object?)e.TargetName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@damage", (object?)e.Damage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@desc",
                e.Description.Length > 200 ? e.Description[..200] : e.Description);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return (int)runId;
    }

    public void SaveMonteCarlo(MonteCarloSummary s, DateTime createdAt)
    {
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            @"INSERT INTO monte_carlo_batches
                (scenario_name, runs, base_seed, blue_strategy, red_strategy,
                 blue_wins, red_wins, draws, avg_turns, created_at)
              VALUES (@scenario, @runs, @seed, @blue, @red, @bw, @rw, @draws, @avg, @created)", conn);
        cmd.Parameters.AddWithValue("@scenario", s.ScenarioName);
        cmd.Parameters.AddWithValue("@runs", s.Runs);
        cmd.Parameters.AddWithValue("@seed", s.BaseSeed);
        cmd.Parameters.AddWithValue("@blue", s.BlueStrategy);
        cmd.Parameters.AddWithValue("@red", s.RedStrategy);
        cmd.Parameters.AddWithValue("@bw", s.BlueWins);
        cmd.Parameters.AddWithValue("@rw", s.RedWins);
        cmd.Parameters.AddWithValue("@draws", s.Draws);
        cmd.Parameters.AddWithValue("@avg", s.AvgTurns);
        cmd.Parameters.AddWithValue("@created", createdAt);
        cmd.ExecuteNonQuery();
    }

    public IReadOnlyList<SavedRun> ListRuns(int limit)
    {
        var runs = new List<SavedRun>();
        using var conn = new MySqlConnection(_connectionString);
        conn.Open();
        using var cmd = new MySqlCommand(
            @"SELECT id, scenario_name, seed, blue_strategy, red_strategy, winner, turns_played, started_at
              FROM simulation_runs ORDER BY started_at DESC, id DESC LIMIT @limit", conn);
        cmd.Parameters.AddWithValue("@limit", limit);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            runs.Add(new SavedRun(
                reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5),
                reader.GetInt32(6), reader.GetDateTime(7)));
        }
        return runs;
    }
}
