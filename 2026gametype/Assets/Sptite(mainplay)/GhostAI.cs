using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HateZoneVisualizer))]
public class GhostAI : MonoBehaviour
{
    public enum GhostState { Patrol, ChasingPlayer, Attracted, Raging }

    [Header("狀態設定")]
    public GhostState currentState = GhostState.Patrol;

    [Header("移動速度")]
    public float patrolSpeed = 2f;      // 平時巡邏/徘徊速度
    public float chaseSpeed = 5.5f;     // 追逐玩家速度
    public float chargeSpeed = 10f;     // 被吸引暴衝時的速度 (極速撞牆)

    [Header("仇恨範圍與目標設定")]
    public float normalHateRadius = 3f; // 平時警戒圈半徑 (黃圈)
    public float rageHateRadius = 5f;   // 暴走撕咬時的半徑 (紅圈)
    public LayerMask playerLayer;       // 玩家 Layer (請在 Inspector 選擇 Player)
    
    // 💡【新增】牆壁/障礙物 Layer (用於判斷玩家是否躲在牆後)
    [Header("視線遮蔽設定")]
    public LayerMask obstacleLayer;     

    public float rageDuration = 5f;     // 被包裹吸引後在原地撕咬/發呆的時間 (秒)

    [Header("巡邏點設定 (Waypoints)")]
    public Transform[] patrolPoints;     // 拖入你的巡邏點 (Waypoint 1, 2, 3...)
    public float waypointWaitTime = 1.5f;// 到達巡邏點後的停頓休息時間
    private int currentPointIndex = 0;
    private float waitTimer = 0f;

    private Rigidbody2D rb;
    private HateZoneVisualizer hateVisualizer;
    private Transform playerTransform;   // 記錄玩家的位置
    
    private Vector2 targetPosition;
    private Transform lockedObstacle;   // 被磁吸鎖定的危牆
    private Coroutine rageCoroutine;    // 暴走計時協程引用

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        hateVisualizer = GetComponent<HateZoneVisualizer>();
    }

    void Start()
    {
        // 初始狀態為普通巡邏 (黃圈)
        if (hateVisualizer != null)
        {
            hateVisualizer.SetRageState(false, normalHateRadius);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GhostState.Patrol:
                HandlePatrol();
                CheckPlayerInHateZone(); // 巡邏時持續偵測玩家
                break;

            case GhostState.ChasingPlayer:
                HandleChasePlayer();     // 追逐玩家
                break;

            case GhostState.Attracted:
                MoveTowardsTarget();     // 高速暴衝向包裹/危牆
                break;

            case GhostState.Raging:
                // 正在撕咬包裹，在原地暴走 (由 Coroutine 倒數 5 秒後切回 Patrol)
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // --- 1. 檢測玩家是否踩入仇恨圈 (含牆壁遮蔽檢測) ---
    void CheckPlayerInHateZone()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(transform.position, normalHateRadius, playerLayer);
        if (playerCol != null)
        {
            // 💡【新增關鍵防線】從鬼的位置向玩家拉一條視線射線 (Linecast)
            // 檢查鬼與玩家之間是否被牆壁/障礙物遮擋
            RaycastHit2D hitObstacle = Physics2D.Linecast(transform.position, playerCol.transform.position, obstacleLayer);

            // 如果射線有撞到牆壁/障礙物，代表玩家躲在牆後，鬼看不見！直接 Return 繼續巡邏
            if (hitObstacle.collider != null)
            {
                return;
            }

            // 無牆壁遮擋，成功看見玩家！切換為追擊狀態
            playerTransform = playerCol.transform;
            currentState = GhostState.ChasingPlayer;
            
            // 切換為紅色暴走圈
            if (hateVisualizer != null)
            {
                hateVisualizer.SetRageState(true, rageHateRadius);
            }
        }
    }

    // --- 2. 追逐玩家邏輯 ---
    void HandleChasePlayer()
    {
        if (playerTransform == null)
        {
            ReturnToPatrol();
            return;
        }

        // 朝玩家移動
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        rb.velocity = direction * chaseSpeed;

        // 轉向朝向玩家
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, angle, 360f * Time.deltaTime));
        }

        // 如果玩家逃離仇恨圈 1.5 倍距離以上，脫離仇恨返回巡邏
        if (Vector2.Distance(transform.position, playerTransform.position) > rageHateRadius * 1.5f)
        {
            ReturnToPatrol();
        }
    }

    // --- 3. 巡邏邏輯 ---
    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 direction = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;

        // 移動至巡邏點
        rb.velocity = direction * patrolSpeed;

        // 轉向朝向移動方向
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, angle, 360f * Time.deltaTime));
        }

        // 檢測是否到達巡邏點
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            rb.velocity = Vector2.zero;
            waitTimer += Time.deltaTime;

            if (waitTimer >= waypointWaitTime)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                waitTimer = 0f;
            }
        }
    }

    // --- 4. 當附近有 KarmaPackage 落地時由 Package 呼叫此方法 ---
    public void AttractToPackage(Vector2 packagePos, Transform obstacle)
    {
        // 若先前有正在執行的暴走計時器，先停止
        if (rageCoroutine != null)
        {
            StopCoroutine(rageCoroutine);
        }

        targetPosition = packagePos;
        lockedObstacle = obstacle;
        
        // 強制打斷巡邏/追擊，切換為被吸引狀態
        currentState = GhostState.Attracted;
        
        if (hateVisualizer != null)
        {
            hateVisualizer.SetRageState(true, rageHateRadius);
        }
    }

    void MoveTowardsTarget()
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * chargeSpeed;

        // 衝刺時轉向朝向目標
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(angle);
        }

        // 到達包裹/危牆位置
        if (Vector2.Distance(transform.position, targetPosition) < 0.8f)
        {
            rb.velocity = Vector2.zero;
            currentState = GhostState.Raging;

            // 撞碎危牆
            if (lockedObstacle != null)
            {
                BreakObstacle(lockedObstacle);
            }

            // 開啟 5 秒撕咬/發呆計時，時間到自動返回巡邏
            rageCoroutine = StartCoroutine(RageTimerRoutine());
        }
    }

    // --- 5. 撕咬 5 秒後自動返回巡邏狀態 ---
    IEnumerator RageTimerRoutine()
    {
        yield return new WaitForSeconds(rageDuration);
        ReturnToPatrol();
    }

    // 重置並返回巡邏模式
    void ReturnToPatrol()
    {
        playerTransform = null;
        lockedObstacle = null;
        currentState = GhostState.Patrol;

        if (hateVisualizer != null)
        {
            hateVisualizer.SetRageState(false, normalHateRadius);
        }
    }

    void BreakObstacle(Transform obstacle)
    {
        if (!DestructibleHouse.TryBreakObstacle(obstacle))
            Destroy(obstacle.gameObject);
        lockedObstacle = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillPlayer(collision.gameObject);
    }

    // 情況 2：觸發器碰撞 (Collider 有勾選 Is Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillPlayer(other.gameObject);
    }

    // 統一執行的死亡判定
    void TryKillPlayer(GameObject obj)
    {
        if (obj.CompareTag("Player") || obj.GetComponent<PlayerMove>() != null)
        {
            GhostMode ghostMode = obj.GetComponent<GhostMode>();
            if (ghostMode != null)
            {
                Debug.Log("<color=red>【厲鬼】抓到玩家了！觸發死亡模式。</color>");
                ghostMode.Die();
            }
            else
            {
                Debug.LogError("【錯誤】玩家物件上找不到 GhostMode 腳本！");
            }
        }
    }
}
