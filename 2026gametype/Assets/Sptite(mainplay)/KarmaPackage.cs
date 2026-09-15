using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KarmaPackage : MonoBehaviour
{
    [Header("飛行與吸引設定")]
    public float flySpeed = 15f;           // 拋投飛行速度
    public float attractionRadius = 8f;   // 落地吸引厲鬼半徑
    public float pickupDistance = 1.2f;    // 玩家/幽靈靠近包裹的拾取距離 (手動距離檢測)
    public LayerMask ghostLayer;          // 厲鬼 Layer

    private Vector2 targetPos;
    private Transform lockedObstacle;
    private bool isFlying = false;
    private bool isLanded = false;        // 是否已落地

    private Transform playerTransform;    // 記錄玩家/幽靈 Transform

    // 由 Aim.cs 拋投時呼叫
    public void InitTarget(Vector2 target, Transform obstacle)
    {
        targetPos = target;
        lockedObstacle = obstacle;
        isFlying = true;
        isLanded = false;

        // 尋找場景中的玩家/幽靈物件
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            // 防呆：如果找不到 Tag 為 Player 的物件，嘗試用 GhostMode 腳本搜尋
            GhostMode gMode = FindObjectOfType<GhostMode>();
            if (gMode != null) playerTransform = gMode.transform;
        }
    }

    void Update()
    {
        // 1. 飛行階段 (使用 unscaledDeltaTime 確保暫停下也能飛完)
        if (isFlying)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, flySpeed * Time.unscaledDeltaTime);

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            {
                OnLanded();
            }
        }

        // 2. 落地後：手動檢測玩家/幽靈與包裹的距離 (不受 Time.timeScale = 0 影響)
        if (isLanded && playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            // 當幽靈或玩家靠近包裹小於 pickupDistance 時，自動觸發拾取！
            if (dist <= pickupDistance)
            {
                PickupPackage();
            }
        }
    }

    void OnLanded()
    {
        isFlying = false;
        isLanded = true;

        Debug.Log("<color=cyan>【業障包裹】包裹已落地，散發黑氣吸引附近厲鬼！</color>");

        // 搜尋周圍厲鬼並觸發吸引
        Collider2D[] ghosts = Physics2D.OverlapCircleAll(transform.position, attractionRadius, ghostLayer);
        foreach (Collider2D g in ghosts)
        {
            GhostAI ghost = g.GetComponent<GhostAI>();
            if (ghost != null)
            {
                ghost.AttractToPackage(transform.position, lockedObstacle);
            }
        }
    }

    // 拾取邏輯
    void PickupPackage()
    {
        if (playerTransform == null) return;

        GhostMode ghostMode = playerTransform.GetComponent<GhostMode>();
        Aim playerAim = playerTransform.GetComponent<Aim>();

        // 判斷是否為幽靈型態
        if (ghostMode != null && ghostMode.IsGhostState())
        {
            ghostMode.CollectItemInGhostMode("KarmaPackage");
            Debug.Log("<color=purple>【幽靈拾取】幽靈成功拾起包裹！</color>");
        }
        else
        {
            Debug.Log("<color=green>【普通拾取】玩家肉身拾回了包裹！</color>");
        }

        // 通知 Aim 腳本重新裝填包裹
        if (playerAim != null)
        {
            playerAim.OnPackagePickedUp();
        }

        // 銷毀場景中的包裹
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // 紫色圓圈：吸引厲鬼範圍
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);

        // 黃色圓圈：拾取檢測範圍
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}