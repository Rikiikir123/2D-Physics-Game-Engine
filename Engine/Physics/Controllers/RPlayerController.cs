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

        // grace period after leaving a ledge where a jump is still allowed
        public float CoyoteTime = 0.12f;
        // grace period before landing where a jump press is remembered
        public float JumpBufferTime = 0.12f;
        // how much upward speed remains when jump is released early
        public float JumpCutMultiplier = 0.45f;
        // clamp fall speed so long drops don't feel out of control
        public float MaxFallSpeed = 600f;

        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool jumpCutApplied;

        public RPlayerController(RRigidBody body)
        {
            Body = body;
        }

        // call once per physics step with the current input state.
        // jumpPressed should be true only on the frame the jump key was pressed (edge detection is the caller's job).
        // jumpHeld stays true for as long as the key is down.
        public void ApplyInput(bool moveLeft, bool moveRight, bool jumpPressed, bool jumpHeld, float deltaTime)
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

            // grounded resets coyote and allows a new jump-cut next airborne phase
            if (Body.IsGrounded)
            {
                coyoteTimer = CoyoteTime;
                jumpCutApplied = false;
            }
            else
            {
                coyoteTimer -= deltaTime;
                if (coyoteTimer < 0f)
                {
                    coyoteTimer = 0f;
                }
            }

            // remember a jump press for a short window so early presses still fire on landing
            if (jumpPressed)
            {
                jumpBufferTimer = JumpBufferTime;
            }
            else
            {
                jumpBufferTimer -= deltaTime;
                if (jumpBufferTimer < 0f)
                {
                    jumpBufferTimer = 0f;
                }
            }

            // jump if we still have coyote time and a buffered press
            if (coyoteTimer > 0f && jumpBufferTimer > 0f)
            {
                Body.Velocity.Y = -JumpSpeed;
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
                jumpCutApplied = false;
            }

            // early release shortens the jump arc (only once per jump)
            if (!jumpHeld && !jumpCutApplied && Body.Velocity.Y < 0f)
            {
                Body.Velocity.Y *= JumpCutMultiplier;
                jumpCutApplied = true;
            }

            // clamp downward speed
            if (Body.Velocity.Y > MaxFallSpeed)
            {
                Body.Velocity.Y = MaxFallSpeed;
            }
        }
    }
}
