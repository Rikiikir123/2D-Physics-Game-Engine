using Engine.Math;
using Engine.Physics.Bodies;
using Engine.Physics.Shapes;
using UnityEngine;
using static Engine.Physics.Shapes.RShape;

/// <summary>
/// Creates an RRigidBody and syncs its engine position to this Transform each FixedUpdate.
/// By default, physics width/height/radius are taken from the SpriteRenderer's world size
/// so scaling the object in the Scene view is enough — no separate Width/Height typing.
/// </summary>
[DefaultExecutionOrder(100)] // after RPhysicsWorldBehaviour (default 0)
public class RBodyView : MonoBehaviour
{
    public enum ShapeType
    {
        Rectangle,
        Circle
    }

    [Header("Body")]
    public ShapeType shapeType = ShapeType.Rectangle;

    [Tooltip("If on, physics size is read from the SpriteRenderer (recommended). If off, use Manual Width/Height/Radius.")]
    public bool syncSizeFromSprite = true;

    [Header("Manual size (only when Sync Size From Sprite is off)")]
    public float manualWidth = 1f;
    public float manualHeight = 1f;
    public float manualRadius = 0.5f;

    [Header("Physics")]
    public float mass = 10f;
    public bool useGravity = true;
    public bool isStatic = false;
    public bool canSleep = true;
    public float restitution = 0f;

    [Header("Sync")]
    public float z = 0f;

    public RRigidBody Body { get; private set; }

    // resolved size used by physics (world units = engine units)
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float Radius { get; private set; }

    private void Start()
    {
        RPhysicsWorldBehaviour worldBehaviour = RPhysicsWorldBehaviour.Instance;
        if (worldBehaviour == null)
        {
            Debug.LogError("RBodyView requires an RPhysicsWorldBehaviour in the scene.");
            enabled = false;
            return;
        }

        ResolveSize();

        RVector2 engineCenter = RUnityConvert.ToEngine(transform.position);
        RVector2 topLeft = ShapeTopLeftFromCenter(engineCenter);
        RShape shape = shapeType == ShapeType.Circle
            ? (RShape)new RCircleShape(Radius)
            : new RRectangleShape(Width, Height);

        Body = new RRigidBody(topLeft, shape, mass, isStatic, useGravity);
        Body.CanSleep = canSleep;
        Body.Restitution = restitution;

        worldBehaviour.RegisterBody(Body);
    }

    private void ResolveSize()
    {
        if (syncSizeFromSprite)
        {
            RUnitySize.GetWorldSize(gameObject, out float w, out float h);
            Width = w;
            Height = h;
            Radius = 0.5f * Mathf.Max(w, h);
        }
        else
        {
            Width = manualWidth;
            Height = manualHeight;
            Radius = manualRadius;
        }

        if (Width < 0.0001f) Width = 0.0001f;
        if (Height < 0.0001f) Height = 0.0001f;
        if (Radius < 0.0001f) Radius = 0.0001f;
    }

    private void FixedUpdate()
    {
        if (Body == null)
        {
            return;
        }

        RVector2 engineCenter = ShapeCenterFromTopLeft(Body.Position);
        transform.position = RUnityConvert.ToUnity(engineCenter, z);
    }

    private RVector2 ShapeTopLeftFromCenter(RVector2 center)
    {
        if (shapeType == ShapeType.Circle)
        {
            return new RVector2(center.X - Radius, center.Y - Radius);
        }

        return new RVector2(center.X - Width * 0.5f, center.Y - Height * 0.5f);
    }

    private RVector2 ShapeCenterFromTopLeft(RVector2 topLeft)
    {
        if (shapeType == ShapeType.Circle)
        {
            return new RVector2(topLeft.X + Radius, topLeft.Y + Radius);
        }

        return new RVector2(topLeft.X + Width * 0.5f, topLeft.Y + Height * 0.5f);
    }

#if UNITY_EDITOR
    // draws the physics box in the Scene view so you can verify it matches the sprite
    private void OnDrawGizmosSelected()
    {
        float w = Width;
        float h = Height;
        float r = Radius;

        if (!Application.isPlaying)
        {
            if (syncSizeFromSprite)
            {
                RUnitySize.GetWorldSize(gameObject, out w, out h);
                r = 0.5f * Mathf.Max(w, h);
            }
            else
            {
                w = manualWidth;
                h = manualHeight;
                r = manualRadius;
            }
        }

        Gizmos.color = Color.yellow;
        if (shapeType == ShapeType.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, r);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(w, h, 0.01f));
        }
    }
#endif
}
