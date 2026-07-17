using Engine.Physics.Bodies;

namespace Engine.Physics.Controllers
{
    // moves a rigid body based on simple platformer input - no keyboard/engine-specific
    // code here, so the same class can drive a WinForms sandbox or a Unity MonoBehaviour later
    public class RPlayerController
    {
        public RRigidBody Body;

        public float MoveSpeed = 220f;   // px/s horizontal
        public float JumpSpeed = 380f;   // px/s upward (screen Y-down, so this becomes negative Y)

        public RPlayerController(RRigidBody body)
        {
            Body = body;
        }

        // call once per physics step with the current input state.
        // jump should be true only on the frame the jump key was pressed (edge detection is the caller's job)
        public void ApplyInput(bool moveLeft, bool moveRight, bool jump)
        {
            Body.Wake();

            // direct horizontal velocity - standard platformer approach, no acceleration/friction feel
            if (moveLeft)
            {
                Body.Velocity.X = -MoveSpeed;
            }
            else if (moveRight)
            {
                Body.Velocity.X = MoveSpeed;
            }
            else if (Body.IsGrounded)
            {
                Body.Velocity.X = 0f;
            }

            // jump only when grounded
            if (jump && Body.IsGrounded)
            {
                Body.Velocity.Y = -JumpSpeed;
            }
        }
    }
}
