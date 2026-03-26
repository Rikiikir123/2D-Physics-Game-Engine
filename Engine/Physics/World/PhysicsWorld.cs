using Engine.Physics;
using System;
using Engine.Physics.Bodies;

namespace Engine.Physics.World
{
    public class PhysicsWorld
    {
        public List<RRigidBody> Bodies = new();
        public List<RAABB> StaticColliders = new();
        public List<RAABB> BoundaryColliders = new();




        // one physics step
        public void Step(float deltaTime)
        {
            foreach (var body in Bodies)
            {
                body.Update(deltaTime);
            }

            foreach (var body in Bodies)
            {
                foreach (var collider in StaticColliders)
                {
                    if (body.Bounds.Intersects(collider))
                    {
                        ResolveCollision(body, collider);
                    }
                }
                foreach (var collider in BoundaryColliders)
                {
                    if (body.Bounds.Intersects(collider))
                    {
                        ResolveCollision(body, collider);
                    }
                }
            }
        }





        // handles collision between two objects 
        private void ResolveCollision(RRigidBody body, RAABB platform)
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
                // resolve horizontally
                if (overlapLeft < overlapRight)
                {
                    body.Position.X -= overlapLeft;
                }
                else
                {
                    body.Position.X += overlapRight;
                }

                body.Velocity.X *= -0.5f;
            }
            else
            {
                // resolve vertically
                if (overlapTop < overlapBottom)
                {
                    body.Position.Y -= overlapTop;
                }
                else
                {
                    body.Position.Y += overlapBottom;
                }

                body.Velocity.Y *= -0.5f;
            }
        }
        
        // add screen bounds to the static colliders
        public void updateBounds(float clientHeight, float clientWidth)
        {
            BoundaryColliders.Clear();
            BoundaryColliders.Add(new RAABB(0f, clientWidth, clientHeight, clientHeight+500f));    //floor
            BoundaryColliders.Add(new RAABB(0f, clientWidth, -500f, 0f));    //ceiling
            BoundaryColliders.Add(new RAABB(-500f, 0f, 0f, clientHeight));    //left wall
            BoundaryColliders.Add(new RAABB(clientWidth, clientWidth+500f, 0f, clientHeight));    //right wall
        }


    }
}
