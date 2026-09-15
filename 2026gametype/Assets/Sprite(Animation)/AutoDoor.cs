using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AutoDoor : MonoBehaviour
{
    [Header("門的阻擋碰撞體")]
    public Collider2D doorSolidCollider; // 請將門的「實體阻擋 BoxCollider2D」拉入此欄位

    private Animator anim;
    private int playerInsideCount = 0; // 記錄在感應區內的玩家/幽靈數量

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

    // 玩家/幽靈踏入感應區 (Circle Collider 2D - Is Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayerOrGhost(other))
        {
            playerInsideCount++;
            UpdateDoorState();
        }
    }

    // 玩家/幽靈離開感應區
    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayerOrGhost(other))
        {
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
            UpdateDoorState();
        }
    }

    bool IsPlayerOrGhost(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<GhostMode>() != null || other.GetComponent<PlayerMove>() != null;
    }

    void UpdateDoorState()
    {
        bool shouldOpen = playerInsideCount > 0;

        // 1. 播放 Animator 動畫 (IsOpen 設為 true/false)
        if (anim != null)
        {
            anim.SetBool("IsOpen", shouldOpen);
        }

        // 2. 💡 關鍵修復：門開啟時關閉阻擋碰撞體；門關閉時恢復阻擋！
        if (doorSolidCollider != null)
        {
            doorSolidCollider.enabled = !shouldOpen;
        }

        Debug.Log(shouldOpen ? "<color=green>【自動門】玩家靠近，門已開啟，阻擋碰撞已關閉！</color>" 
                             : "<color=orange>【自動門】玩家離開，門已關閉，阻擋碰撞已恢復！</color>");
    }
}
