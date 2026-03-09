using Engine.Math;

namespace Engine.Physics
{
    public class RRigidBody
    {
        public RVector2 Position;
        public RVector2 Velocity;

        public RRigidBody(RVector2 position)
        {
            Position = position;
            Velocity = RVector2.Zero;
        }

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }
    }
}
