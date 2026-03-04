using Physics2D.Core.Collision;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;
using Physics2D.Core.Utilities;

namespace Physics2D.DemoConsole;

/// <summary>
/// Console demo and evaluation harness for the custom 2D physics engine.
/// </summary>
internal static class Program
{
    private static void Main()
    {
        RunMainDemo();
        RunDropTest();
        RunStackTest();
        RunStressTests();
    }

    private static void RunMainDemo()
    {
        Console.WriteLine("=== 10-Second Demo Simulation ===");
        var world = new World(new Vec2(0f, -20f), gridCellSize: 1.5f, solverIterations: 10);
        AddGround(world, y: -5f, width: 60f);

        for (var i = 0; i < 12; i++)
        {
            var body = new Body(new Vec2(-4f + i * 0.75f, 6f + i * 0.5f), mass: 1f);
            var collider = new PhysicsBoxCollider(body, new Vec2(0.9f, 0.9f), Vec2.Zero);
            world.AddBody(body, collider);
        }

        SimulateAndReport(world, seconds: 10f, printSamplePositions: true);
    }

    private static void RunDropTest()
    {
        Console.WriteLine("\n=== Drop Test ===");
        var world = new World(new Vec2(0f, -9.81f));
        AddGround(world, y: -2f, width: 20f);

        for (var i = 0; i < 20; i++)
        {
            var body = new Body(new Vec2(0f, 2f + i));
            world.AddBody(body, new PhysicsBoxCollider(body, new Vec2(0.8f, 0.8f), Vec2.Zero));
        }

        SimulateAndReport(world, seconds: 5f, printSamplePositions: false);
    }

    private static void RunStackTest()
    {
        Console.WriteLine("\n=== Stack Test ===");
        var world = new World(new Vec2(0f, -9.81f));
        AddGround(world, y: -3f, width: 30f);

        for (var x = -3; x <= 3; x++)
        {
            for (var y = 0; y < 12; y++)
            {
                var body = new Body(new Vec2(x * 1.1f, -2f + y * 1.0f));
                world.AddBody(body, new PhysicsBoxCollider(body, new Vec2(1f, 1f), Vec2.Zero));
            }
        }

        SimulateAndReport(world, seconds: 8f, printSamplePositions: false);
    }

    private static void RunStressTests()
    {
        Console.WriteLine("\n=== Stress Test + Broadphase Comparison ===");
        foreach (var count in new[] { 100, 500, 1000 })
        {
            var world = new World(new Vec2(0f, -9.81f), gridCellSize: 1.5f, solverIterations: 6);
            AddGround(world, y: -4f, width: 200f);

            var random = new Random(1234 + count);
            for (var i = 0; i < count; i++)
            {
                var body = new Body(new Vec2(random.NextSingle() * 40f - 20f, random.NextSingle() * 40f));
                world.AddBody(body, new PhysicsBoxCollider(body, new Vec2(0.8f, 0.8f), Vec2.Zero));
            }

            var profiler = new TimerProfiler(120);
            const float dt = 1f / 60f;
            const int frames = 120;

            for (var frame = 0; frame < frames; frame++)
            {
                profiler.Begin();
                world.Step(dt);
                profiler.End();
            }

            var naivePairs = CountNaivePairs(world.Colliders);
            world.Grid.Clear();
            foreach (var c in world.Colliders) world.Grid.Insert(c);
            var gridPairs = world.Grid.QueryPairs().Count();

            Console.WriteLine($"Bodies={count,4} | AvgStep={profiler.AverageMilliseconds,8:0.000} ms | NaivePairs={naivePairs,8} | GridPairs={gridPairs,8}");
        }
    }

    private static int CountNaivePairs(IReadOnlyList<PhysicsCollider> colliders)
    {
        var pairs = 0;
        for (var i = 0; i < colliders.Count; i++)
        {
            for (var j = i + 1; j < colliders.Count; j++)
            {
                pairs++;
            }
        }

        return pairs;
    }

    private static void SimulateAndReport(World world, float seconds, bool printSamplePositions)
    {
        var profiler = new TimerProfiler();
        const float dt = 1f / 60f;
        var steps = (int)(seconds / dt);

        for (var i = 0; i < steps; i++)
        {
            profiler.Begin();
            world.Step(dt);
            profiler.End();
        }

        if (printSamplePositions)
        {
            foreach (var body in world.Bodies.Where(b => !b.IsStatic).Take(6))
            {
                Console.WriteLine($"Body at {body.Position}");
            }
        }

        Console.WriteLine($"Frames: {steps}, Avg Step: {profiler.AverageMilliseconds:0.000} ms");
    }

    private static void AddGround(World world, float y, float width)
    {
        var ground = new Body(new Vec2(0f, y), isStatic: true);
        world.AddBody(ground, new PhysicsBoxCollider(ground, new Vec2(width, 1f), Vec2.Zero));
    }
}
