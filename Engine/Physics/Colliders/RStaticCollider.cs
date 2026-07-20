using Engine.Math;

namespace Engine.Physics
{
    // static platform geometry with optional one-way behavior and optional motion.
    // one-way platforms are solid when landing from above, but allow jumping up through them.
    // Velocity != 0 makes the platform a mover; PathMin/PathMax are used by the demo to reverse direction.
    public class RStaticCollider
    {
        public RAABB Bounds;
        public bool IsOneWay;
        public RVector2 Velocity;

        // path endpoints for ping-pong movers (same units as Bounds). unused when both are 0.
        public float PathMin;
        public float PathMax;

        public RStaticCollider(RAABB bounds, bool isOneWay = false)
        {
            Bounds = bounds;
            IsOneWay = isOneWay;
            Velocity = RVector2.Zero;
            PathMin = 0f;
            PathMax = 0f;
        }

        // shift the whole aabb by delta (struct field, so we rebuild Bounds)
        public void Translate(RVector2 delta)
        {
            Bounds = new RAABB(
                Bounds.Left + delta.X,
                Bounds.Right + delta.X,
                Bounds.Top + delta.Y,
                Bounds.Bottom + delta.Y);
        }

        public bool IsMoving => Velocity.X != 0f || Velocity.Y != 0f;
    }
}
