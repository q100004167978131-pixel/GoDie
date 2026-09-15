using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class TutorialTipTrigger : MonoBehaviour
{
    [Header("提示設定")]
    [TextArea(2, 4)]
    public string promptMessage = "使用 WASD 控制角色上下左右移動";
    public bool hideOnExit = true;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<GhostMode>() != null)
        {
            if (UIManager.Instance != null)
            {
                Debug.Log($"<color=cyan>【教學區域】踩入空地！顯示提示：{promptMessage}</color>");
                UIManager.Instance.ShowTip(promptMessage);
            }
            else
            {
                Debug.LogError("【錯誤】場景中找不到 UIManager 實例！請確認場景中是否有 GameObject 掛載 UIManager 腳本！");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hideOnExit && (other.CompareTag("Player") || other.GetComponent<GhostMode>() != null))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideTip();
            }
        }
    }
}
