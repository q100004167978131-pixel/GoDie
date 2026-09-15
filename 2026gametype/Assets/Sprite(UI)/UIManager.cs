using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI 元件綁定")]
    public GameObject tipPanel;           
    public TextMeshProUGUI tipText;       

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTip();
    }

    public void ShowTip(string message)
    {
        if (tipPanel != null && tipText != null)
        {
            tipText.text = message;
            tipPanel.SetActive(true);
        }
    }

    public void HideTip()
    {
        if (tipPanel != null) tipPanel.SetActive(false);
    }

    // 💡【新增】設定遊戲時間流速 (慢動作 / 恢復正常)
    public void SetTimeScale(float scale)
    {
        // 如果玩家正處於幽靈模式 (Time.timeScale == 0)，不干涉幽靈暫停
        if (Time.timeScale == 0f && scale != 0f) return;
        Time.timeScale = scale;
    }
}
