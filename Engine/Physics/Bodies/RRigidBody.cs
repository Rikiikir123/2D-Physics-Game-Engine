using Engine.Math;
using Engine.Physics.Shapes;
using static Engine.Physics.Shapes.RShape;

namespace Engine.Physics.Bodies
{
    public class RRigidBody
    {
        public RVector2 Position;

        public bool IsStatic;
        public bool IsGrounded;

        public RVector2 AccumulatedForce;
        public RVector2 Velocity;
        public RVector2 Gravity;
        public float Mass;

        public float Restitution;
        public float Friction;

        public RShape Shape;
        



        public RRigidBody(RVector2 position, RShape shape, float mass, bool isStatic, bool useGravity)
        {

            Position = position;
            Velocity = RVector2.Zero;
            AccumulatedForce = RVector2.Zero;
            Mass = mass;
            Shape = shape;
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
        public RAABB Bounds
        {
            get
            {
                if (Shape is RRectangleShape rect)
                {
                    return new RAABB(
                        Position.X,
                        Position.X + rect.Width,
                        Position.Y,
                        Position.Y + rect.Height
                    );
                }

                if (Shape is RCircleShape circle)
                {
                    return new RAABB(
                        Position.X,
                        Position.X + circle.Radius * 2f,
                        Position.Y,
                        Position.Y + circle.Radius * 2f
                    );
                }
                throw new InvalidOperationException("Unknown shape type.");
            }
        }

        public void Update(float deltaTime)
        {
            if (IsStatic || Mass <= 0f)
            {
                ClearForces();
                return;
            }

            RVector2 acceleration = Gravity + (AccumulatedForce / Mass);
            Velocity += acceleration * deltaTime;     
            Position += Velocity * deltaTime;
            ClearForces();
        }

        public void ClearForces()
        {
            AccumulatedForce = RVector2.Zero;
        }

        public void AddForce(RVector2 force)
        {
            if (IsStatic || Mass <= 0f)
            {
                return;
            }

            AccumulatedForce += force;
        }
        public void AddImpulse(RVector2 impulse)
        {
            if (IsStatic || Mass <= 0f)
            {
                return;
            }

            Velocity += impulse / Mass;
        }
    }
}
