using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GhostMode : MonoBehaviour
{
    [Header("血條與心形 UI 設定")]
    public int maxHealth = 3;                 // 最大血量 (3顆心)
    public Image healthUIImage;              // Canvas 上的血條 UI (Image 元件)
    public Sprite[] heartSprites;            // 血條圖片陣列 (請依序放入：3心、2心、1心、0心)

    [Header("幽靈型態設定")]
    public float ghostDuration = 30f;        // 幽靈型態持續時間 (30秒)
    public float ghostMoveSpeed = 5f;        // 幽靈型態下的移動速度
    public Sprite ghostSprite;              // 幽靈視覺 (半透明美術圖)
    public LayerMask solidObstacleLayer;     // 普通牆壁/障礙物 Layer (幽靈撞到會被擋住)

    [Header("存檔與重生點設定")]
    public Transform currentCheckpoint;     // 目前的存檔點/重生點
    public GameObject deadBodyPrefab;       // 玩家死亡留在地上的肉身/遺體 Prefab

    [Header("倒數 UI 顯示")]
    public GameObject timerPanel;           // 倒數 UI 背景面板
    public TMPro.TextMeshProUGUI timerText; // 畫面白倒數 UI 文字

    private int currentHealth;               // 目前剩餘血量
    private bool isGhost = false;
    private float currentTimer = 0f;

    private GameObject spawnedBodyInstance;  // 留在死亡地點的肉身物件
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private PlayerMove playerMoveScript;
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private Color originalTimerColor = Color.white;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[5];
    private List<string> ghostCollectedItems = new List<string>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalSprite = spriteRenderer.sprite;
        
        playerMoveScript = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        if (timerText != null)
        {
            originalTimerColor = timerText.color;
        }

        // 初始化血量為滿血 (3顆心)
        currentHealth = maxHealth;
        UpdateHealthUI();

        // 初始隱藏倒數 UI
        UpdateTimerUIState(false);
    }

    void Update()
    {
        if (isGhost)
        {
            // 1. 在 Time.timeScale = 0 下進行手動物理檢測位移
            HandleGhostMovementWithCollision();

            // 2. 倒數計時 (使用 unscaledDeltaTime)
            currentTimer -= Time.unscaledDeltaTime;

            // 更新倒數 UI 文字與最後 5 秒閃爍
            UpdateTimerUI();

            // 30 秒倒數結束，回到存檔點重生
            if (currentTimer <= 0f)
            {
                RespawnAtCheckpoint();
            }
        }
    }

    // 💡 更新血條 UI 圖片 (根據目前血量更換 Sprite)
    void UpdateHealthUI()
    {
        if (healthUIImage != null && heartSprites != null && heartSprites.Length > 0)
        {
            // 計算圖片 Index (例如: 3心對應 Index 0, 2心對應 Index 1...)
            int spriteIndex = Mathf.Clamp(maxHealth - currentHealth, 0, heartSprites.Length - 1);
            healthUIImage.sprite = heartSprites[spriteIndex];
        }
    }

    // --- 觸發死亡與扣血邏輯 ---
    public void Die()
    {
        if (isGhost) return; // 幽靈狀態下不重複觸發死亡

        // 1. 扣掉 1 顆心
        currentHealth--;
        UpdateHealthUI();

        Debug.Log($"<color=red>【玩家受到傷害】剩餘血量: {currentHealth}/{maxHealth}</color>");

        // 2. 判定是否徹底死亡
        if (currentHealth <= 0)
        {
            Debug.Log("<color=red><b>【完全死亡】3 顆心已耗盡！重置教學關卡...</b></color>");
            RestartTutorialLevel();
        }
        else
        {
            Debug.Log("<color=orange>【進入幽靈模式】進入 30 秒倒數...</color>");
            EnterGhostMode();
        }
    }

    // 耗盡 3 顆心，徹底重置教學關卡
    void RestartTutorialLevel()
    {
        Time.timeScale = 1f; // 確保恢復時間正常
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name); // 重新載入當前教學關卡
    }

    void EnterGhostMode()
    {
        isGhost = true;
        currentTimer = ghostDuration;
        ghostCollectedItems.Clear();

        // 顯示倒數 UI
        UpdateTimerUIState(true);

        // 1. 關閉原本的 PlayerMove 腳本與物理速度
        if (playerMoveScript != null) playerMoveScript.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // 2. 在死亡地點留下肉身遺體
        if (deadBodyPrefab != null)
        {
            spawnedBodyInstance = Instantiate(deadBodyPrefab, transform.position, Quaternion.identity);
        }

        // 3. 切換為半透明幽靈視覺
        if (spriteRenderer != null)
        {
            if (ghostSprite != null) spriteRenderer.sprite = ghostSprite;
            Color ghostColor = spriteRenderer.color;
            ghostColor.a = 0.5f;
            spriteRenderer.color = ghostColor;
        }

        // 4. 凍結整個遊戲世界 (時間暫停)
        Time.timeScale = 0f;
    }

    // --- 30 秒倒數結束，於存檔點重生 ---
    void RespawnAtCheckpoint()
    {
        isGhost = false;

        // 隱藏倒數 UI 並重置顏色
        UpdateTimerUIState(false);
        if (timerText != null)
        {
            timerText.color = originalTimerColor;
            timerText.text = "";
        }

        // 1. 恢復遊戲世界時間正常運作
        Time.timeScale = 1f;

        // 2. 還原物理與原本的 PlayerMove 腳本
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        if (playerMoveScript != null) playerMoveScript.enabled = true;

        // 3. 移至存檔點
        if (currentCheckpoint != null)
        {
            transform.position = currentCheckpoint.position;
        }

        // 4. 銷毀現場的遺體
        if (spawnedBodyInstance != null)
        {
            Destroy(spawnedBodyInstance);
        }

        // 5. 恢復肉身視覺與角度
        if (spriteRenderer != null)
        {
            if (originalSprite != null) spriteRenderer.sprite = originalSprite;
            Color normalColor = spriteRenderer.color;
            normalColor.a = 1.0f;
            spriteRenderer.color = normalColor;
        }

        transform.rotation = Quaternion.identity;

        ConsolidateInventory();

        Debug.Log("<color=green>【重生成功】玩家於存檔點恢復肉身！</color>");
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(currentTimer));
        timerText.text = $"靈魂狀態剩餘: {remainingSeconds}s";

        if (currentTimer <= 5f)
        {
            float alpha = Mathf.PingPong(Time.unscaledTime * 4f, 1f);
            Color warningColor = Color.red;
            warningColor.a = Mathf.Clamp(alpha, 0.3f, 1f);
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = originalTimerColor;
        }
    }

    void UpdateTimerUIState(bool show)
    {
        if (timerPanel != null) timerPanel.SetActive(show);
        else if (timerText != null) timerText.gameObject.SetActive(show);
    }

    void HandleGhostMovementWithCollision()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(moveX, moveY).normalized;

        if (moveDir == Vector2.zero) return;

        float moveDistance = ghostMoveSpeed * Time.unscaledDeltaTime;

        bool canMove = true;
        if (playerCollider != null)
        {
            int count = playerCollider.Cast(moveDir, hitBuffer, moveDistance);
            for (int i = 0; i < count; i++)
            {
                GameObject hitObj = hitBuffer[i].collider.gameObject;
                if (hitObj != gameObject && hitObj.GetComponent<GhostWall>() == null)
                {
                    if (((1 << hitObj.layer) & solidObstacleLayer) != 0 || hitBuffer[i].collider.CompareTag("Wall"))
                    {
                        canMove = false;
                        break;
                    }
                }
            }
        }

        if (canMove)
        {
            transform.Translate(moveDir * moveDistance, Space.World);
        }

        transform.rotation = Quaternion.identity;
    }

    public void CollectItemInGhostMode(string itemID)
    {
        if (isGhost) ghostCollectedItems.Add(itemID);
    }

    void ConsolidateInventory()
    {
        foreach (string itemID in ghostCollectedItems)
        {
            Debug.Log($"【背包更新】獲得幽靈狀態採集物品: {itemID}");
        }
        ghostCollectedItems.Clear();
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
    }

    public bool IsGhostState()
    {
        return isGhost;
    }
}