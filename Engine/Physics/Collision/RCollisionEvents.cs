using Engine.Physics.Bodies;

namespace Engine.Physics.Collision
{
    // whether a body just entered, is still inside, or just left a trigger volume
    public enum RContactPhase
    {
        Enter,
        Stay,
        Exit
    }

    // one contact event between a dynamic body and a static collider (usually a trigger)
    public struct RContactEvent
    {
        public RRigidBody Body;
        public RStaticCollider Collider;
        public RContactPhase Phase;

        public RContactEvent(RRigidBody body, RStaticCollider collider, RContactPhase phase)
        {
            Body = body;
            Collider = collider;
            Phase = phase;
        }
    }

    public delegate void RContactEventHandler(RContactEvent e);
}
