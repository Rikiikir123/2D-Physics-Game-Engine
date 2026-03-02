using System.Diagnostics;

namespace Physics2D.Core.Utilities;

/// <summary>
/// Captures execution time samples and reports aggregate metrics.
/// </summary>
public sealed class TimerProfiler
{
    private readonly Stopwatch _watch = new();
    private readonly Queue<double> _samples = new();

    public TimerProfiler(int maxSamples = 600)
    {
        MaxSamples = maxSamples;
    }

    public int MaxSamples { get; }

    public double LastMilliseconds { get; private set; }
    public double AverageMilliseconds => _samples.Count == 0 ? 0d : _samples.Average();

    public void Begin() => _watch.Restart();

    public void End()
    {
        _watch.Stop();
        LastMilliseconds = _watch.Elapsed.TotalMilliseconds;
        _samples.Enqueue(LastMilliseconds);
        while (_samples.Count > MaxSamples)
        {
            _samples.Dequeue();
        }
    }
}
