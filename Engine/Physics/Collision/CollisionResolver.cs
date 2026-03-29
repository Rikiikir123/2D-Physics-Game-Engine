
using Engine.Physics.Bodies;
using Engine.Math;

namespace Engine.Physics.Collision
{
	public class CollisionResolver
	{
        // handles collision between two objects 
        public static void ResolveCollision(RRigidBody body, RAABB platform)
        {
            if (body.IsStatic)
            {
                return;
            }

            RAABB b = body.Bounds;

            float overlapLeft = b.Right - platform.Left;
            float overlapRight = platform.Right - b.Left;
            float overlapTop = b.Bottom - platform.Top;
            float overlapBottom = platform.Bottom - b.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            if (minOverlapX < minOverlapY)
            {
                // resolve horizontally
                if (overlapLeft < overlapRight)
                {
                    body.Position.X -= overlapLeft;
                }
                else
                {
                    body.Position.X += overlapRight;
                }

                body.Velocity.X *= -body.Restitution;
            }
            else
            {
                // vertical collision
                if (overlapTop < overlapBottom)
                {
                    // landed on top of platform
                    body.Position.Y -= overlapTop;

                    // if body is moving downward
                    if (body.Velocity.Y > 0f)
                    {
                        // and if its speed is small enough, set it to grounded 
                        if (System.Math.Abs(body.Velocity.Y) < 30f)
                        {
                            body.Velocity.Y = 0f;
                            body.IsGrounded = true;
                        }
                        else
                        {
                            body.Velocity.Y *= -body.Restitution;
                        }
                    }
                }
                else
                {
                    // hit underside
                    body.Position.Y += overlapBottom;

                    if (body.Velocity.Y < 0f)
                    {
                        body.Velocity.Y *= -body.Restitution;
                    }
                }
            }
            if (body.IsGrounded)
            {
                // if body is grounded apply friction
                body.Velocity.X *= body.Friction;

                // if velocity too small, set it to 0
                if (System.Math.Abs(body.Velocity.X) < 1f)
                {
                    body.Velocity.X = 0f;
                }
            }
        }
        public static void ResolveDynamicCollision(RRigidBody a, RRigidBody b)
        {
            if (a.IsStatic && b.IsStatic)
            {
                return;
            }

            RAABB ab = a.Bounds;
            RAABB bb = b.Bounds;

            float overlapLeft = ab.Right - bb.Left;
            float overlapRight = bb.Right - ab.Left;
            float overlapTop = ab.Bottom - bb.Top;
            float overlapBottom = bb.Bottom - ab.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            RVector2 normal;

            // 1. positional correction
            if (minOverlapX < minOverlapY)
            {
                float separation = minOverlapX;

                if (overlapLeft < overlapRight)
                {
                    normal = new RVector2(1f, 0f);

                    if (!a.IsStatic && !b.IsStatic)
                    {
                        a.Position.X -= separation / 2f;
                        b.Position.X += separation / 2f;
                    }
                    else if (!a.IsStatic)
                    {
                        a.Position.X -= separation;
                    }
                    else if (!b.IsStatic)
                    {
                        b.Position.X += separation;
                    }
                }
                else
                {
                    normal = new RVector2(-1f, 0f);

                    if (!a.IsStatic && !b.IsStatic)
                    {
                        a.Position.X += separation / 2f;
                        b.Position.X -= separation / 2f;
                    }
                    else if (!a.IsStatic)
                    {
                        a.Position.X += separation;
                    }
                    else if (!b.IsStatic)
                    {
                        b.Position.X -= separation;
                    }
                }
            }
            else
            {
                float separation = minOverlapY;

                if (overlapTop < overlapBottom)
                {
                    normal = new RVector2(0f, 1f);

                    if (!a.IsStatic && !b.IsStatic)
                    {
                        a.Position.Y -= separation / 2f;
                        b.Position.Y += separation / 2f;
                    }
                    else if (!a.IsStatic)
                    {
                        a.Position.Y -= separation;
                    }
                    else if (!b.IsStatic)
                    {
                        b.Position.Y += separation;
                    }
                }
                else
                {
                    normal = new RVector2(0f, -1f);

                    if (!a.IsStatic && !b.IsStatic)
                    {
                        a.Position.Y += separation / 2f;
                        b.Position.Y -= separation / 2f;
                    }
                    else if (!a.IsStatic)
                    {
                        a.Position.Y += separation;
                    }
                    else if (!b.IsStatic)
                    {
                        b.Position.Y -= separation;
                    }
                }
            }

            // 2. velocity response

            // how b is moving relative to a
            RVector2 relativeVelocity = b.Velocity - a.Velocity;
            // dot product which tells us how fast bodies are moving toward eachother or seperating
            float velAlongNormal = relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y;

            // skip if bodies are already seperating
            if (velAlongNormal > 0f)
            {
                return;
            }
            
            // inverse masses of a and b
            float invMassA = a.IsStatic ? 0f : 1f / a.Mass;
            float invMassB = b.IsStatic ? 0f : 1f / b.Mass;

            // we choose the smaller restitution of the two bodies
            // MAYBE CHANGE LATER CAUSE WHAT IF ONE BALL IS SUPER BOUNCY AND NEEDS TO BOUNCE OFF A STURDY BALL
            float restitution = System.Math.Min(a.Restitution, b.Restitution);

            // how strong the collision push should be (magnitude)
            float j = -(1f + restitution) * velAlongNormal;
            j /= (invMassA + invMassB);

            // calculate the impulse to give each body along the normal vector
            RVector2 impulse = normal * j;

            if (!a.IsStatic)
                a.Velocity -= impulse * invMassA;

            if (!b.IsStatic)
                b.Velocity += impulse * invMassB;
        }
    }
}

