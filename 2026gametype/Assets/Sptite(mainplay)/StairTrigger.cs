using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairTrigger : MonoBehaviour
{
    [Header("樓梯與關卡設定")]
    public GameObject stairObject;          // 隱藏的樓梯物件
    public string nextSceneName = "Level2";    // 下一關場景名稱
    public float interactDistance = 2.0f;      // 幽靈按 F 鍵的互動距離

    [Header("提示文字內容")]
    public string interactPrompt = "按 【F】 鍵啟動靈魂通路";

    private bool isStairActivated = false;
    private Transform playerTransform;
    private GhostMode ghostMode;
    private bool wasShowingTip = false;

    void Awake()
    {
        if (stairObject != null)
        {
            stairObject.SetActive(false); // 初始隱藏
        }
    }

    void Update()
    {
        // 樓梯已出現後不需要再檢測幽靈按 F 鍵
        if (isStairActivated) return;

        // 1. 動態尋找場景中的玩家/幽靈 Transform
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                ghostMode = playerObj.GetComponent<GhostMode>();
            }
            return;
        }

        // 2. 計算幽靈與機關的距離 (手動距離檢測，不受 Time.timeScale = 0 影響)
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactDistance;
        bool isGhost = (ghostMode != null && ghostMode.IsGhostState());

        // 3. 靠近且為幽靈型態時顯示 UIManager 提示
        if (inRange && isGhost)
        {
            if (!wasShowingTip && UIManager.Instance != null)
            {
                UIManager.Instance.ShowTip(interactPrompt);
                wasShowingTip = true;
            }

            // 按下 F 鍵啟動機關
            if (Input.GetKeyDown(KeyCode.F))
            {
                ActivateStair();
            }
        }
        else
        {
            if (wasShowingTip && UIManager.Instance != null)
            {
                UIManager.Instance.HideTip();
                wasShowingTip = false;
            }
        }
    }

    void ActivateStair()
    {
        isStairActivated = true;

        // 隱藏按 F 鍵提示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideTip();
        }

        if (stairObject != null)
        {
            stairObject.SetActive(true);

            // 確保顯示的樓梯渲染層級高於地圖 Tilemap
            SpriteRenderer sr = stairObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;

            Debug.Log("<color=yellow>【機關啟動】幽靈按下 F 鍵，樓梯成功顯現！</color>");
        }
    }

    // 肉身玩家觸碰樓梯過關
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isStairActivated) return;

        GhostMode ghost = other.GetComponent<GhostMode>();
        if (other.CompareTag("Player") || ghost != null)
        {
            if (ghost == null || !ghost.IsGhostState())
            {
                Debug.Log("<color=green>【過關】玩家本體踏上樓梯，進入下一關！</color>");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}