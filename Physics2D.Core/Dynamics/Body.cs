using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Rigid body particle model (translation only) using semi-implicit Euler integration.
/// </summary>
public sealed class Body
{
    public Vec2 Position;
    public Vec2 Velocity;
    public Vec2 Force;

    public float Mass;
    public float InverseMass;
    public bool IsStatic;
    public float Restitution;
    public float Friction;

    public Body(Vec2 position, float mass = 1f, bool isStatic = false)
    {
        Position = position;
        Velocity = Vec2.Zero;
        Force = Vec2.Zero;
        IsStatic = isStatic;
        Restitution = 0.05f;
        Friction = 0.8f;

        if (isStatic || mass <= 0f)
        {
            Mass = float.PositiveInfinity;
            InverseMass = 0f;
            IsStatic = true;
        }
        else
        {
            Mass = mass;
            InverseMass = 1f / mass;
        }
    }

    public void ApplyForce(in Vec2 force)
    {
        if (IsStatic)
        {
            return;
        }

        Force = Force + force;
    }

    public void IntegrateForces(float dt, in Vec2 gravity)
    {
        if (IsStatic)
        {
            return;
        }

        var acceleration = gravity + (Force * InverseMass);
        Velocity = Velocity + (acceleration * dt);
    }

    public void IntegrateVelocity(float dt)
    {
        if (IsStatic)
        {
            return;
        }

        Position = Position + (Velocity * dt);
    }

    public void ClearForces()
    {
        Force = Vec2.Zero;
    }
}
