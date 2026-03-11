using Engine.Math;

namespace Engine.Physics
{
    public class RRigidBody
    {
        public RVector2 Position;
        public RVector2 Velocity;
        public RVector2 Gravity;
        public float Mass;

        public float Width;
        public float Height;
        //NOT IMPLEMENTED 
        //public float gravityScale;


        

        public RRigidBody(RVector2 position, float width, float height, float mass, bool useGravity)
        {
            Position = position;
            Velocity = RVector2.Zero;
            Mass = mass;
            Width = width;
            Height = height;

            // check if we want to use gravity
            if (useGravity)
            {
                Gravity = new RVector2(0f, 500f);  // downward gravity
            }
            else Gravity = new RVector2(0f, 0f);


        }

        public void Update(float deltaTime, float floorY)
        {
            Velocity += Gravity * deltaTime;      //gravity gradually increases
            Position += Velocity * deltaTime;
            
            
            if (Position.Y + Height >= floorY)
            {
                Position.Y = floorY - Height;
                Velocity.Y = -Velocity.Y * 0.5f;
                Velocity.X *= 0.5f;
            }
        }
    }
}
