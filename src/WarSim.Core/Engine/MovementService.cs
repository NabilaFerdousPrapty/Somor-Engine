using WarSim.Core.Domain;

namespace WarSim.Core.Engine;

public class MovementService
{
    private static readonly (int dx, int dy)[] Directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    /// <summary>
    /// Greedy movement: each step must reduce the distance to the target. Of the (up to two) steps that do,
    /// the cheaper one wins. Stops when out of movement points, blocked, or within stopDistance.
    /// Returns the number of tiles moved.
    /// </summary>
    public int MoveToward(Unit unit, Position target, int stopDistance, GameMap map, ISet<Position> occupied)
    {
        int points = unit.Movement;
        int steps = 0;

        while (unit.Position.DistanceTo(target) > stopDistance)
        {
            var current = unit.Position;
            int currentDist = current.DistanceTo(target);
            Position? best = null;
            int bestDist = int.MaxValue;
            int bestCost = int.MaxValue;

            foreach (var (dx, dy) in Directions)
            {
                var next = new Position(current.X + dx, current.Y + dy);
                if (!map.InBounds(next)) continue;
                var terrain = map.TerrainAt(next);
                if (!terrain.IsPassable()) continue;
                if (occupied.Contains(next)) continue;
                int cost = terrain.MovementCost();
                if (cost > points) continue;
                int dist = next.DistanceTo(target);
                if (dist >= currentDist) continue;

                if (best is null || dist < bestDist || (dist == bestDist && cost < bestCost))
                {
                    best = next;
                    bestDist = dist;
                    bestCost = cost;
                }
            }

            if (best is null) break;

            occupied.Remove(current);
            occupied.Add(best.Value);
            unit.Position = best.Value;
            points -= bestCost;
            steps++;
        }

        return steps;
    }
}
