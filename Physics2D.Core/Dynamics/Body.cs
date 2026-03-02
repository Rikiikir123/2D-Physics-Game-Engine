using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Rigid body state with translational dynamics only.
/// </summary>
public sealed class Body
{
    private float _mass;
    private bool _isStatic;

    public Vec2 Position;
    public Vec2 Velocity;
    public Vec2 Force;

    public Body(Vec2 position, float mass = 1f, bool isStatic = false)
    {
        Position = position;
        Restitution = 0.1f;
        Friction = 0.6f;

        _mass = 1f;
        Mass = mass;
        IsStatic = isStatic;
    }

    public float Mass
    {
        get => _mass;
        set
        {
            _mass = value <= 0f ? 1f : value;
            InverseMass = _isStatic ? 0f : 1f / _mass;
        }
    }

    public float InverseMass { get; private set; }

    public bool IsStatic
    {
        get => _isStatic;
        set
        {
            _isStatic = value;
            InverseMass = _isStatic ? 0f : 1f / _mass;
        }
    }

    public float Restitution { get; set; }
    public float Friction { get; set; }

    public void ApplyForce(in Vec2 force)
    {
        if (IsStatic) return;
        Force += force;
    }

    /// <summary>
    /// Semi-implicit Euler force integration: velocity is updated from acceleration.
    /// </summary>
    public void IntegrateForces(float dt, in Vec2 gravity)
    {
        if (IsStatic) return;
        var acceleration = gravity + (Force * InverseMass);
        Velocity += acceleration * dt;
    }

    /// <summary>
    /// Semi-implicit Euler second stage: position updated from new velocity.
    /// </summary>
    public void IntegrateVelocity(float dt)
    {
        if (IsStatic) return;
        Position += Velocity * dt;
    }

    public void ClearForces() => Force = Vec2.Zero;
}
