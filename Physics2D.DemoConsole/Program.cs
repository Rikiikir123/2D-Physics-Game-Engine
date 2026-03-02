using Physics2D.Core.DemoConsole;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using Physics2D.Core.Utilities;

namespace Physics2D.DemoConsole;

internal static class Program
{
    private static void Main()
    {
        const float dt = 1f / 60f;
        const float simSeconds = 10f;

        var world = new World(new Vec2(0f, -9.81f), gridCellSize: 1.25f, solverIterations: 8);

        // Ground
        var ground = new Body(new Vec2(0f, -1f), isStatic: true);
        world.AddBody(ground, new Physics2D.Core.Collision.BoxCollider(ground, new Vec2(100f, 2f), Vec2.Zero));

        // Falling boxes
        for (var i = 0; i < 20; i++)
        {
            var body = new Body(new Vec2(-5f + (i % 5) * 2f, 2f + (i / 5) * 2f), 1f, false);
            world.AddBody(body, new Physics2D.Core.Collision.BoxCollider(body, new Vec2(1f, 1f), Vec2.Zero));
        }

        var profiler = new TimerProfiler();
        var totalSteps = (int)(simSeconds / dt);

        for (var i = 0; i < totalSteps; i++)
        {
            profiler.Begin();
            world.Step(dt);
            profiler.End();
        }

        Console.WriteLine("=== 10 Second Simulation Complete ===");
        foreach (var body in world.Bodies.Where(b => !b.IsStatic).Take(10))
        {
            Console.WriteLine($"Body at {body.Position}");
        }

        Console.WriteLine($"Average step time: {profiler.AverageMilliseconds:0.####} ms");
        Console.WriteLine();

        ProgramScenarios.RunAllScenarios();
    }
}
