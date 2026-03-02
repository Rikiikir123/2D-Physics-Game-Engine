using Engine.Math;

namespace Engine.Physics
{
    public class Rigidbody2D
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public float GravityScale = 1f;
        public bool UseGravity = true;
        public bool IsGrounded;

        // Default gravity points down in screen-space (+Y).
        public static Vector2 Gravity = new Vector2(0f, 980f);

        public Rigidbody2D(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            Vector2 totalAcceleration = Acceleration;

            if (UseGravity)
            {
                totalAcceleration += Gravity * GravityScale;
            }

            Velocity += totalAcceleration * deltaTime;
            Position += Velocity * deltaTime;

            // Reset per-frame acceleration. External forces can set this each step.
            Acceleration = Vector2.Zero;
        }

        public void ResolveGround(float groundY)
        {
            if (Position.Y >= groundY)
            {
                Position.Y = groundY;

                if (Velocity.Y > 0f)
                {
                    Velocity.Y = 0f;
                }

                IsGrounded = true;
                return;
            }

            IsGrounded = false;
        }
    }
}