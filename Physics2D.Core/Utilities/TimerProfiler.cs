using System.Diagnostics;

namespace Physics2D.Core.Utilities;

/// <summary>
/// Utility for measuring per-frame duration and reporting rolling averages for evaluation.
/// </summary>
public sealed class TimerProfiler
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Queue<double> _samples = new();
    private readonly int _maxSamples;

    public TimerProfiler(int maxSamples = 300)
    {
        _maxSamples = maxSamples;
    }

    public double LastMilliseconds { get; private set; }
    public double AverageMilliseconds => _samples.Count == 0 ? 0d : _samples.Average();

    public void Begin() => _stopwatch.Restart();

    public void End()
    {
        _stopwatch.Stop();
        LastMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;
        _samples.Enqueue(LastMilliseconds);

        while (_samples.Count > _maxSamples)
        {
            _samples.Dequeue();
        }
    }
}
