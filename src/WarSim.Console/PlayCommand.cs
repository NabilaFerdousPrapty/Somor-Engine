using WarSim.Core.AI;
using WarSim.Core.Common;
using WarSim.Core.Domain;
using WarSim.Core.Engine;

namespace WarSim.App;

public static class PlayCommand
{
    /// <summary>
    /// Interactive battle. You command Blue: change its orders before any turn, then play the turn.
    /// Works with single key presses in a normal terminal; if keys are not available
    /// (for example the VS Code Debug Console) it falls back to typing a letter and pressing Enter.
    /// Returns null if you quit before the battle ends.
    /// </summary>
    public static BattleResult? Run(Scenario scenario, IAiStrategy startingBlue, IAiStrategy red, int seed)
    {
        var blue = new SwitchableStrategy(startingBlue);
        var engine = new SimulationEngine(blue, red, new SeededRandomProvider(seed));
        engine.Start(scenario);

        bool keyMode = !Console.IsInputRedirected;

        while (true)
        {
            ClearScreen();
            ConsoleRenderer.DrawLive(scenario, engine);
            Console.WriteLine();

            if (engine.IsFinished)
            {
                Console.WriteLine("Battle over.");
                if (keyMode)
                {
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey(true);
                }
                return engine.BuildResult();
            }

            Console.WriteLine($"You command Blue. Current orders: {blue.Name}");
            Console.WriteLine("[A] Aggressive   [D] Defensive   [B] Balanced   [Enter] play next turn   [F] fast-forward   [Q] quit");
            if (!keyMode) Console.Write("Type a letter, then press Enter: ");

            string? command = ReadCommand(keyMode);
            if (command is null) return null;

            switch (command)
            {
                case "a": SetOrders(blue, engine, "Aggressive"); break;
                case "d": SetOrders(blue, engine, "Defensive"); break;
                case "b": SetOrders(blue, engine, "Balanced"); break;
                case "": case "n": engine.PlayTurn(); break;
                case "f": while (!engine.IsFinished) engine.PlayTurn(); break;
                case "q": return null;
            }
        }
    }

    private static string? ReadCommand(bool keyMode)
    {
        if (keyMode)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Spacebar) return "";
            if (key.Key == ConsoleKey.Escape) return "q";
            return key.KeyChar.ToString().ToLowerInvariant();
        }

        string? line = Console.ReadLine();
        return line?.Trim().ToLowerInvariant();
    }

    private static void ClearScreen()
    {
        try
        {
            if (!Console.IsOutputRedirected) Console.Clear();
        }
        catch (IOException)
        {
            // Some terminals cannot be cleared; just keep printing.
        }
    }

    private static void SetOrders(SwitchableStrategy blue, SimulationEngine engine, string strategyName)
    {
        if (blue.Name == strategyName) return;
        blue.Current = StrategyFactory.Create(strategyName);
        engine.AddNote($"Blue commander changed orders to {strategyName}");
    }
}