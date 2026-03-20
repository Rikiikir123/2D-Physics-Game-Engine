using Engine.Math;

namespace Engine.Physics
{
    public class RRigidBody
    {
        public RVector2 Position;
        public float Width;
        public float Height;

        public RVector2 Velocity;
        public RVector2 Gravity;
        public float Mass;

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



        //RAABB (left, right, top, bottom)
        public RAABB Bounds => new RAABB
                (
                    Position.X,
                    Position.X + Width,
                    Position.Y,
                    Position.Y + Height
                );


        public void Update(float deltaTime, float clientHeight, float clientWidth)
        {
            Velocity += Gravity * deltaTime;      //gravity gradually increases
            Position += Velocity * deltaTime;
            
            // floor boundary
            if (Position.Y + Height >= clientHeight)
            {
                Position.Y = clientHeight - Height;   
                Velocity.Y *= -0.5f;
                Velocity.X *=  0.5f;
            }

            // right wall boundary
            if (Position.X + Height >= clientWidth)
            {
                Position.X = clientWidth - Height;  
                Velocity.X *= -0.5f;
            }

            // left wall boundary
            if (Position.X <= 0f)
            {
                Position.X = 0f + Height;
                Velocity.X *= -0.5f;
            }

            // roof boundary
            if (Position.Y <= 0f)
            {
                Position.Y = 0f + Height;
                Velocity.Y *= -0.5f;
            }
        }
    }
}
