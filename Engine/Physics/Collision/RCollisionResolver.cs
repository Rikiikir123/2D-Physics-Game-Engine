using Engine.Physics.Bodies;
using Engine.Math;
using static Engine.Physics.Shapes.RShape;

namespace Engine.Physics.Collision
{
	public class RCollisionResolver
	{

        // handles collision between a body and a static platform (may be one-way / moving)
        public static void ResolveStaticCollision(RRigidBody body, RStaticCollider platform)
        {
            if (body.IsStatic)
            {
                return;
            }

            // one-way platforms only block when falling onto them from above
            if (platform.IsOneWay && ShouldPassThroughOneWay(body, platform.Bounds))
            {
                return;
            }

            ResolveAgainstAABB(body, platform.Bounds, platform.Velocity);
        }

        // handles collision between a body and a solid boundary (never one-way, never moving)
        public static void ResolveStaticCollision(RRigidBody body, RAABB platform)
        {
            if (body.IsStatic)
            {
                return;
            }

            ResolveAgainstAABB(body, platform, RVector2.Zero);
        }

        // pass through while jumping up, or when the feet are clearly below the top surface
        // (coming from underneath). only land when falling with feet near the platform top.
        private static bool ShouldPassThroughOneWay(RRigidBody body, RAABB platform)
        {
            // moving upward - jump through
            if (body.Velocity.Y < 0f)
            {
                return true;
            }

            // feet clearly below the top means we entered from underneath, not landed on top
            if (body.Bounds.Bottom > platform.Top + 8f)
            {
                return true;
            }

            return false;
        }

        private static void ResolveAgainstAABB(RRigidBody body, RAABB platform, RVector2 surfaceVelocity)
        {
            if (body.Shape is RRectangleShape)
            {
                if (RCollisionDetector.TryDetectAABBvsAABB(body.Bounds, platform, out RCollisionManifold manifold))
                {
                    ResolveBodyVsStatic(body, manifold, surfaceVelocity);
                }
            }
            else if (body.Shape is RCircleShape circle)
            {
                RVector2 center = body.Position + new RVector2(circle.Radius, circle.Radius);

                if (RCollisionDetector.TryDetectCircleVsAABB(center, circle.Radius, platform, out RCollisionManifold manifold))
                {
                    ResolveBodyVsStatic(body, manifold, surfaceVelocity);
                }
            }
        }

        // handles collision between two dynamic (or dynamic vs static) rigid bodies
        public static void ResolveDynamicCollision(RRigidBody a, RRigidBody b)
        {
            if (a.IsStatic && b.IsStatic)
            {
                return;
            }

            if (a.Shape is RRectangleShape && b.Shape is RRectangleShape)
            {
                if (RCollisionDetector.TryDetectAABBvsAABB(a.Bounds, b.Bounds, out RCollisionManifold manifold))
                {
                    ResolveBodyVsBody(a, b, manifold);
                }
            }
            else if (a.Shape is RCircleShape circleA && b.Shape is RCircleShape circleB)
            {
                RVector2 centerA = a.Position + new RVector2(circleA.Radius, circleA.Radius);
                RVector2 centerB = b.Position + new RVector2(circleB.Radius, circleB.Radius);

                if (RCollisionDetector.TryDetectCircleVsCircle(centerA, circleA.Radius, centerB, circleB.Radius, out RCollisionManifold manifold))
                {
                    ResolveBodyVsBody(a, b, manifold);
                }
            }
            else if (a.Shape is RCircleShape circle && b.Shape is RRectangleShape)
            {
                RVector2 center = a.Position + new RVector2(circle.Radius, circle.Radius);

                if (RCollisionDetector.TryDetectCircleVsAABB(center, circle.Radius, b.Bounds, out RCollisionManifold manifold))
                {
                    ResolveBodyVsBody(a, b, manifold);
                }
            }
            else if (a.Shape is RRectangleShape && b.Shape is RCircleShape circle2)
            {
                RVector2 center = b.Position + new RVector2(circle2.Radius, circle2.Radius);

                // detector expects circle first, so flip normal since it now points from b to a
                if (RCollisionDetector.TryDetectCircleVsAABB(center, circle2.Radius, a.Bounds, out RCollisionManifold manifold))
                {
                    manifold.Normal *= -1f;
                    ResolveBodyVsBody(a, b, manifold);
                }
            }
        }

