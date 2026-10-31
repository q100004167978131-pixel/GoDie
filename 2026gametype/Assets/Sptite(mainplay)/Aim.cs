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
            lineRenderer.sortingLayerName = sortingLayerName;
            lineRenderer.sortingOrder = sortingOrder;

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

        // 1. 按下滑鼠右鍵開始瞄準
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
        Vector2 origin = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);

        Vector2 defaultTarget = origin + Vector2.ClampMagnitude(mouseWorldPos - origin, maxThrowDistance);

        Collider2D hit = Physics2D.OverlapCircle(defaultTarget, magnetRadius, targetLayer);

        if (hit != null)
        {
            currentTargetPos = hit.transform.position;
            lockedTarget = hit.transform;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = Color.cyan;
                lineRenderer.endColor = Color.cyan;
            }
        }
        else
        {
            currentTargetPos = defaultTarget;
            lockedTarget = null;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            }
        }

        if (lineRenderer != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, currentTargetPos);
        }
    }

    void ThrowPackage()
    {
        hasPackage = false;

        Vector2 spawnPos = throwPoint != null ? (Vector2)throwPoint.position : (Vector2)transform.position;

        // 生成業障包裹
        GameObject pkg = Instantiate(karmaPackagePrefab, spawnPos, Quaternion.identity);

        // 💡【關鍵新增】計算拋投方向向量並設定包裹的 8 方向 Sprite
        Vector2 throwDirection = (currentTargetPos - spawnPos).normalized;
        KarmaPackageDirection packageDir = pkg.GetComponent<KarmaPackageDirection>();
        if (packageDir != null)
        {
            packageDir.SetDirection(throwDirection);
        }

        // 初始化包裹目標點
        KarmaPackage packageScript = pkg.GetComponent<KarmaPackage>();
        if (packageScript != null)
        {
            packageScript.InitTarget(currentTargetPos, lockedTarget);
        }
    }

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