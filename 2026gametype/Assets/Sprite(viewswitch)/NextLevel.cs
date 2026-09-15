using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [Header("玩家與 UI 切換")]
    public PlayerMove playerMoveScript;       // 玩家的 PlayerMove 腳本
    public GameObject game2DPanel;            // 2D 平面場景 / UI 面板 (預設隱藏)
    public Camera mainCamera;                 // 主攝影機
    public Camera ui2DCamera;                 // 2D 平面專用攝影機 (選填，若用 Canvas/UI 則不需設定)

    [Header("關卡跳轉設定")]
    public string nextSceneName;              // 下一關的場景名稱 (需在 Build Settings 註冊)

    private bool is2DMode = false;

    private void OnTriggerEnter(Collider other)
    {
        // 判定踩到終點的是玩家 (可依據 Tag 或 PlayerMove 元件)
        if (other.CompareTag("Player") || other.GetComponent<PlayerMove>() != null)
        {
            Enter2DMode();
        }
    }

    // 若為 2D 觸發器 (Physics2D)，自動支援
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMove>() != null)
        {
            Enter2DMode();
        }
    }

    void Enter2DMode()
    {
        if (is2DMode) return;
        is2DMode = true;

        // 1. 禁用玩家移動腳本與物理速度
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false;
            
            // 若 PlayerMove 內部有 Rigidbody/Rigidbody2D，順便清空速度
            Rigidbody rb = playerMoveScript.GetComponent<Rigidbody>();
            if (rb != null) rb.velocity = Vector3.zero;

            Rigidbody2D rb2d = playerMoveScript.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.velocity = Vector2.zero;
        }

        // 2. 顯示 2D 平面場景 / UI
        if (game2DPanel != null)
        {
            game2DPanel.SetActive(true);
        }

        // 3. 切換攝影機 (如有需要)
        if (mainCamera != null && ui2DCamera != null)
        {
            mainCamera.enabled = false;
            ui2DCamera.enabled = true;
        }

        // 4. 開啟滑鼠游標並解除鎖定，進入點擊模式
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("【關卡終點】已進入 2D 平面點擊模式！");
    }

    // 提供給 2D 平面中的「綠色物件 / 按鈕」點擊呼叫
    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // 若未填寫場景名稱，預設載入下一序號場景
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}