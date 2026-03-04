using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Conversion helpers between Unity's vectors and core engine vectors.
/// </summary>
public static class Converters
{
    public static Vec2 ToCore(this Vector2 value) => new(value.x, value.y);
    public static Vector2 ToUnity(this Vec2 value) => new(value.X, value.Y);
}
