using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DestructibleHouse : MonoBehaviour
{
    public HouseDestructionProfile profile;
    public SpriteRenderer houseRenderer;
    public bool IsBroken { get; private set; }
    private GameObject ruinObject;
    private Vector3 originalPosition;
    private Coroutine transition;

    // Called at the existing successful ghost impact, not on ordinary collisions.
    public static bool TryBreakObstacle(Transform obstacle)
    {
        if (obstacle == null) return false;
        DestructibleHouse house = obstacle.GetComponentInParent<DestructibleHouse>();
        if (house == null) house = obstacle.GetComponentInChildren<DestructibleHouse>();
        if (house != null) return house.Break();

        // Existing houses using the supplied intact sprite work without scene edits.
        HouseDestructionProfile defaults = Resources.Load<HouseDestructionProfile>("House5Destruction");
        if (defaults == null || defaults.intactSprite == null || defaults.ruinedSprite == null) return false;
        SpriteRenderer renderer = obstacle.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite != defaults.intactSprite)
        {
            renderer = null;
            foreach (SpriteRenderer candidate in obstacle.GetComponentsInChildren<SpriteRenderer>())
                if (candidate.sprite == defaults.intactSprite) { renderer = candidate; break; }
        }
        if (renderer == null) return false;
        house = obstacle.gameObject.AddComponent<DestructibleHouse>();
        house.profile = defaults;
        house.houseRenderer = renderer;
        return house.Break();
    }

    public bool Break()
    {
        if (IsBroken) return true;
        if (houseRenderer == null) houseRenderer = GetComponentInChildren<SpriteRenderer>();
        if (profile == null || profile.ruinedSprite == null || houseRenderer == null || houseRenderer.sprite == null)
        {
            Debug.LogWarning("【房子破壞】缺少房子圖片或廢墟設定，使用原本移除障礙物的行為。", this);
            return false;
        }
        IsBroken = true;
        originalPosition = houseRenderer.transform.localPosition;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        // Keep the rubble visible, but remove it from obstacle searches and wall checks.
        foreach (Transform part in GetComponentsInChildren<Transform>())
        {
            part.gameObject.layer = 0;
            if (part.CompareTag("Wall")) part.tag = "Untagged";
        }
        Animator animator = houseRenderer.GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
        transition = StartCoroutine(PlayDestruction());
        return true;
    }

    private IEnumerator PlayDestruction()
    {
        ruinObject = new GameObject("House Ruins");
        ruinObject.transform.SetParent(houseRenderer.transform, false);
        SpriteRenderer ruins = ruinObject.AddComponent<SpriteRenderer>();
        ruins.sprite = profile.ruinedSprite;
        ruins.sharedMaterial = houseRenderer.sharedMaterial;
        ruins.sortingLayerID = houseRenderer.sortingLayerID;
        ruins.sortingOrder = houseRenderer.sortingOrder;
        ruins.flipX = houseRenderer.flipX;
        ruins.flipY = houseRenderer.flipY;
        ruins.maskInteraction = houseRenderer.maskInteraction;
        // The two supplied PNGs have different resolutions/PPU. Match world width
        // and the bottom edge instead of inheriting the ruins' raw pixel size.
        Bounds oldBounds = houseRenderer.sprite.bounds;
        Bounds newBounds = ruins.sprite.bounds;
        float scale = oldBounds.size.x / Mathf.Max(0.001f, newBounds.size.x);
        ruinObject.transform.localScale = Vector3.one * scale;
        float oldBottom = houseRenderer.flipY ? -oldBounds.max.y : oldBounds.min.y;
        float newBottom = houseRenderer.flipY ? -newBounds.max.y : newBounds.min.y;
        float oldCenter = houseRenderer.flipX ? -oldBounds.center.x : oldBounds.center.x;
        float newCenter = houseRenderer.flipX ? -newBounds.center.x : newBounds.center.x;
        ruinObject.transform.localPosition = new Vector3(oldCenter - newCenter * scale, oldBottom - newBottom * scale, 0f);

        Color originalColor = houseRenderer.color;
        float elapsed = 0f;
        float duration = Mathf.Max(0f, profile.transitionDuration);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Color intactColor = originalColor;
            intactColor.a *= 1f - t;
            houseRenderer.color = intactColor;
            Color ruinColor = originalColor;
            ruinColor.a *= t;
            ruins.color = ruinColor;
            float shake = Mathf.Sin(t * Mathf.PI * 18f) * profile.shakeDistance * (1f - t);
            houseRenderer.transform.localPosition = originalPosition + Vector3.right * shake;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        houseRenderer.transform.localPosition = originalPosition;
        houseRenderer.color = originalColor;
        houseRenderer.enabled = false;
        ruins.color = originalColor;
        transition = null;
    }

    private void OnDisable()
    {
        if (transition == null || houseRenderer == null) return;
        StopCoroutine(transition);
        transition = null;
        houseRenderer.transform.localPosition = originalPosition;
        houseRenderer.enabled = false;
        if (ruinObject != null)
        {
            SpriteRenderer ruins = ruinObject.GetComponent<SpriteRenderer>();
            if (ruins != null) { Color color = ruins.color; color.a = 1f; ruins.color = color; }
        }
    }
}
