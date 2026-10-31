using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)]
public class PlayerVisionController : MonoBehaviour
{
    [Header("玩家可視範圍（半徑）")]
    [Min(0.01f)] public float visionRadiusInCells = 3f;
    [Tooltip("一格對應的世界距離；目前地圖沒有 Grid，預設一格為 1 Unity 單位。")]
    [Min(0.001f)] public float cellWorldSize = 1f;
    [Tooltip("留空時自動使用 Main Camera。")]
    public Camera visionCamera;

    // Keep the old serialized references so existing player prefabs still load.
    [HideInInspector] public float normalVisionScale = 6f;
    [HideInInspector] public float chasingVisionScale = 10f;
    [HideInInspector] public Transform visionMaskTransform;
    [HideInInspector] public LayerMask ghostLayer;

    private GameObject overlayObject;
    private RectTransform overlayRect;
    private PlayerVisionDarknessGraphic darkness;
    private bool legacyMaskWasActive;

    public float RadiusWorld => Mathf.Max(0.01f, visionRadiusInCells * cellWorldSize);

    private void OnEnable()
    {
        // The old SpriteMask must not hide sprites inside the new visible circle.
        if (visionMaskTransform != null && visionMaskTransform != transform &&
            visionMaskTransform.IsChildOf(transform))
        {
            legacyMaskWasActive = visionMaskTransform.gameObject.activeSelf;
            visionMaskTransform.gameObject.SetActive(false);
        }
        if (overlayObject != null) overlayObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (visionCamera == null || !visionCamera.isActiveAndEnabled) visionCamera = Camera.main;
        if (visionCamera == null) return;
        if (overlayObject == null) CreateOverlay();

        // Use world distance rather than player localScale; respawn, ghost movement,
        // camera smoothing, zoom and timeScale=0 all keep the same world-space radius.
        Vector3 centerScreen = visionCamera.WorldToScreenPoint(transform.position);
        Vector3 edgeScreen = visionCamera.WorldToScreenPoint(transform.position + Vector3.right * RadiusWorld);
        Vector3 topScreen = visionCamera.WorldToScreenPoint(transform.position + Vector3.up * RadiusWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, centerScreen, null, out Vector2 center);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, edgeScreen, null, out Vector2 edge);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, topScreen, null, out Vector2 top);
        darkness.SetView(center, edge - center, top - center, centerScreen.z > 0f);
    }

    private void CreateOverlay()
    {
        overlayObject = new GameObject("Player Vision Darkness", typeof(RectTransform), typeof(Canvas));
        // Keep this Canvas at the scene root so player rotation/scale cannot distort it.
        Canvas canvas = overlayObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1000; // World is blacked out; existing screen UI stays readable.
        canvas.targetDisplay = visionCamera.targetDisplay;

        GameObject graphicObject = new GameObject("Darkness", typeof(RectTransform), typeof(CanvasRenderer));
        overlayRect = graphicObject.GetComponent<RectTransform>();
        overlayRect.SetParent(overlayObject.transform, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        darkness = graphicObject.AddComponent<PlayerVisionDarknessGraphic>();
        darkness.color = Color.black;
        darkness.raycastTarget = false;
    }

    private void OnDisable()
    {
        if (overlayObject != null) overlayObject.SetActive(false);
        if (legacyMaskWasActive && visionMaskTransform != null)
            visionMaskTransform.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (overlayObject != null) Destroy(overlayObject);
    }

    private void OnValidate()
    {
        visionRadiusInCells = Mathf.Max(0.01f, visionRadiusInCells);
        cellWorldSize = Mathf.Max(0.001f, cellWorldSize);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, RadiusWorld);
    }
}

// A solid black ring around a transparent opening. No texture, lighting setup,
// custom shader, SpriteRenderer sorting or render-pipeline setting is required.
public class PlayerVisionDarknessGraphic : MaskableGraphic
{
    private const int Segments = 128;
    private Vector2 center;
    private Vector2 horizontal;
    private Vector2 vertical;
    private bool showOpening;

    public void SetView(Vector2 newCenter, Vector2 newHorizontal, Vector2 newVertical, bool visible)
    {
        if (center == newCenter && horizontal == newHorizontal && vertical == newVertical && showOpening == visible) return;
        center = newCenter;
        horizontal = newHorizontal;
        vertical = newVertical;
        showOpening = visible;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper helper)
    {
        helper.Clear();
        Rect bounds = rectTransform.rect;
        // Enclose every screen corner even when the player is off-center/off-screen.
        float outerRadius = (bounds.size.magnitude + center.magnitude + horizontal.magnitude + vertical.magnitude + 1f) * 2f;
        Color32 black = new Color32(0, 0, 0, 255);
        for (int i = 0; i <= Segments; i++)
        {
            float angle = i * (Mathf.PI * 2f / Segments);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 inner = showOpening ? horizontal * direction.x + vertical * direction.y : Vector2.zero;
            helper.AddVert(center + inner, black, Vector2.zero);
            helper.AddVert(center + direction * outerRadius, black, Vector2.zero);
            if (i == Segments) continue;
            int vertex = i * 2;
            helper.AddTriangle(vertex, vertex + 1, vertex + 3);
            helper.AddTriangle(vertex, vertex + 3, vertex + 2);
        }
    }
}
