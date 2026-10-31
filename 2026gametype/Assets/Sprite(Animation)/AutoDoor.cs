using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AutoDoor : MonoBehaviour
{
    [Header("門的阻擋碰撞體")]
    public Collider2D doorSolidCollider; // 請將門的「實體阻擋 BoxCollider2D」拉入此欄位

    [Header("提示文字內容")]
    public string interactPrompt = "按 【F】 鍵開啟機關門";

    private Animator anim;
    private bool isPlayerInRange = false;
    private bool isOpen = false;

    void Awake()
    {
        anim = GetComponent<Animator>();

        // 防呆：若沒在 Inspector 手動指定，自動尋找本體上未勾選 Is Trigger 的 Collider
        if (doorSolidCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                if (!col.isTrigger)
                {
                    doorSolidCollider = col;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // 只有在玩家處於感應區域內、且門尚未開啟時，按下 F 鍵觸發開門
        if (isPlayerInRange && !isOpen)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenDoor();
            }
        }
    }

    // 玩家/幽靈踏入感應區 (Collider2D - Is Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayerOrGhost(other))
        {
            isPlayerInRange = true;

            // 如果門尚未開啟，顯示 UI 提示
            if (!isOpen && UIManager.Instance != null)
            {
                UIManager.Instance.ShowTip(interactPrompt);
            }
        }
    }

    // 玩家/幽靈離開感應區
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayerOrGhost(other))
        {
            isPlayerInRange = false;

            // 離開感應區時隱藏提示 UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideTip();
            }

            // 離開感應區時，若門是開著的則自動關閉門
            if (isOpen)
            {
                CloseDoor();
            }
        }
    }

    bool IsPlayerOrGhost(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<GhostMode>() != null || other.GetComponent<PlayerMove>() != null;
    }

    // 開門邏輯
    void OpenDoor()
    {
        isOpen = true;

        if (anim != null)
        {
            anim.SetBool("IsOpen", true);
        }

        if (doorSolidCollider != null)
        {
            doorSolidCollider.enabled = false; // 關閉阻擋碰撞體
        }

        // 成功開門後隱藏 UI 提示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideTip();
        }

        Debug.Log("<color=green>【機關門】玩家按下 F 鍵，門已開啟，阻擋碰撞已關閉！</color>");
    }

    // 關門邏輯
    void CloseDoor()
    {
        isOpen = false;

        if (anim != null)
        {
            anim.SetBool("IsOpen", false);
        }

        if (doorSolidCollider != null)
        {
            doorSolidCollider.enabled = true; // 恢復阻擋碰撞體
        }

        Debug.Log("<color=orange>【機關門】玩家離開感應區，門已自動關閉，阻擋碰撞已恢復！</color>");
    }
}