        // pushes a and b apart along the manifold normal, split by inverse mass so a heavier
        // body barely moves and a static body (invMass 0) doesn't move at all, then applies the impulse
        private static void ResolveBodyVsBody(RRigidBody a, RRigidBody b, RCollisionManifold manifold)
        {
            float invMassA = a.IsStatic ? 0f : 1f / a.Mass;
            float invMassB = b.IsStatic ? 0f : 1f / b.Mass;

            float totalInvMass = invMassA + invMassB;

            if (totalInvMass > 0f)
            {
                float correctionA = manifold.Penetration * (invMassA / totalInvMass);
                float correctionB = manifold.Penetration * (invMassB / totalInvMass);

                a.Position -= manifold.Normal * correctionA;
                b.Position += manifold.Normal * correctionB;
            }

            // manifold normal always points from a to b, so a resting on top of b means
            // b is mostly below a (normal.Y > 0) and vice versa - without this, standing on
            // a dynamic body (like a pushable prop) never counts as grounded for jumping
            if (manifold.Normal.Y > 0.9f)
            {
                a.IsGrounded = true;
            }
            else if (manifold.Normal.Y < -0.9f)
            {
                b.IsGrounded = true;
            }

            ApplyImpulse(a, b, manifold.Normal);
        }

        // static platforms never move as bodies, so the full penetration correction goes on the body.
        // surfaceVelocity is the platform's motion (zero for fixed floors/walls).
        private static void ResolveBodyVsStatic(RRigidBody body, RCollisionManifold manifold, RVector2 surfaceVelocity)
        {
            body.Position -= manifold.Normal * manifold.Penetration;

            // axis-aligned contacts (aabb rectangles and flat platform faces) use the old axis rules
            if (manifold.Normal.Y > 0.9f)
            {
                // landing on top of something below
                if (body.Velocity.Y > 0f)
                {
                    if (System.Math.Abs(body.Velocity.Y) < 30f)
                    {
                        body.Velocity.Y = 0f;
                        body.IsGrounded = true;
                        body.PlatformVelocity = surfaceVelocity;
                    }
                    else
                    {
                        body.Velocity.Y *= -body.Restitution;
                    }
                }
            }
            else if (manifold.Normal.Y < -0.9f)
            {
                // hitting underside of a platform / ceiling
                if (body.Velocity.Y < 0f)
                {
                    body.Velocity.Y *= -body.Restitution;
                }
            }
            else if (System.Math.Abs(manifold.Normal.X) > 0.9f)
            {
                // side wall hit
                body.Velocity.X *= -body.Restitution;
            }
            else
            {
                // circle corner or diagonal contact - bounce along the contact normal
                float velocityAlongNormal = body.Velocity.X * manifold.Normal.X + body.Velocity.Y * manifold.Normal.Y;

                if (velocityAlongNormal > 0f)
                {
                    body.Velocity -= manifold.Normal * ((1f + body.Restitution) * velocityAlongNormal);
                }

                if (manifold.Normal.Y > 0f)
                {
                    body.IsGrounded = true;
                    body.PlatformVelocity = surfaceVelocity;
                }
            }

            if (body.IsGrounded)
            {
                body.Velocity.X *= body.Friction;

                if (System.Math.Abs(body.Velocity.X) < 1f)
                {
                    body.Velocity.X = 0f;
                }
            }
        }

        private static void ApplyImpulse(RRigidBody a, RRigidBody b, RVector2 normal)
        {
            RVector2 relativeVelocity = b.Velocity - a.Velocity;
            float velAlongNormal = relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y;

            if (velAlongNormal > 0f)
            {
                return;
            }

            float invMassA = a.IsStatic ? 0f : 1f / a.Mass;
            float invMassB = b.IsStatic ? 0f : 1f / b.Mass;
            float totalInvMass = invMassA + invMassB;

            if (totalInvMass == 0f)
            {
                return;
            }

            float restitution = System.Math.Min(a.Restitution, b.Restitution);

            float j = -(1f + restitution) * velAlongNormal;
            j /= totalInvMass;

            RVector2 impulse = normal * j;

            if (!a.IsStatic)
            {
                a.Velocity -= impulse * invMassA;
            }

            if (!b.IsStatic)
            {
                b.Velocity += impulse * invMassB;
            }
        }
    }
}
