using Engine.Physics.Bodies;

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
    }
}

