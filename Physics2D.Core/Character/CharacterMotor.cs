using Physics2D.Core.Collision;
using Physics2D.Core.Dynamics;
using Physics2D.Core.Math;

namespace Physics2D.Core.Character;

/// <summary>
/// Platformer-focused movement controller built on top of the physics body.
/// Handles jump buffering, coyote time, variable jump, and grounded movement tuning.
/// </summary>
public sealed class CharacterMotor
{
    private float _jumpBufferTimer;
    private float _coyoteTimer;
    private bool _jumpHeld;

    public CharacterMotor(Body body)
    {
        Body = body;
    }

    public Body Body { get; }

    public float MaxSpeed { get; set; } = 8f;
    public float GroundAcceleration { get; set; } = 80f;
    public float AirAcceleration { get; set; } = 35f;
    public float JumpSpeed { get; set; } = 12f;
    public float JumpCutMultiplier { get; set; } = 0.5f;
    public float MaxFallSpeed { get; set; } = 25f;
    public float CoyoteTimeSeconds { get; set; } = 0.1f;
    public float JumpBufferSeconds { get; set; } = 0.1f;
    public float GroundNormalThreshold { get; set; } = 0.6f;
    public bool IsGrounded { get; private set; }

    public void TickInput(float horizontal, bool jumpPressed, bool jumpHeld, float dt)
    {
        _jumpHeld = jumpHeld;

        if (jumpPressed)
        {
            _jumpBufferTimer = JumpBufferSeconds;
        }

        var targetSpeed = horizontal * MaxSpeed;
        var accel = IsGrounded ? GroundAcceleration : AirAcceleration;
        var speedDelta = targetSpeed - Body.Velocity.X;
        var maxStep = accel * dt;
        var appliedDelta = System.MathF.Clamp(speedDelta, -maxStep, maxStep);

        Body.Velocity = new Vec2(Body.Velocity.X + appliedDelta, Body.Velocity.Y);

        if (!IsGrounded && Body.Velocity.Y < -MaxFallSpeed)
        {
            Body.Velocity = new Vec2(Body.Velocity.X, -MaxFallSpeed);
        }

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            Body.Velocity = new Vec2(Body.Velocity.X, JumpSpeed);
            IsGrounded = false;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }

        if (!_jumpHeld && Body.Velocity.Y > 0f)
        {
            Body.Velocity = new Vec2(Body.Velocity.X, Body.Velocity.Y * JumpCutMultiplier);
        }

        _jumpBufferTimer = System.MathF.Max(0f, _jumpBufferTimer - dt);
        _coyoteTimer = System.MathF.Max(0f, _coyoteTimer - dt);
    }

    public void ConsumeContacts(IEnumerable<Contact> contacts)
    {
        IsGrounded = false;

        foreach (var contact in contacts)
        {
            if (!ReferenceEquals(contact.A, Body) && !ReferenceEquals(contact.B, Body))
            {
                continue;
            }

            var normal = ReferenceEquals(contact.A, Body) ? contact.Normal : -contact.Normal;
            if (normal.Y >= GroundNormalThreshold)
            {
                IsGrounded = true;
                _coyoteTimer = CoyoteTimeSeconds;
                return;
            }
        }
    }
}
