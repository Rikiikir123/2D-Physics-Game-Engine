using Physics2D.Core.Math;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Converts between Unity vectors and core physics vectors.
/// </summary>
public static class Converters
{
    public static Vec2 ToCore(this Vector2 v) => new(v.x, v.y);
    public static Vector2 ToUnity(this Vec2 v) => new(v.X, v.Y);
}
