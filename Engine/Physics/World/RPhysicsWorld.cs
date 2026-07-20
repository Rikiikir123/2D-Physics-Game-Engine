using Engine.Physics;
using System;
using Engine.Physics.Bodies;
using Engine.Physics.Collision;
using Engine.Math;

namespace Engine.Physics.World
{
    public class RPhysicsWorld
    {
        public List<RRigidBody> Bodies = new();
        public List<RStaticCollider> StaticColliders = new();
        public List<RAABB> BoundaryColliders = new();

        // how many times to re-run collision resolution per step.
        // more iterations = less penetration creep and jitter on resting contacts, at slightly more cpu cost.
        // 1 is equivalent to the old single-pass behavior.
        public int SolverIterations = 3;

        // bodies slower than this (px/s) count toward falling asleep
        public float SleepVelocityThreshold = 8f;
        // seconds a body must stay below the threshold, with no contact, before it sleeps
        public float SleepTimeRequired = 0.4f;

        // when true, body-vs-body candidates come from the spatial hash; when false, every pair is tested (O(n²))
        public bool UseBroadPhase = true;
        // how many body-body pairs were considered last step (for evaluation HUD)
        public int LastCandidatePairCount { get; private set; }

        // fired for trigger volumes: enter on first overlap, stay while overlapping, exit when overlap ends
        public event RContactEventHandler? OnStaticContact;

        private readonly RSpatialHash spatialHash = new(64f);
        private readonly List<(int, int)> candidatePairs = new();

        // previous vs current trigger overlaps keyed by (bodyIndex, colliderIndex)
        private HashSet<(int, int)> previousTriggerPairs = new();
        private HashSet<(int, int)> currentTriggerPairs = new();


        // one physics step
        public void Step(float deltaTime)
        {
            // carry passengers using last step's platform velocity so they don't slide off movers
            foreach (var body in Bodies)
            {
                if (!body.IsStatic && (body.PlatformVelocity.X != 0f || body.PlatformVelocity.Y != 0f))
                {
                    body.Position += body.PlatformVelocity * deltaTime;
                }
            }

            // integrate moving platforms
            foreach (var collider in StaticColliders)
            {
                if (collider.Enabled && collider.IsMoving)
                {
                    collider.Translate(collider.Velocity * deltaTime);
                }
            }

            // reset per-step flags before integration
            foreach (var body in Bodies)
            {
                body.IsGrounded = false;
                body.HadContact = false;
                body.PlatformVelocity = RVector2.Zero;
            }

            // integrate velocity and position - sleeping bodies skip this so they stay put
            foreach (var body in Bodies)
            {
                if (!body.IsStatic && !body.IsSleeping)
                {
                    body.Update(deltaTime);
                }
            }

            // run collision detection and resolution multiple times to converge on stable contacts
            for (int iter = 0; iter < SolverIterations; iter++)
            {
                foreach (var body in Bodies)
                {
                    foreach (var collider in StaticColliders)
                    {
                        if (!collider.Enabled || collider.IsTrigger)
                        {
                            continue;
                        }

                        if (body.Bounds.Intersects(collider.Bounds))
                        {
                            // only actually wake a sleeping body here - a body already awake and
                            // resting in continuous contact shouldn't have its sleep timer reset every step
                            if (body.IsSleeping) body.Wake();
                            body.HadContact = true;
                            RCollisionResolver.ResolveStaticCollision(body, collider);
                        }
                    }
                    foreach (var collider in BoundaryColliders)
                    {
                        if (body.Bounds.Intersects(collider))
                        {
                            if (body.IsSleeping) body.Wake();
                            body.HadContact = true;
                            RCollisionResolver.ResolveStaticCollision(body, collider);
                        }
                    }
                }

                ResolveBodyVsBodyPairs();
            }

            // trigger sensors once per step (not per solver iteration) so enter doesn't spam
            ProcessTriggerContacts();

            // sleeping is based purely on how slow a body is moving, not on whether it's in contact -
            // a body resting on a platform is touching it every step and should still be able to sleep
            foreach (var body in Bodies)
            {
                if (!body.IsStatic)
                {
                    body.TrySleep(deltaTime, SleepVelocityThreshold, SleepTimeRequired);
                }
            }
        }

        // compare this step's trigger overlaps to last step and raise enter/stay/exit
        private void ProcessTriggerContacts()
        {
            currentTriggerPairs.Clear();

            for (int bi = 0; bi < Bodies.Count; bi++)
            {
                RRigidBody body = Bodies[bi];
                if (body.IsStatic)
                {
                    continue;
                }

                for (int ci = 0; ci < StaticColliders.Count; ci++)
                {
                    RStaticCollider collider = StaticColliders[ci];
                    if (!collider.Enabled || !collider.IsTrigger)
                    {
                        continue;
                    }

                    if (!body.Bounds.Intersects(collider.Bounds))
                    {
                        continue;
                    }

                    var key = (bi, ci);
                    currentTriggerPairs.Add(key);

                    RContactPhase phase = previousTriggerPairs.Contains(key)
                        ? RContactPhase.Stay
                        : RContactPhase.Enter;

                    OnStaticContact?.Invoke(new RContactEvent(body, collider, phase));
                }
            }

            foreach (var key in previousTriggerPairs)
            {
                if (currentTriggerPairs.Contains(key))
                {
                    continue;
                }

                // body or collider may have been removed; skip stale indices
                if (key.Item1 < 0 || key.Item1 >= Bodies.Count ||
                    key.Item2 < 0 || key.Item2 >= StaticColliders.Count)
                {
                    continue;
                }

                RRigidBody body = Bodies[key.Item1];
                RStaticCollider collider = StaticColliders[key.Item2];
                OnStaticContact?.Invoke(new RContactEvent(body, collider, RContactPhase.Exit));
            }

            // swap sets for next step
            (previousTriggerPairs, currentTriggerPairs) = (currentTriggerPairs, previousTriggerPairs);
            currentTriggerPairs.Clear();
        }

        // clear trigger history when rebuilding a scene so old indices don't fire false exits
        public void ClearTriggerContactHistory()
        {
            previousTriggerPairs.Clear();
            currentTriggerPairs.Clear();
        }

        // gathers candidate pairs (spatial hash or brute force), then runs narrow-phase + resolve
        private void ResolveBodyVsBodyPairs()
        {
            if (UseBroadPhase)
            {
                spatialHash.Clear();
                for (int i = 0; i < Bodies.Count; i++)
                {
                    spatialHash.Insert(i, Bodies[i].Bounds);
                }
                spatialHash.GetCandidatePairs(candidatePairs);
            }
            else
            {
                // brute-force baseline for A/B evaluation in the thesis
                candidatePairs.Clear();
                for (int i = 0; i < Bodies.Count; i++)
                {
                    for (int j = i + 1; j < Bodies.Count; j++)
                    {
                        candidatePairs.Add((i, j));
                    }
                }
            }

            LastCandidatePairCount = candidatePairs.Count;

            foreach (var (i, j) in candidatePairs)
            {
                var a = Bodies[i];
                var b = Bodies[j];

                if (a.Bounds.Intersects(b.Bounds))
                {
                    if (a.IsSleeping) a.Wake();
                    if (b.IsSleeping) b.Wake();
                    a.HadContact = true;
                    b.HadContact = true;
                    RCollisionResolver.ResolveDynamicCollision(a, b);
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
