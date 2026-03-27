using Engine.Math;

namespace Engine.Physics.Bodies
{
    public class RRigidBody
    {
        public RVector2 Position;
        public float Width;
        public float Height;

        public bool IsStatic;
        public bool IsGrounded;

        public RVector2 Velocity;
        public RVector2 Gravity;
        public float Mass;

        public float Restitution;
        public float Friction;

        



        public RRigidBody(RVector2 position, float width, float height, float mass, bool isStatic, bool useGravity)
        {
            Position = position;
            Velocity = RVector2.Zero;
            Mass = mass;
            Width = width;
            Height = height;
            IsStatic = isStatic;
            IsGrounded = false;

            Restitution = 0.5f;
            Friction = 0.99f;


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
            if (IsStatic)
            {
                return;
            }

            Velocity += Gravity * deltaTime;      //gravity gradually increases
            Position += Velocity * deltaTime;
        }
    }
}
