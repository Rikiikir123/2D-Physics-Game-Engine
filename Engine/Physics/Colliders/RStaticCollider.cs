using Engine.Math;

namespace Engine.Physics
{
    // static platform geometry with optional one-way behavior and optional motion.
    // Velocity != 0 makes the platform a mover; PathMin/PathMax are used by the demo to reverse direction.
    // IsTrigger volumes detect overlap only (no push/impulse) and drive enter/stay/exit events.
    public class RStaticCollider
    {
        public RAABB Bounds;
        public bool IsOneWay;
        public bool IsTrigger;
        public bool Enabled = true;
        // optional gameplay label (e.g. "coin", "hazard") used by the demo / game layer
        public string Tag = "";
        public RVector2 Velocity;

        // path endpoints for ping-pong movers (same units as Bounds). unused when both are 0.
        public float PathMin;
        public float PathMax;

        public RStaticCollider(RAABB bounds, bool isOneWay = false)
        {
            Bounds = bounds;
            IsOneWay = isOneWay;
            IsTrigger = false;
            Enabled = true;
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
