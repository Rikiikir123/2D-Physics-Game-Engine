using Physics2D.Core.Character;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Input bridge that drives the core character motor from Unity input.
/// </summary>
[RequireComponent(typeof(PhysicsBodyBehaviour))]
public sealed class CharacterControllerBehaviour : MonoBehaviour
{
    private PhysicsBodyBehaviour _body = default!;
    private PhysicsWorldRunner _runner = default!;
    private CharacterMotor _motor = default!;

    private void Start()
    {
        _body = GetComponent<PhysicsBodyBehaviour>();
        _runner = FindFirstObjectByType<PhysicsWorldRunner>();
        _motor = new CharacterMotor(_body.CoreBody);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _motor.SetJumpPressed();
        }

        _motor.SetJumpHeld(Input.GetButton("Jump"));
    }

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        _motor.Tick(Time.fixedDeltaTime, horizontal, _runner.World.LastContacts);
    }
}
