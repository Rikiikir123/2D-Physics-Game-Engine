using Engine.Math;

namespace Engine.Physics
{
    public class Rigidbody2D
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public Rigidbody2D(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
        }
    }
}
