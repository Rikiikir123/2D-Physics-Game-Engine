using UnityEngine;

/// <summary>
/// Reads how big an object appears in the Unity scene (world units) so physics matches the visual.
/// Prefers SpriteRenderer.bounds; falls back to Transform lossy scale if no sprite.
/// </summary>
public static class RUnitySize
{
    public static void GetWorldSize(GameObject go, out float width, out float height)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // bounds already include transform scale and sprite pixels-per-unit
            Bounds b = sr.bounds;
            width = Mathf.Abs(b.size.x);
            height = Mathf.Abs(b.size.y);
            if (width > 0.0001f && height > 0.0001f)
            {
                return;
            }
        }

        // fallback: treat localScale as world size (works for a 1x1 unit quad/sprite)
        Vector3 s = go.transform.lossyScale;
        width = Mathf.Abs(s.x);
        height = Mathf.Abs(s.y);
    }
}
