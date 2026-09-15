using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HateZoneVisualizer : MonoBehaviour
{
    [Header("圈圈設定")]
    public int segments = 50;           // 圓形的平滑度
    public float currentRadius = 3f;    // 當前仇恨半徑
    public LayerMask playerLayer;       // 玩家 Layer

    [Header("渲染層級設定 (解決被地圖蓋住)")]
    public string sortingLayerName = "Default"; // 可設為 "Effects" 或 "Foreground"
    public int sortingOrder = 2;               // 數字高於地圖 (地圖為 0)

    [Header("狀態顏色")]
    public Color normalColor = new Color(1f, 0.9f, 0f, 0.5f); // 警戒黃
    public Color rageColor = new Color(1f, 0f, 0f, 0.8f);      // 暴走紅

    private LineRenderer line;
    private bool isRaging = false;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        // 💡 關鍵修復 1：設定 Sorting Layer 與 Order in Layer (強制浮在地圖上方)
        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        // 💡 關鍵修復 2：防呆處理！若沒掛載 Material，自動賦予預設 2D Sprites 材質球，避免看不到
        if (line.material == null || line.material.name.Contains("Default-Material"))
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
        }

        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
    }

    void Update()
    {
        DrawCircle();
        CheckPlayerCollision();
    }

    void DrawCircle()
    {
        float x;
        float y;
        float angle = 0f;

        Color targetColor = isRaging ? rageColor : normalColor;
        line.startColor = targetColor;
        line.endColor = targetColor;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentRadius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * currentRadius;

            line.SetPosition(i, new Vector3(x, y, 0));
            angle += (360f / segments);
        }
    }

    void CheckPlayerCollision()
    {
        // 判定玩家是否踏入圈內
        Collider2D player = Physics2D.OverlapCircle(transform.position, currentRadius, playerLayer);
        
        if (player != null && !isRaging)
        {
            // 踩線！觸發追擊或扣除精神值 (Sanity)
            Debug.Log("踩入仇恨圈！觸發鬼魂追擊！");
        }
    }

    // 供 GhostAI 呼叫：切換為暴走狀態（如撕咬包裹時）
    public void SetRageState(bool rage, float newRadius)
    {
        isRaging = rage;
        currentRadius = newRadius;
    }
}