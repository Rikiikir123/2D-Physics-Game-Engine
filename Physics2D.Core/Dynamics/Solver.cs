using Physics2D.Core.Collision;
using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Iterative impulse solver with positional correction and Coulomb-like friction.
/// </summary>
public sealed class Solver
{
    private readonly float _positionPercent;
    private readonly float _positionSlop;

    public Solver(int iterations = 8, float positionPercent = 0.8f, float positionSlop = 0.001f)
    {
        Iterations = iterations;
        _positionPercent = positionPercent;
        _positionSlop = positionSlop;
    }

    public int Iterations { get; set; }

    public void SolveContacts(IReadOnlyList<Contact> contacts)
    {
        for (var i = 0; i < Iterations; i++)
        {
            foreach (var contact in contacts)
            {
                ResolveVelocity(contact);
                PositionalCorrection(contact);
            }
        }
    }

    private void ResolveVelocity(Contact contact)
    {
        var a = contact.A;
        var b = contact.B;

        var rv = b.Velocity - a.Velocity;
        var velAlongNormal = Vec2.Dot(rv, contact.Normal);

        if (velAlongNormal > 0f)
        {
            return;
        }

        var invMassSum = a.InverseMass + b.InverseMass;
        if (invMassSum <= 0f)
        {
            return;
        }

        var restitution = MathF.Min(a.Restitution, b.Restitution);
        var j = -(1f + restitution) * velAlongNormal;
        j /= invMassSum;

        var impulse = contact.Normal * j;
        if (!a.IsStatic)
        {
            a.Velocity -= impulse * a.InverseMass;
        }

        if (!b.IsStatic)
        {
            b.Velocity += impulse * b.InverseMass;
        }

        // Friction impulse in tangent direction.
        rv = b.Velocity - a.Velocity;
        var tangent = rv - (contact.Normal * Vec2.Dot(rv, contact.Normal));
        tangent = tangent.Normalize();

        var jt = -Vec2.Dot(rv, tangent);
        jt /= invMassSum;

        var staticFriction = MathF.Sqrt(a.Friction * b.Friction);
        Vec2 frictionImpulse;
        if (MathF.Abs(jt) < j * staticFriction)
        {
            frictionImpulse = tangent * jt;
        }
        else
        {
            var dynamicFriction = staticFriction * 0.8f;
            frictionImpulse = tangent * (-j * dynamicFriction);
        }

        if (!a.IsStatic)
        {
            a.Velocity -= frictionImpulse * a.InverseMass;
        }

        if (!b.IsStatic)
        {
            b.Velocity += frictionImpulse * b.InverseMass;
        }
    }

    private void PositionalCorrection(Contact contact)
    {
        var a = contact.A;
        var b = contact.B;

        var invMassSum = a.InverseMass + b.InverseMass;
        if (invMassSum <= 0f)
        {
            return;
        }

        var magnitude = MathF.Max(contact.PenetrationDepth - _positionSlop, 0f) / invMassSum * _positionPercent;
        var correction = contact.Normal * magnitude;

        if (!a.IsStatic)
        {
            a.Position -= correction * a.InverseMass;
        }

        if (!b.IsStatic)
        {
            b.Position += correction * b.InverseMass;
        }
    }
}
