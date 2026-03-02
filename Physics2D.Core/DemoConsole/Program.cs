using Physics2D.Core.Broadphase;
using Physics2D.Core.Collision;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using Physics2D.Core.Utilities;

namespace Physics2D.Core.DemoConsole;

/// <summary>
/// Evaluation scenarios used by console entrypoint for academic measurements.
/// </summary>
public static class ProgramScenarios
{
    public static void RunAllScenarios()
    {
        RunDropTest();
        RunStackTest();
        RunStressTests();
        CompareBroadphaseMethods();
    }

    public static void RunDropTest()
    {
        var world = new World(new Vec2(0f, -20f), 1f);
        BuildGround(world);

        var falling = SpawnBox(world, new Vec2(0f, 8f), new Vec2(1f, 1f), mass: 1f, isStatic: false);
        Simulate(world, 2f, 1f / 60f);

        Console.WriteLine($"[DropTest] Final position: {falling.Position}");
    }

    public static void RunStackTest()
    {
        var world = new World(new Vec2(0f, -20f), 1f);
        BuildGround(world);

        for (var i = 0; i < 10; i++)
        {
            SpawnBox(world, new Vec2(0f, i + 1f), new Vec2(1f, 1f), 1f, false);
        }

        Simulate(world, 4f, 1f / 60f);
        Console.WriteLine($"[StackTest] Simulated {world.Bodies.Count} bodies.");
    }

    public static void RunStressTests()
    {
        foreach (var bodyCount in new[] { 100, 500, 1000 })
        {
            var world = new World(new Vec2(0f, -9.81f), 1f);
            BuildGround(world);

            var side = (int)MathF.Ceiling(MathF.Sqrt(bodyCount));
            var spawned = 0;
            for (var y = 0; y < side && spawned < bodyCount; y++)
            {
                for (var x = 0; x < side && spawned < bodyCount; x++)
                {
                    SpawnBox(world, new Vec2(-10f + x * 1.1f, 2f + y * 1.1f), new Vec2(1f, 1f), 1f, false);
                    spawned++;
                }
            }

            var profiler = new TimerProfiler();
            const float dt = 1f / 60f;
            for (var i = 0; i < 120; i++)
            {
                profiler.Begin();
                world.Step(dt);
                profiler.End();
            }

            Console.WriteLine($"[Stress {bodyCount}] avg step: {profiler.AverageMilliseconds:0.###} ms");
        }
    }

    public static void CompareBroadphaseMethods()
    {
        var world = new World(new Vec2(0f, -9.81f), 1f);
        BuildGround(world);

        for (var i = 0; i < 400; i++)
        {
            SpawnBox(world, new Vec2((i % 20) * 1.1f, 2f + (i / 20) * 1.1f), new Vec2(1f, 1f), 1f, false);
        }

        var gridProfiler = new TimerProfiler();
        var naiveProfiler = new TimerProfiler();
        var grid = world.Grid;

        for (var frame = 0; frame < 60; frame++)
        {
            grid.Clear();
            foreach (var collider in world.Colliders)
            {
                grid.Insert(collider);
            }

            gridProfiler.Begin();
            _ = grid.QueryPairs();
            gridProfiler.End();

            naiveProfiler.Begin();
            _ = grid.QueryPairsNaive(world.Colliders);
            naiveProfiler.End();
        }

        Console.WriteLine($"[Broadphase] Grid avg query: {gridProfiler.AverageMilliseconds:0.###} ms");
        Console.WriteLine($"[Broadphase] Naive avg query: {naiveProfiler.AverageMilliseconds:0.###} ms");
    }

    public static void Simulate(World world, float seconds, float dt)
    {
        var steps = (int)(seconds / dt);
        for (var i = 0; i < steps; i++)
        {
            world.Step(dt);
        }
    }

    private static Body SpawnBox(World world, Vec2 position, Vec2 size, float mass, bool isStatic)
    {
        var body = new Body(position, mass, isStatic);
        var collider = new BoxCollider(body, size, Vec2.Zero);
        world.AddBody(body, collider);
        return body;
    }

    private static void BuildGround(World world)
    {
        SpawnBox(world, new Vec2(0f, -1f), new Vec2(100f, 2f), 1f, true);
    }
}
