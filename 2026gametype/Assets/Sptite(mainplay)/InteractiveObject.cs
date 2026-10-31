using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("物件設定")]
    public int objectID = 0;              // 💡 務必在 Inspector 設定：第一個設 0，第二個設 1，第三個設 2
    public float interactDistance = 2.0f;  // 互動距離
    public string interactPrompt = "按 【F】 選擇此物件";
    public bool requireGhostState = false; // 是否限制幽靈狀態

    [Header("所屬群組管理器 (可手動拉入)")]
    public ChoiceGroupManager groupManager;

    private Transform playerTransform;
    private GhostMode ghostMode;
    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;
    private bool wasInDistance = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();

        // 防呆：如果沒手動拉，自動從父物件或場景找 ChoiceGroupManager
        if (groupManager == null)
        {
            groupManager = GetComponentInParent<ChoiceGroupManager>();
        }
    }

    void Update()
    {
        // 1. 動態搜尋玩家
        if (playerTransform == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                playerTransform = pObj.transform;
                ghostMode = pObj.GetComponent<GhostMode>();
            }
            return;
        }

        // 2. 如果物件已被隱藏則不偵測
        if (spriteRenderer != null && !spriteRenderer.enabled) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool isGhost = (ghostMode != null && ghostMode.IsGhostState());

        // 3. 判斷是否滿足互動條件
        if (dist <= interactDistance && (!requireGhostState || isGhost))
        {
            if (!wasInDistance && UIManager.Instance != null)
            {
                UIManager.Instance.ShowTip(interactPrompt);
                wasInDistance = true;
            }

            // 按下 F 鍵選擇
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (UIManager.Instance != null) UIManager.Instance.HideTip();
                wasInDistance = false;

                if (groupManager != null)
                {
                    // 💡 關鍵修復：直接將自己的 objectID 傳過去，絕對不會再被寫死成 0
                    groupManager.SelectObjectWithID(this, objectID);
                }
                else
                {
                    Debug.LogError($"【錯誤】{gameObject.name} 沒有綁定 ChoiceGroupManager！");
                }
            }
        }
        else
        {
            if (wasInDistance && UIManager.Instance != null)
            {
                UIManager.Instance.HideTip();
                wasInDistance = false;
            }
        }
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (col2D != null) col2D.enabled = visible;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}