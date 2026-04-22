
using Engine.Physics.Bodies;
using Engine.Math;
using static Engine.Physics.Shapes.RShape;

namespace Engine.Physics.Collision
{
	public class CollisionResolver
	{

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
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

            if (invMassA + invMassB == 0f)
            {
                return;
            }

            float restitution = System.Math.Min(a.Restitution, b.Restitution);

            float j = -(1f + restitution) * velAlongNormal;
            j /= (invMassA + invMassB);

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



        // handles collision between two objects 
        public static void ResolveStaticCollision(RRigidBody body, RAABB platform)
        {
            if (body.IsStatic)
            {
                return;
            }

            if (body.Shape is RRectangleShape rect)
            {
                ResolveRectangleStatic(body, platform, rect);
            }
            else if (body.Shape is RCircleShape circle)
            {
                ResolveCircleStatic(body, platform, circle);
            }
        }
        public static void ResolveDynamicCollision(RRigidBody a, RRigidBody b)
        {
            if (a.IsStatic && b.IsStatic)
            {
                return;
            }

            if (a.Shape is RRectangleShape rectA && b.Shape is RRectangleShape rectB)
            {
                ResolveRectangleRectangle(a, b, rectA, rectB);
            }
            else if (a.Shape is RCircleShape circleA && b.Shape is RCircleShape circleB)
            {
                ResolveCircleCircle(a, b, circleA, circleB);
            }
            else if (a.Shape is RCircleShape circle && b.Shape is RRectangleShape rect)
            {
                ResolveCircleRectangle(a, b, circle, rect);
            }
            else if (a.Shape is RRectangleShape rect2 && b.Shape is RCircleShape circle2)
            {
                ResolveCircleRectangle(b, a, circle2, rect2);
            }
        }

        private static void ResolveRectangleRectangle(RRigidBody a, RRigidBody b, RRectangleShape rectA, RRectangleShape rectB)
        {
            RAABB ab = a.Bounds;
            RAABB bb = b.Bounds;

            float overlapLeft = ab.Right - bb.Left;
            float overlapRight = bb.Right - ab.Left;
            float overlapTop = ab.Bottom - bb.Top;
            float overlapBottom = bb.Bottom - ab.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            RVector2 normal;

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

            ApplyImpulse(a, b, normal);
        }
        private static void ResolveCircleCircle(RRigidBody a, RRigidBody b, RCircleShape circleA, RCircleShape circleB)
        {
            RVector2 centerA = new RVector2(
                a.Position.X + circleA.Radius,
                a.Position.Y + circleA.Radius
            );

            RVector2 centerB = new RVector2(
                b.Position.X + circleB.Radius,
                b.Position.Y + circleB.Radius
            );

            RVector2 delta = centerB - centerA;

            float distanceSquared = delta.X * delta.X + delta.Y * delta.Y;
            float radiusSum = circleA.Radius + circleB.Radius;

            if (distanceSquared >= radiusSum * radiusSum)
            {
                return;
            }

            float distance = (float)System.Math.Sqrt(distanceSquared);

            RVector2 normal;
            float penetration;

            if (distance == 0f)
            {
                normal = new RVector2(1f, 0f);
                penetration = radiusSum;
            }
            else
            {
                normal = delta / distance;
                penetration = radiusSum - distance;
            }

            if (!a.IsStatic && !b.IsStatic)
            {
                a.Position -= normal * (penetration / 2f);
                b.Position += normal * (penetration / 2f);
            }
            else if (!a.IsStatic)
            {
                a.Position -= normal * penetration;
            }
            else if (!b.IsStatic)
            {
                b.Position += normal * penetration;
            }

            ApplyImpulse(a, b, normal);
        }
        private static void ResolveCircleRectangle(RRigidBody circleBody, RRigidBody rectBody, RCircleShape circle, RRectangleShape rect)
        {
            float rectLeft = rectBody.Position.X;
            float rectRight = rectBody.Position.X + rect.Width;
            float rectTop = rectBody.Position.Y;
            float rectBottom = rectBody.Position.Y + rect.Height;

            RVector2 circleCenter = new RVector2(
                circleBody.Position.X + circle.Radius,
                circleBody.Position.Y + circle.Radius
            );

            RVector2 normal;
            float penetration;

            bool centerInsideX = circleCenter.X >= rectLeft && circleCenter.X <= rectRight;
            bool centerInsideY = circleCenter.Y >= rectTop && circleCenter.Y <= rectBottom;

            // CASE 1: circle center is horizontally inside rectangle span
            // -> treat as top/bottom collision only
            if (centerInsideX && !centerInsideY)
            {
                if (circleCenter.Y < rectTop)
                {
                    float distanceToTop = rectTop - circleCenter.Y;
                    penetration = circle.Radius - distanceToTop;

                    if (penetration <= 0f)
                        return;

                    normal = new RVector2(0f, 1f);
                }
                else
                {
                    float distanceToBottom = circleCenter.Y - rectBottom;
                    penetration = circle.Radius - distanceToBottom;

                    if (penetration <= 0f)
                        return;

                    normal = new RVector2(0f, -1f);
                }
            }
            // CASE 2: circle center is vertically inside rectangle span
            // -> treat as left/right collision only
            else if (!centerInsideX && centerInsideY)
            {
                if (circleCenter.X < rectLeft)
                {
                    float distanceToLeft = rectLeft - circleCenter.X;
                    penetration = circle.Radius - distanceToLeft;

                    if (penetration <= 0f)
                        return;

                    normal = new RVector2(1f, 0f);
                }
                else
                {
                    float distanceToRight = circleCenter.X - rectRight;
                    penetration = circle.Radius - distanceToRight;

                    if (penetration <= 0f)
                        return;

                    normal = new RVector2(-1f, 0f);
                }
            }
            // CASE 3: corner collision
            else
            {
                float closestX = Clamp(circleCenter.X, rectLeft, rectRight);
                float closestY = Clamp(circleCenter.Y, rectTop, rectBottom);

                RVector2 closestPoint = new RVector2(closestX, closestY);
                RVector2 delta = circleCenter - closestPoint;

                float distanceSquared = delta.X * delta.X + delta.Y * delta.Y;

                if (distanceSquared >= circle.Radius * circle.Radius)
                    return;

                float distance = (float)System.Math.Sqrt(distanceSquared);

                if (distance == 0f)
                    return;

                normal = (closestPoint - circleCenter) / distance;
                penetration = circle.Radius - distance;

                if (penetration <= 0f)
                    return;
            }

            // positional correction
            if (!circleBody.IsStatic && !rectBody.IsStatic)
            {
                circleBody.Position -= normal * (penetration / 2f);
                rectBody.Position += normal * (penetration / 2f);
            }
            else if (!circleBody.IsStatic)
            {
                circleBody.Position -= normal * penetration;
            }
            else if (!rectBody.IsStatic)
            {
                rectBody.Position += normal * penetration;
            }

            ApplyImpulse(circleBody, rectBody, normal);
        }

