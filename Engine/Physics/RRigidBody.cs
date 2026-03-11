using Engine.Math;

namespace Engine.Physics
{
    public class RRigidBody
    {
        public RVector2 Position;
        public RVector2 Velocity;
        public RVector2 Gravity;
        public bool useGravity;

        //NOT IMPLEMENTED YET
        //public float Mass;
        //public float gravityScale;


        

        public RRigidBody(RVector2 position, bool useGravity)
        {
            Position = position;
            Velocity = RVector2.Zero;
            if (useGravity)
            {
                Gravity = new RVector2(0f, 500f);  // downward gravity
            }
        }

        public void Update(float deltaTime)
        {
            Velocity += Gravity * deltaTime;      //gravity gradually increases
            Position += Velocity * deltaTime;
        }
    }
}
