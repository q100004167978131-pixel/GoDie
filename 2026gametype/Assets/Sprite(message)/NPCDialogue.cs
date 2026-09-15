using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC 資料設定")]
    public string npcName = "神秘郵差";
    
    [TextArea(3, 5)]
    public string[] dialogueLines; // 在 Inspector 直接輸入多行對話內容

    [Header("互動提示 (選填)")]
    public GameObject interactPrompt; // 例如 NPC 頭頂上的「按 F 對話」提示圖標

    private bool isPlayerInRange = false;

    void Update()
    {
        // 玩家在觸發範圍內，且按下 F 鍵
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
                
                // 開啟對話時隱藏頭頂提示
                if (interactPrompt != null) interactPrompt.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMove>() != null)
        {
            isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMove>() != null)
        {
            isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
            
            // 玩家走出範圍時強制關閉對話
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.EndDialogue();
            }
        }
    }
}
