using WarSim.Core.AI;
using WarSim.Core.Common;
using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

/// <summary>Runs one full battle. Create a new engine (and random provider) per battle.</summary>
public class SimulationEngine
{
    private readonly IAiStrategy _blue;
    private readonly IAiStrategy _red;
    private readonly MovementService _movement = new();
    private readonly CombatResolver _combat;

    public SimulationEngine(IAiStrategy blue, IAiStrategy red, IRandomProvider random)
    {
        _blue = blue;
        _red = red;
        _combat = new CombatResolver(random);
    }

    public BattleResult Run(Scenario scenario)
    {
        var map = scenario.Map;
        var units = scenario.CreateUnits();
        var events = new List<BattleEvent>();
        var occupied = new HashSet<Position>(units.Select(u => u.Position));

        int blueStart = units.Count(u => u.Side == Side.Blue);
        int redStart = units.Count(u => u.Side == Side.Red);
        int turnsPlayed = 0;

        for (int turn = 1; turn <= scenario.TurnLimit; turn++)
        {
            turnsPlayed = turn;

            // Alternate who moves first so neither side always has the initiative.
            var order = turn % 2 == 1 ? new[] { Side.Blue, Side.Red } : new[] { Side.Red, Side.Blue };
            foreach (var side in order)
            {
                PlaySide(side, turn, map, units, occupied, events);
                units.RemoveAll(u => !u.IsAlive);
                if (IsOver(units)) break;
            }
            if (IsOver(units)) break;
        }

        int blueAlive = units.Count(u => u.Side == Side.Blue);
        int redAlive = units.Count(u => u.Side == Side.Red);

        Side? winner;
        if (redAlive == 0 && blueAlive > 0) winner = Side.Blue;
        else if (blueAlive == 0 && redAlive > 0) winner = Side.Red;
        else
        {
            int blueHealth = units.Where(u => u.Side == Side.Blue).Sum(u => u.Health);
            int redHealth = units.Where(u => u.Side == Side.Red).Sum(u => u.Health);
            winner = blueHealth > redHealth ? Side.Blue : redHealth > blueHealth ? Side.Red : null;
        }

        return new BattleResult(scenario.Name, _blue.Name, _red.Name, winner, turnsPlayed,
                                blueStart - blueAlive, redStart - redAlive, events);
    }

    private static bool IsOver(List<Unit> units) =>
        !units.Any(u => u.Side == Side.Blue) || !units.Any(u => u.Side == Side.Red);

    private void PlaySide(Side side, int turn, GameMap map, List<Unit> units,
                          HashSet<Position> occupied, List<BattleEvent> events)
    {
        var strategy = side == Side.Blue ? _blue : _red;
        var state = new BattleState(map, units, turn);

        foreach (var unit in units.Where(u => u.Side == side && u.IsAlive).OrderBy(u => u.Id).ToList())
        {
            if (!unit.IsAlive) continue;
            var order = strategy.Decide(unit, state);

            if (order.MoveTarget is Position target)
            {
                var from = unit.Position;
                int steps = _movement.MoveToward(unit, target, order.StopDistance, map, occupied);
                if (steps > 0)
                    events.Add(new BattleEvent(turn, "Move", unit.Name, null, null,
                        $"{unit.Name} moved {from} -> {unit.Position}"));
            }

            if (!order.Attack) continue;

            var victim = units
                .Where(u => u.IsAlive && _combat.CanAttack(unit, u))
                .OrderBy(u => u.Health).ThenBy(u => u.Id)
                .FirstOrDefault();

            if (victim is not null && _combat.TryAttack(unit, victim, map, out int damage))
            {
                events.Add(new BattleEvent(turn, "Attack", unit.Name, victim.Name, damage,
                    $"{unit.Name} hit {victim.Name} for {damage} (health {victim.Health})"));
                if (!victim.IsAlive)
                {
                    occupied.Remove(victim.Position);
                    events.Add(new BattleEvent(turn, "Death", victim.Name, null, null,
                        $"{victim.Name} was destroyed"));
                }
            }
        }
    }
}
