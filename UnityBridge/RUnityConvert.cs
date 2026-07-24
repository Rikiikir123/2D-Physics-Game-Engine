using Engine.Math;
using UnityEngine;

/// <summary>
/// Converts between engine space (Y-down) and Unity space (Y-up).
/// Engine +Y is down; Unity +Y is up. X is the same.
/// </summary>
public static class RUnityConvert
{
    public static Vector3 ToUnity(RVector2 enginePos, float z = 0f)
    {
        return new Vector3(enginePos.X, -enginePos.Y, z);
    }

    public static RVector2 ToEngine(Vector3 unityPos)
    {
        return new RVector2(unityPos.x, -unityPos.y);
    }

    public static Vector2 ToUnityVelocity(RVector2 engineVel)
    {
        return new Vector2(engineVel.X, -engineVel.Y);
    }

    public static RVector2 ToEngineVelocity(Vector2 unityVel)
    {
        return new RVector2(unityVel.x, -unityVel.y);
    }
}
