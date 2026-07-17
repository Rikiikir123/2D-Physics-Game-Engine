namespace Engine.Physics
{
    // static platform geometry with optional one-way behavior.
    // one-way platforms are solid when landing from above, but allow jumping up through them.
    public class RStaticCollider
    {
        public RAABB Bounds;
        public bool IsOneWay;

        public RStaticCollider(RAABB bounds, bool isOneWay = false)
        {
            Bounds = bounds;
            IsOneWay = isOneWay;
        }
    }
}
