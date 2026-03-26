using Engine.Math;

namespace Engine.Physics.Bodies
{
    public class RRigidBody
    {
        public RVector2 Position;
        public float Width;
        public float Height;

        public RVector2 Velocity;
        public RVector2 Gravity;
        public float Mass;

        


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



        //RAABB (left, right, top, bottom)
        public RAABB Bounds => new RAABB
                (
                    Position.X,
                    Position.X + Width,
                    Position.Y,
                    Position.Y + Height
                );


        public void Update(float deltaTime)
        {
            Velocity += Gravity * deltaTime;      //gravity gradually increases
            Position += Velocity * deltaTime;
        }
    }
}
