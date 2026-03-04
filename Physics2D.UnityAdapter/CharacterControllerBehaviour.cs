using Physics2D.Core.Character;
using UnityEngine;

namespace Physics2D.UnityAdapter;

/// <summary>
/// Bridges Unity input to the core CharacterMotor.
/// </summary>
[RequireComponent(typeof(PhysicsBodyBehaviour))]
public sealed class CharacterControllerBehaviour : MonoBehaviour
{
    private PhysicsBodyBehaviour _bodyBehaviour = null!;
    private CharacterMotor _motor = null!;
    private PhysicsWorldRunner _runner = null!;

    private void Awake()
    {
        _bodyBehaviour = GetComponent<PhysicsBodyBehaviour>();
        _runner = FindFirstObjectByType<PhysicsWorldRunner>();
    }

    private void Start()
    {
        _motor = new CharacterMotor(_bodyBehaviour.CoreBody);
    }

    private void FixedUpdate()
    {
        _motor.ConsumeContacts(_runner.World.LastContacts);

        var horizontal = Input.GetAxisRaw("Horizontal");
        var jumpPressed = Input.GetButtonDown("Jump");
        var jumpHeld = Input.GetButton("Jump");

        _motor.TickInput(horizontal, jumpPressed, jumpHeld, Time.fixedDeltaTime);
    }
}
