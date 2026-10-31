using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HateZoneVisualizer : MonoBehaviour
{
    [Header("錐形視野設定")]
    public float currentRadius = 1f;        // 視野距離 (完全使用你在 Inspector 設定的值)
    [Range(10f, 180f)]
    public float viewAngle = 45f;           // 視野角度 (設為 45 度)
    public int segments = 20;               // 畫線平滑度

    [Header("層級與遮擋設定")]
    public LayerMask playerLayer;           // 玩家 Layer
    public LayerMask obstacleLayer;         // 牆壁 / 建築物 Layer (阻擋視線用)

    [Header("渲染層級設定")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 2;

    [Header("狀態顏色")]
    public Color normalColor = new Color(1f, 0.9f, 0f, 0.4f); // 警戒黃
    public Color rageColor = new Color(1f, 0f, 0f, 0.7f);      // 暴走紅

    private LineRenderer line;
    private bool isRaging = false;
    private bool isChasingPlayer = false;

    // 儲存你在 Inspector 設定的原生長度
    private float baseRadius;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        if (line.material == null || line.material.name.Contains("Default-Material"))
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
        }

        line.useWorldSpace = false;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;

        // 紀錄你在 Inspector 手動設定的真實長度
        baseRadius = currentRadius;
    }

    void Update()
    {
        DrawConeOfSight();
        CheckPlayerInCone();
    }

    // 💡 繪製 45 度錐形視野導引線
    void DrawConeOfSight()
    {
        line.positionCount = segments + 2; // 原點 + 弧線點 + 回原點

        Color targetColor = isRaging ? rageColor : normalColor;
        line.startColor = targetColor;
        line.endColor = targetColor;

        // 起點：厲鬼中心 (0,0,0)
        line.SetPosition(0, Vector3.zero);

        float halfAngle = viewAngle / 2f;
        float startAngle = -halfAngle;
        float angleStep = viewAngle / segments;

        // 朝向角度 (預設取 transform.up 作為正面方向)
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            
            // 計算錐形邊界點的 local 座標
            Vector3 rayDir = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;
            Vector3 point = rayDir * currentRadius;

            line.SetPosition(i + 1, point);
        }
    }

    // 💡 45 度扇形 + 建築物遮擋 (Raycast) 檢測
    void CheckPlayerInCone()
    {
        // 1. 在距離內搜尋玩家
        Collider2D playerCol = Physics2D.OverlapCircle(transform.position, currentRadius, playerLayer);

        bool canSeePlayer = false;

        if (playerCol != null)
        {
            Vector2 dirToPlayer = (playerCol.transform.position - transform.position).normalized;

            // 2. 角度檢測：判定玩家是否在厲鬼正前方 45 度夾角內
            float angleToPlayer = Vector2.Angle(transform.up, dirToPlayer);

            if (angleToPlayer <= viewAngle / 2f)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerCol.transform.position);

                // 3. 視線遮擋檢測 (Raycast)：從厲鬼射向玩家，檢查中途是否有建築物/牆壁
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleLayer);

                if (hit.collider == null)
                {
                    // 中途無建築物遮擋，成功看見玩家！
                    canSeePlayer = true;
                }
                else
                {
                    Debug.Log($"<color=yellow>【厲鬼】視線被建築物 [{hit.collider.name}] 擋住！</color>");
                }
            }
        }

        // 4. 狀態切換與追擊控制
        if (canSeePlayer)
        {
            if (!isChasingPlayer)
            {
                isChasingPlayer = true;
                Debug.Log("<color=red>【厲鬼】進入 45 度視野，發動追擊！</color>");
            }
        }
        else
        {
            // 玩家跑進建築物後方，或離開 45 度視野
            if (isChasingPlayer && !isRaging)
            {
                isChasingPlayer = false;
                Debug.Log("<color=green>【厲鬼】失去玩家視線 (躲入建築物)，停止追擊。</color>");
            }
        }
    }

    // 💡 修復關鍵：攔截並鎖定 GhostAI 傳進來的半徑，嚴格遵守你在 Inspector 設定的值
    public void SetRageState(bool rage, float overrideRadius)
    {
        isRaging = rage;

        if (rage)
        {
            // 只有在暴走/咬包裹時，才允許改動半徑 (若不想改變，可直接改為 currentRadius = baseRadius)
            currentRadius = overrideRadius;
        }
        else
        {
            // 恢復正常巡邏時，強制還原為你在 Inspector 填寫的初始長度 (如 1.0)
            currentRadius = baseRadius;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Scene 視窗畫出 45 度錐形檢測範圍
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.up;
        Vector3 leftDir = Quaternion.Euler(0, 0, viewAngle / 2f) * forward;
        Vector3 rightDir = Quaternion.Euler(0, 0, -viewAngle / 2f) * forward;

        Gizmos.DrawRay(transform.position, leftDir * currentRadius);
        Gizmos.DrawRay(transform.position, rightDir * currentRadius);
    }
}