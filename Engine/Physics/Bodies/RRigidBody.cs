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

        public bool IsSleeping;
        public bool HadContact;     // reset and set by PhysicsWorld each step
        public float SleepTimer;    // seconds spent continuously below the sleep velocity threshold

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

            IsSleeping = false;
            HadContact = false;
            SleepTimer = 0f;

            Restitution = 0.5f;
            Friction = 0.99f;    // used as per-frame horizontal damping when grounded on a static surface


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

            Wake();
            AccumulatedForce += force;
        }
        public void AddImpulse(RVector2 impulse)
        {
            if (IsStatic || Mass <= 0f)
            {
                return;
            }

            Wake();
            Velocity += impulse / Mass;
        }

        // reactivates a sleeping body - called whenever something disturbs it
        public void Wake()
        {
            IsSleeping = false;
            SleepTimer = 0f;
        }

        // called once per step for bodies that had no contact this step - accumulates time spent
        // slow enough to sleep, and puts the body to sleep once it's been still for long enough
        public void TrySleep(float deltaTime, float velocityThreshold, float sleepTimeRequired)
        {
            if (IsStatic || IsSleeping)
            {
                return;
            }

            if (Velocity.Length < velocityThreshold)
            {
                SleepTimer += deltaTime;

                if (SleepTimer >= sleepTimeRequired)
                {
                    IsSleeping = true;
                    Velocity = RVector2.Zero;
                }
            }
            else
            {
                SleepTimer = 0f;
            }
        }
    }
}
