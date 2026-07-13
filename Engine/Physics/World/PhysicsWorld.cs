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

        // how many times to re-run collision resolution per step.
        // more iterations = less penetration creep and jitter on resting contacts, at slightly more cpu cost.
        // 1 is equivalent to the old single-pass behavior.
        public int SolverIterations = 3;


        // one physics step
        public void Step(float deltaTime)
        {
            // reset grounded state before integration so it gets set fresh each step
            foreach (var body in Bodies)
            {
                body.IsGrounded = false;
            }

            // integrate velocity and position
            foreach (var body in Bodies)
            {
                body.Update(deltaTime);
            }

            // run collision detection and resolution multiple times to converge on stable contacts
            for (int iter = 0; iter < SolverIterations; iter++)
            {
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
        }


        // add screen bounds to the static colliders
        public void UpdateBounds(float clientHeight, float clientWidth)
        {
            BoundaryColliders.Clear();
            BoundaryColliders.Add(new RAABB(0f, clientWidth, clientHeight, clientHeight + 500f));   // floor
            BoundaryColliders.Add(new RAABB(0f, clientWidth, -500f, 0f));                           // ceiling
            BoundaryColliders.Add(new RAABB(-500f, 0f, 0f, clientHeight));                          // left wall
            BoundaryColliders.Add(new RAABB(clientWidth, clientWidth + 500f, 0f, clientHeight));    // right wall
        }
    }
}
