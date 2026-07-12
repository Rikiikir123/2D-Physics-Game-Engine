using Engine.Physics;
using System;
using Engine.Physics.Bodies;
using Engine.Physics.Collision;

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
                body.IsGrounded = false;
            }
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
                        CollisionResolver.ResolveStaticCollision(body, collider);
                    }
                }
                foreach (var collider in BoundaryColliders)
                {
                    if (body.Bounds.Intersects(collider))
                    {
                        CollisionResolver.ResolveStaticCollision(body, collider);
                    }
                }
            }
            for (int i = 0; i < Bodies.Count; i++)
            {
                for (int j = i + 1; j < Bodies.Count; j++)
                {
                    var a = Bodies[i];
                    var b = Bodies[j];

                    if (a.Bounds.Intersects(b.Bounds))
                    {
                        CollisionResolver.ResolveDynamicCollision(a, b);
                    }
                }
            }
        }

        
        // add screen bounds to the static colliders
        public void UpdateBounds(float clientHeight, float clientWidth)
        {
            BoundaryColliders.Clear();
            BoundaryColliders.Add(new RAABB(0f, clientWidth, clientHeight, clientHeight+500f));    //floor
            BoundaryColliders.Add(new RAABB(0f, clientWidth, -500f, 0f));    //ceiling
            BoundaryColliders.Add(new RAABB(-500f, 0f, 0f, clientHeight));    //left wall
            BoundaryColliders.Add(new RAABB(clientWidth, clientWidth+500f, 0f, clientHeight));    //right wall
        }


    }
}