        private static void ResolveRectangleStatic(RRigidBody body, RAABB platform, RRectangleShape rect)
        {
            RAABB b = body.Bounds;

            float overlapLeft = b.Right - platform.Left;
            float overlapRight = platform.Right - b.Left;
            float overlapTop = b.Bottom - platform.Top;
            float overlapBottom = platform.Bottom - b.Top;

            float minOverlapX = System.Math.Min(overlapLeft, overlapRight);
            float minOverlapY = System.Math.Min(overlapTop, overlapBottom);

            if (minOverlapX < minOverlapY)
            {
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
                if (overlapTop < overlapBottom)
                {
                    body.Position.Y -= overlapTop;

                    if (body.Velocity.Y > 0f)
                    {
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
                    body.Position.Y += overlapBottom;

                    if (body.Velocity.Y < 0f)
                    {
                        body.Velocity.Y *= -body.Restitution;
                    }
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
        private static void ResolveCircleStatic(RRigidBody body, RAABB platform, RCircleShape circle)
        {
            RVector2 circleCenter = new RVector2(
                body.Position.X + circle.Radius,
                body.Position.Y + circle.Radius
            );

            float closestX = Clamp(circleCenter.X, platform.Left, platform.Right);
            float closestY = Clamp(circleCenter.Y, platform.Top, platform.Bottom);

            RVector2 closestPoint = new RVector2(closestX, closestY);
            RVector2 delta = circleCenter - closestPoint;

            float distanceSquared = delta.X * delta.X + delta.Y * delta.Y;

            if (distanceSquared >= circle.Radius * circle.Radius)
            {
                return;
            }

            float distance = (float)System.Math.Sqrt(distanceSquared);

            RVector2 normal;
            float penetration;

            if (distance == 0f)
            {
                float overlapLeft = circleCenter.X - platform.Left;
                float overlapRight = platform.Right - circleCenter.X;
                float overlapTop = circleCenter.Y - platform.Top;
                float overlapBottom = platform.Bottom - circleCenter.Y;

                float minOverlap = System.Math.Min(
                    System.Math.Min(overlapLeft, overlapRight),
                    System.Math.Min(overlapTop, overlapBottom)
                );

                if (minOverlap == overlapLeft)
                    normal = new RVector2(-1f, 0f);
                else if (minOverlap == overlapRight)
                    normal = new RVector2(1f, 0f);
                else if (minOverlap == overlapTop)
                    normal = new RVector2(0f, -1f);
                else
                    normal = new RVector2(0f, 1f);

                penetration = circle.Radius;
            }
            else
            {
                normal = delta / distance;
                penetration = circle.Radius - distance;
            }

            body.Position += normal * penetration;

            float velocityAlongNormal = body.Velocity.X * normal.X + body.Velocity.Y * normal.Y;

            if (velocityAlongNormal < 0f)
            {
                body.Velocity -= normal * ((1f + body.Restitution) * velocityAlongNormal);
            }

            if (normal.Y < 0f)
            {
                body.IsGrounded = true;
                body.Velocity.X *= body.Friction;

                if (System.Math.Abs(body.Velocity.X) < 1f)
                {
                    body.Velocity.X = 0f;
                }
            }
        }
    }
}

