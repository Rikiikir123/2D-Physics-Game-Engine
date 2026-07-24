using Engine.Physics;
using UnityEngine;

/// <summary>
/// Registers a static AABB from this Transform. By default, width/height come from the SpriteRenderer
/// so scaling the object in the Scene view drives physics size automatically.
/// </summary>
[DefaultExecutionOrder(-50)]
public class RStaticColliderView : MonoBehaviour
{
    [Tooltip("If on, physics size is read from the SpriteRenderer (recommended). If off, use Manual Width/Height.")]
    public bool syncSizeFromSprite = true;

    [Header("Manual size (only when Sync Size From Sprite is off)")]
    public float manualWidth = 1f;
    public float manualHeight = 1f;

    [Header("Collider")]
    public bool isOneWay = false;
    public bool isTrigger = false;
    public string tagName = "";
    public bool isMoving = false;
    public Vector2 engineVelocity;
    public float pathMin;
    public float pathMax;

    public RStaticCollider Collider { get; private set; }

    public float Width { get; private set; }
    public float Height { get; private set; }

    private void Start()
    {
        RPhysicsWorldBehaviour worldBehaviour = RPhysicsWorldBehaviour.Instance;
        if (worldBehaviour == null)
        {
            Debug.LogError("RStaticColliderView requires an RPhysicsWorldBehaviour in the scene.");
            enabled = false;
            return;
        }

        ResolveSize();

        Engine.Math.RVector2 center = RUnityConvert.ToEngine(transform.position);
        float left = center.X - Width * 0.5f;
        float right = center.X + Width * 0.5f;
        float top = center.Y - Height * 0.5f;
        float bottom = center.Y + Height * 0.5f;

        Collider = new RStaticCollider(new RAABB(left, right, top, bottom), isOneWay);
        Collider.IsTrigger = isTrigger;
        Collider.Tag = tagName;

        if (isMoving)
        {
            Collider.Velocity = new Engine.Math.RVector2(engineVelocity.x, engineVelocity.y);
            Collider.PathMin = pathMin;
            Collider.PathMax = pathMax;
        }

        worldBehaviour.RegisterStaticCollider(Collider);
    }

    private void ResolveSize()
    {
        if (syncSizeFromSprite)
        {
            RUnitySize.GetWorldSize(gameObject, out float w, out float h);
            Width = w;
            Height = h;
        }
        else
        {
            Width = manualWidth;
            Height = manualHeight;
        }

        if (Width < 0.0001f) Width = 0.0001f;
        if (Height < 0.0001f) Height = 0.0001f;
    }

    // path reverse before the world steps
    private void FixedUpdate()
    {
        if (Collider == null || !Collider.Enabled || !Collider.IsMoving)
        {
            return;
        }

        if (Collider.PathMin == Collider.PathMax)
        {
            return;
        }

        if (Collider.Velocity.X != 0f)
        {
            if (Collider.Bounds.Left < Collider.PathMin || Collider.Bounds.Right > Collider.PathMax)
            {
                Collider.Velocity = new Engine.Math.RVector2(-Collider.Velocity.X, Collider.Velocity.Y);
            }
        }
        else if (Collider.Velocity.Y != 0f)
        {
            if (Collider.Bounds.Top < Collider.PathMin || Collider.Bounds.Bottom > Collider.PathMax)
            {
                Collider.Velocity = new Engine.Math.RVector2(Collider.Velocity.X, -Collider.Velocity.Y);
            }
        }
    }

    // sync sprite after the world has moved the collider
    private void LateUpdate()
    {
        if (Collider == null)
        {
            return;
        }

        if (!Collider.Enabled)
        {
            gameObject.SetActive(false);
            return;
        }

        float cx = (Collider.Bounds.Left + Collider.Bounds.Right) * 0.5f;
        float cy = (Collider.Bounds.Top + Collider.Bounds.Bottom) * 0.5f;
        transform.position = RUnityConvert.ToUnity(new Engine.Math.RVector2(cx, cy), transform.position.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float w;
        float h;
        if (Application.isPlaying)
        {
            w = Width;
            h = Height;
        }
        else if (syncSizeFromSprite)
        {
            RUnitySize.GetWorldSize(gameObject, out w, out h);
        }
        else
        {
            w = manualWidth;
            h = manualHeight;
        }

        Gizmos.color = isTrigger ? Color.yellow : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(w, h, 0.01f));
    }
#endif
}
