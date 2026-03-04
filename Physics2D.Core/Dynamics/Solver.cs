using Physics2D.Core.Collision;
using Physics2D.Core.Math;

namespace Physics2D.Core.Dynamics;

/// <summary>
/// Iterative impulse solver with positional correction and Coulomb-style friction approximation.
/// </summary>
public sealed class Solver
{
    private const float Slop = 0.001f;
    private const float Percent = 0.8f;

    public int Iterations { get; set; } = 8;

    public void Solve(IReadOnlyList<Contact> contacts)
    {
        for (var i = 0; i < Iterations; i++)
        {
            foreach (var contact in contacts)
            {
                ResolveSingle(contact);
            }
        }
    }

    private static void ResolveSingle(Contact contact)
    {
        var a = contact.A;
        var b = contact.B;

        var invMassSum = a.InverseMass + b.InverseMass;
        if (invMassSum <= 0f)
        {
            return;
        }

        #region Positional Correction
        var correctionMagnitude = System.MathF.Max(contact.PenetrationDepth - Slop, 0f) / invMassSum * Percent;
        var correction = contact.Normal * correctionMagnitude;

        if (!a.IsStatic)
        {
            a.Position -= correction * a.InverseMass;
        }

        if (!b.IsStatic)
        {
            b.Position += correction * b.InverseMass;
        }
        #endregion

        #region Normal Impulse
        var rv = b.Velocity - a.Velocity;
        var velocityAlongNormal = rv.Dot(contact.Normal);

        if (velocityAlongNormal > 0f)
        {
            return;
        }

        var restitution = System.MathF.Min(a.Restitution, b.Restitution);
        var j = -(1f + restitution) * velocityAlongNormal;
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
        #endregion

        #region Friction Impulse
        rv = b.Velocity - a.Velocity;
        var tangent = rv - (contact.Normal * rv.Dot(contact.Normal));
        tangent = tangent.Normalized();

        var jt = -rv.Dot(tangent);
        jt /= invMassSum;

        var mu = System.MathF.Sqrt(a.Friction * b.Friction);
        Vec2 frictionImpulse;
        if (System.MathF.Abs(jt) < j * mu)
        {
            frictionImpulse = tangent * jt;
        }
        else
        {
            frictionImpulse = tangent * (-j * mu);
        }

        if (!a.IsStatic)
        {
            a.Velocity -= frictionImpulse * a.InverseMass;
        }

        if (!b.IsStatic)
        {
            b.Velocity += frictionImpulse * b.InverseMass;
        }
        #endregion
    }
}
