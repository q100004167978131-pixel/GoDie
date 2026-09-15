using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
   [Header("跟隨目標")]
    public Transform target;           // 拖入 Player 物件

    [Header("跟隨參數")]
    public float smoothSpeed = 5f;     // 跟隨平滑速度
    public Vector3 offset = new Vector3(0, 0, -10f); // 2D 攝影機 Z 軸偏移量

    [Header("地圖邊界限制 (防止露出地圖外)")]
    public bool enableBounds = true;   // 是否開啟邊界限制
    public Vector2 minBounds;          // 地圖左下角座標 (Min X, Min Y)
    public Vector2 maxBounds;          // 地圖右上角座標 (Max X, Max Y)

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 計算理想的跟隨位置
        Vector3 desiredPosition = target.position + offset;

        // 2. 如果啟用了邊界限制，手動計算並扣除攝影機半高與半寬
        if (enableBounds && cam != null)
        {
            // 攝影機垂直半高
            float camVertHalfSize = cam.orthographicSize;
            // 攝影機水平半寬 (視角半高 * 螢幕長寬比)
            float camHorizHalfSize = camVertHalfSize * cam.aspect;

            // 計算攝影機中心點允許移動的極限範圍
            float leftLimit = minBounds.x + camHorizHalfSize;
            float rightLimit = maxBounds.x - camHorizHalfSize;
            float bottomLimit = minBounds.y + camVertHalfSize;
            float topLimit = maxBounds.y - camVertHalfSize;

            // 夾制 (Clamp) 攝影機座標，確保不會超出邊界
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, leftLimit, rightLimit);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, bottomLimit, topLimit);
        }

        // 3. 💡 使用 Time.unscaledDeltaTime 進行平滑移動 (確保幽靈模式時間凍結下，鏡頭依然平滑跟隨)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.unscaledDeltaTime);

        transform.position = smoothedPosition;
    }

    // --- 在 Scene 編輯器視窗中畫出綠色地圖邊界線，方便調整設定 ---
    private void OnDrawGizmosSelected()
    {
        if (!enableBounds) return;

        Gizmos.color = Color.cyan;

        Vector3 borderBottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 borderTopLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 borderTopRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 borderBottomRight = new Vector3(maxBounds.x, minBounds.y, 0);

        Gizmos.DrawLine(borderBottomLeft, borderTopLeft);
        Gizmos.DrawLine(borderTopLeft, borderTopRight);
        Gizmos.DrawLine(borderTopRight, borderBottomRight);
        Gizmos.DrawLine(borderBottomRight, borderBottomLeft);
    }
}