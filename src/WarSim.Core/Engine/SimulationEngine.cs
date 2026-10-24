using WarSim.Core.AI;
using WarSim.Core.Common;
using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

/// <summary>
/// Runs one battle. Use Run() for a whole battle, or Start() then PlayTurn() one turn at a time
/// (the interactive 'play' command does this). Create a new engine (and random provider) per battle.
/// </summary>
public class SimulationEngine
{
    private readonly IAiStrategy _blue;
    private readonly IAiStrategy _red;
    private readonly MovementService _movement = new();
    private readonly CombatResolver _combat;

    private Scenario _scenario = null!;
    private List<Unit> _units = new();
    private List<BattleEvent> _events = new();
    private HashSet<Position> _occupied = new();
    private List<TurnSnapshot> _snapshots = new();
    private int _blueStart;
    private int _redStart;
    private int _turn;

    public bool IsFinished { get; private set; }

    public SimulationEngine(IAiStrategy blue, IAiStrategy red, IRandomProvider random)
    {
        _blue = blue;
        _red = red;
        _combat = new CombatResolver(random);
    }

    /// <summary>Plays a whole battle and returns the result.</summary>
    public BattleResult Run(Scenario scenario)
    {
        Start(scenario);
        while (!IsFinished) PlayTurn();
        return BuildResult();
    }

    public void Start(Scenario scenario)
    {
        _scenario = scenario;
        _units = scenario.CreateUnits();
        _events = new List<BattleEvent>();
        _occupied = new HashSet<Position>(_units.Select(u => u.Position));
        _snapshots = new List<TurnSnapshot> { Snapshot(0, _units) };
        _blueStart = _units.Count(u => u.Side == Side.Blue);
        _redStart = _units.Count(u => u.Side == Side.Red);
        _turn = 0;
        IsFinished = scenario.TurnLimit <= 0;
    }

    public void PlayTurn()
    {
        if (IsFinished) return;

        int turn = ++_turn;

        // Alternate who moves first so neither side always has the initiative.
        var order = turn % 2 == 1 ? new[] { Side.Blue, Side.Red } : new[] { Side.Red, Side.Blue };
        foreach (var side in order)
        {
            PlaySide(side, turn);
            _units.RemoveAll(u => !u.IsAlive);
            if (IsOver()) break;
        }

        _snapshots.Add(Snapshot(turn, _units));
        if (IsOver() || turn >= _scenario.TurnLimit) IsFinished = true;
    }

    /// <summary>Adds a note (for example a player order change) to the event log for the next turn.</summary>
    public void AddNote(string description) =>
        _events.Add(new BattleEvent(_turn + 1, "Order", "Commander", null, null, description));

    /// <summary>Result so far. If the battle is not finished, the winner is whoever has more health right now.</summary>
    public BattleResult BuildResult()
    {
        int blueAlive = _units.Count(u => u.Side == Side.Blue);
        int redAlive = _units.Count(u => u.Side == Side.Red);

        Side? winner;
        if (redAlive == 0 && blueAlive > 0) winner = Side.Blue;
        else if (blueAlive == 0 && redAlive > 0) winner = Side.Red;
        else
        {
            int blueHealth = _units.Where(u => u.Side == Side.Blue).Sum(u => u.Health);
            int redHealth = _units.Where(u => u.Side == Side.Red).Sum(u => u.Health);
            winner = blueHealth > redHealth ? Side.Blue : redHealth > blueHealth ? Side.Red : null;
        }

        return new BattleResult(_scenario.Name, _blue.Name, _red.Name, winner, _turn,
                                _blueStart - blueAlive, _redStart - redAlive,
                                _events.ToList(), _snapshots.ToList());
    }

    private static TurnSnapshot Snapshot(int turn, List<Unit> units) =>
        new(turn, units
            .Select(u => new UnitSnapshot(u.Name, u.Side, u.GetType().Name, u.Position, u.Health, u.MaxHealth))
            .ToList());

    private bool IsOver() =>
        !_units.Any(u => u.Side == Side.Blue) || !_units.Any(u => u.Side == Side.Red);

    private void PlaySide(Side side, int turn)
    {
        var map = _scenario.Map;
        var strategy = side == Side.Blue ? _blue : _red;
        var state = new BattleState(map, _units, turn);

        foreach (var unit in _units.Where(u => u.Side == side && u.IsAlive).OrderBy(u => u.Id).ToList())
        {
            if (!unit.IsAlive) continue;
            var order = strategy.Decide(unit, state);

            if (order.MoveTarget is Position target)
            {
                var from = unit.Position;
                int steps = _movement.MoveToward(unit, target, order.StopDistance, map, _occupied);
                if (steps > 0)
                    _events.Add(new BattleEvent(turn, "Move", unit.Name, null, null,
                        $"{unit.Name} moved {from} -> {unit.Position}"));
            }

            if (!order.Attack) continue;

            var victim = _units
                .Where(u => u.IsAlive && _combat.CanAttack(unit, u))
                .OrderBy(u => u.Health).ThenBy(u => u.Id)
                .FirstOrDefault();

            if (victim is not null && _combat.TryAttack(unit, victim, map, out int damage))
            {
                _events.Add(new BattleEvent(turn, "Attack", unit.Name, victim.Name, damage,
                    $"{unit.Name} hit {victim.Name} for {damage} (health {victim.Health})"));
                if (!victim.IsAlive)
                {
                    _occupied.Remove(victim.Position);
                    _events.Add(new BattleEvent(turn, "Death", victim.Name, null, null,
                        $"{victim.Name} was destroyed"));
                }
            }
        }
    }
}