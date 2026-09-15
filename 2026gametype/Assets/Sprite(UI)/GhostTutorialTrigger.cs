using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTutorialTrigger : MonoBehaviour
{
    [Header("特寫與過場設定")]
    public Transform ghostTransform;        // 厲鬼 Transform (預設填自己)
    public float closeUpCameraSize = 1.5f;  // 特寫時鏡頭縮放大小
    public float closeUpDuration = 2.5f;    // 鏡頭特寫持續時間 (秒)

    [Header("教學文字設定")]
    [TextArea] public string firstSightTip = "這是厲鬼！被抓到將陷入靈魂型態...";
    [TextArea] public string firstChaseTip = "【提示】厲鬼會被業障包裹吸引，並衝撞破壞危牆！";

    // 狀態開關 (確保單場遊戲只觸發一次)
    private bool hasTriggeredSight = false;
    private bool hasTriggeredChase = false;

    private Camera mainCam;
    private CameraFollow2D camFollow;       
    private GhostAI ghostAI;
    private float originalCamSize = 2.5f;
    private Transform originalCamTarget;   // 💡 紀錄原本攝影機跟隨的目標 (玩家)

    void Awake()
    {
        if (ghostTransform == null) ghostTransform = transform;
        ghostAI = GetComponent<GhostAI>();
    }

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            originalCamSize = mainCam.orthographicSize;
            camFollow = mainCam.GetComponent<CameraFollow2D>();
            
            // 💡 預先記住原本跟隨的玩家物體
            if (camFollow != null)
            {
                originalCamTarget = camFollow.target;
            }
        }
    }

    void Update()
    {
        // 1. 檢測厲鬼是否第一次進入玩家鏡頭 (畫面視野檢測)
        if (!hasTriggeredSight && mainCam != null)
        {
            if (IsVisibleToCamera())
            {
                hasTriggeredSight = true;
                StartCoroutine(FirstSightRoutine());
            }
        }

        // 2. 檢測厲鬼是否第一次切換為追擊狀態 (ChasingPlayer)
        if (!hasTriggeredChase && ghostAI != null)
        {
            if (ghostAI.currentState == GhostAI.GhostState.ChasingPlayer)
            {
                hasTriggeredChase = true;
                StartCoroutine(FirstChaseSlowMotionRoutine());
            }
        }
    }

    // --- 判定厲鬼是否在攝影機畫面內 ---
    bool IsVisibleToCamera()
    {
        Vector3 screenPoint = mainCam.WorldToViewportPoint(ghostTransform.position);
        return screenPoint.x >= 0f && screenPoint.x <= 1f && screenPoint.y >= 0f && screenPoint.y <= 1f && screenPoint.z > 0f;
    }

    // --- 第一次遇見厲鬼：鏡頭特寫移至厲鬼 + 暫停提示 ---
    IEnumerator FirstSightRoutine()
    {
        // 0. 備份攝影機目標 (防呆)
        if (camFollow != null && camFollow.target != ghostTransform && camFollow.target != null)
        {
            originalCamTarget = camFollow.target;
        }

        // 1. 暫停時間
        if (UIManager.Instance != null) UIManager.Instance.SetTimeScale(0f);

        // 2. 將攝影機目標切換至「厲鬼」，並縮放視角進行特寫
        if (camFollow != null)
        {
            camFollow.target = ghostTransform;
        }
        if (mainCam != null) mainCam.orthographicSize = closeUpCameraSize;

        // 3. 顯示提示 UI
        if (UIManager.Instance != null) UIManager.Instance.ShowTip(firstSightTip);

        // 4. 等待指定時間 (不受 Time.timeScale 影響的 Realtime)
        yield return new WaitForSecondsRealtime(closeUpDuration);

        // 5. 💡【關鍵修復】精準切回原本跟隨的玩家 Transform
        if (camFollow != null && originalCamTarget != null)
        {
            camFollow.target = originalCamTarget;
        }
        if (mainCam != null) mainCam.orthographicSize = originalCamSize;

        // 6. 💡【關鍵修復】強制恢復時間與喚醒玩家移動腳本
        Time.timeScale = 1f; // 強制寫死恢復為 1.0 正常時間
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideTip();
        }

        // 尋找玩家物件並強制重置物理狀態
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // 確保 PlayerMove 腳本是開啟的
            PlayerMove moveScript = playerObj.GetComponent<PlayerMove>();
            if (moveScript != null) moveScript.enabled = true;

            // 確保 Rigidbody2D 不是 Kinematic，且速度重置
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.velocity = Vector2.zero;
            }
        }
    }

    // --- 第一次追擊：慢動作 + 包裹誘餌提示 ---
    IEnumerator FirstChaseSlowMotionRoutine()
    {
        // 1. 減慢時間至 0.2 倍速 (極致慢動作/子彈時間)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTimeScale(0.2f);
            UIManager.Instance.ShowTip(firstChaseTip);
        }

        // 2. 慢動作維持 3 秒 (Realtime 實施計算)
        yield return new WaitForSecondsRealtime(10.0f);

        // 3. 恢復正常速度並關閉提示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideTip();
            UIManager.Instance.SetTimeScale(1f);
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}