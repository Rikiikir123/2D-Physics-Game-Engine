using Physics2D.Core.Dynamics;

namespace Physics2D.Core.Character;

/// <summary>
/// Platformer-focused character motor layered above the rigid body simulation.
/// Handles coyote time, jump buffering, variable jump, and air/ground acceleration.
/// </summary>
public sealed class CharacterMotor
{
    private readonly Body _body;

    private float _timeSinceGrounded;
    private float _timeSinceJumpPressed;
    private bool _jumpHeld;

    public CharacterMotor(Body body)
    {
        _body = body;
    }

    public float GroundAcceleration { get; set; } = 90f;
    public float AirAcceleration { get; set; } = 45f;
    public float MaxHorizontalSpeed { get; set; } = 8f;
    public float JumpVelocity { get; set; } = 11f;
    public float MaxFallSpeed { get; set; } = 20f;
    public float CoyoteTime { get; set; } = 0.1f;
    public float JumpBufferTime { get; set; } = 0.1f;
    public float JumpCutMultiplier { get; set; } = 0.5f;
    public float GroundNormalThreshold { get; set; } = 0.5f;

    public bool IsGrounded { get; private set; }

    public void SetJumpPressed()
    {
        _timeSinceJumpPressed = 0f;
    }

    public void SetJumpHeld(bool held)
    {
        _jumpHeld = held;
    }

    /// <summary>
    /// Updates motor using contact normals from world step.
    /// </summary>
    public void Tick(float dt, float horizontalInput, IReadOnlyList<Collision.Contact> contacts)
    {
        EvaluateGrounding(contacts);

        _timeSinceGrounded += dt;
        _timeSinceJumpPressed += dt;

        var targetSpeed = horizontalInput * MaxHorizontalSpeed;
        var accel = IsGrounded ? GroundAcceleration : AirAcceleration;
        var delta = targetSpeed - _body.Velocity.X;
        var maxDelta = accel * dt;
        delta = float.Clamp(delta, -maxDelta, maxDelta);

        _body.Velocity = new Math.Vec2(_body.Velocity.X + delta, _body.Velocity.Y);

        if (_timeSinceJumpPressed <= JumpBufferTime && _timeSinceGrounded <= CoyoteTime)
        {
            _body.Velocity = new Math.Vec2(_body.Velocity.X, JumpVelocity);
            _timeSinceJumpPressed = float.MaxValue;
            _timeSinceGrounded = float.MaxValue;
            IsGrounded = false;
        }

        if (!_jumpHeld && _body.Velocity.Y > 0f)
        {
            _body.Velocity = new Math.Vec2(_body.Velocity.X, _body.Velocity.Y * JumpCutMultiplier);
        }

        if (_body.Velocity.Y < -MaxFallSpeed)
        {
            _body.Velocity = new Math.Vec2(_body.Velocity.X, -MaxFallSpeed);
        }
    }

    private void EvaluateGrounding(IReadOnlyList<Collision.Contact> contacts)
    {
        IsGrounded = false;

        foreach (var contact in contacts)
        {
            if (!ReferenceEquals(contact.A, _body) && !ReferenceEquals(contact.B, _body))
            {
                continue;
            }

            var normal = ReferenceEquals(contact.A, _body) ? -contact.Normal : contact.Normal;
            if (normal.Y >= GroundNormalThreshold)
            {
                IsGrounded = true;
                _timeSinceGrounded = 0f;
                break;
            }
        }
    }
}
