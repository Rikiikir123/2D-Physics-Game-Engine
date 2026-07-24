using Engine.Physics.Controllers;
using UnityEngine;

/// <summary>
/// Feeds legacy Input Manager keys into RPlayerController each FixedUpdate (before the world steps).
/// Requires RBodyView on the same GameObject.
/// </summary>
[DefaultExecutionOrder(-100)] // before RPhysicsWorldBehaviour
[RequireComponent(typeof(RBodyView))]
public class RPlayerInputBehaviour : MonoBehaviour
{
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode leftAlt = KeyCode.LeftArrow;
    public KeyCode rightAlt = KeyCode.RightArrow;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode jumpAlt = KeyCode.W;

    private RBodyView bodyView;
    private RPlayerController controller;
    private bool jumpPressed;

    private void Start()
    {
        bodyView = GetComponent<RBodyView>();
    }

    private void Update()
    {
        // edge-detect jump on render frames; consume in FixedUpdate
        if (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpAlt) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        if (bodyView == null || bodyView.Body == null)
        {
            return;
        }

        if (controller == null)
        {
            bodyView.Body.CanSleep = false;
            bodyView.Body.Restitution = 0f;
            controller = new RPlayerController(bodyView.Body);
        }

        bool moveLeft = Input.GetKey(leftKey) || Input.GetKey(leftAlt);
        bool moveRight = Input.GetKey(rightKey) || Input.GetKey(rightAlt);
        bool jumpHeld = Input.GetKey(jumpKey) || Input.GetKey(jumpAlt) || Input.GetKey(KeyCode.UpArrow);

        controller.ApplyInput(moveLeft, moveRight, jumpPressed, jumpHeld, Time.fixedDeltaTime);
        jumpPressed = false;
    }
}
