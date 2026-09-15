using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aim : MonoBehaviour
{
    [Header("拋投設定")]
    public Transform throwPoint;       // 拋投起始點
    public float maxThrowDistance = 8f; // 最大拋投距離
    public float magnetRadius = 2.5f;   // 磁吸搜尋半徑
    public LayerMask targetLayer;      // 可破壞危牆/機關的 Layer
    public GameObject karmaPackagePrefab; // 業障包裹 Prefab

    [Header("包裹數量狀態")]
    public bool hasPackage = true;     // 手頭是否有包裹 (預設為 true)

    [Header("視覺呈現")]
    public LineRenderer lineRenderer;   // 螢光導引線 LineRenderer
    public string sortingLayerName = "Default"; // 圖層名稱 (可改成 Foreground/UI)
    public int sortingOrder = 10;               // 數字一定要大於地圖 (地圖預設是 0)

    private Vector2 currentTargetPos;
    private bool isAiming = false;
    private Transform lockedTarget = null;

    void Awake()
    {
        // 防呆初始化 LineRenderer
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            // 💡 關鍵修復 1：強制將導引線的渲染層級拉到最高 (防止被地圖/Tilemap 蓋住)
            lineRenderer.sortingLayerName = sortingLayerName;
            lineRenderer.sortingOrder = sortingOrder;

            // 💡 關鍵修復 2：若沒指定 Material，自動賦予預設 2D Sprites 材質球
            if (lineRenderer.material == null || lineRenderer.material.name.Contains("Default-Material"))
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.enabled = false; // 初始隱藏
        }
    }

    void Update()
    {
        // 如果手頭沒有包裹，無法進行瞄準與拋投
        if (!hasPackage)
        {
            if (lineRenderer != null && lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                isAiming = false;
            }
            return;
        }

        // 1. 按下滑鼠右鍵 (MouseButton 1) 開始瞄準
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            if (lineRenderer != null) lineRenderer.enabled = true;
        }

        // 2. 瞄準狀態中：持續更新導引線與磁吸檢測
        if (isAiming)
        {
            UpdateTrajectory();
        }

        // 3. 放開滑鼠右鍵，精準拋投包裹
        if (Input.GetMouseButtonUp(1))
        {
            if (isAiming)
            {
                ThrowPackage();
                isAiming = false;
                if (lineRenderer != null) lineRenderer.enabled = false;
            }
        }
    }

    void UpdateTrajectory()
    {
        // 確保 throwPoint 有拉入，否則以玩家本體座標取代
        Vector2 origin = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;

        // 取得滑鼠在世界座標的位置 (加入相機 Z 軸補償)
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        // 預設瞄準位置（限制在 maxThrowDistance 範圍內）
        Vector2 defaultTarget = origin + Vector2.ClampMagnitude(mouseWorldPos - origin, maxThrowDistance);

        // 磁吸檢測：在預設目標點附近搜尋可鎖定的障礙物/危牆
        Collider2D hit = Physics2D.OverlapCircle(defaultTarget, magnetRadius, targetLayer);

        if (hit != null)
        {
            // 磁吸成功！將目標位置強行吸附至危牆/機關中心
            currentTargetPos = hit.transform.position;
            lockedTarget = hit.transform;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = Color.cyan; // 鎖定時顯示亮青色
                lineRenderer.endColor = Color.cyan;
            }
        }
        else
        {
            // 未磁吸：使用普通彈道
            currentTargetPos = defaultTarget;
            lockedTarget = null;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = Color.green; // 未鎖定顯示螢光綠
                lineRenderer.endColor = Color.green;
            }
        }

        // 繪製螢光導引線
        if (lineRenderer != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, currentTargetPos);
        }
    }

    void ThrowPackage()
    {
        // 拋出包裹後，將手頭狀態設為 false (無法連續拋投)
        hasPackage = false;

        Vector2 spawnPos = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;

        // 生成業障包裹並傳入目標點
        GameObject pkg = Instantiate(karmaPackagePrefab, spawnPos, Quaternion.identity);
        KarmaPackage packageScript = pkg.GetComponent<KarmaPackage>();
        
        if (packageScript != null)
        {
            packageScript.InitTarget(currentTargetPos, lockedTarget);
        }
    }

    // 由 KarmaPackage 在玩家撿起包裹時呼叫
    public void OnPackagePickedUp()
    {
        hasPackage = true;
        Debug.Log("【玩家】包裹已重新裝填，可再次拋投！");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = throwPoint != null ? throwPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, maxThrowDistance);
    }
}