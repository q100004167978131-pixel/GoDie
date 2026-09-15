using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI 介面組件")]
    public GameObject dialoguePanel;       // 對話框 Panel (背景圖掛在這)
    public TextMeshProUGUI nameText;       // NPC 名字
    public TextMeshProUGUI dialogueText;   // 對話內容文本
    public GameObject continuePrompt;     // 「按任意鍵繼續」提示

    [Header("玩家移動腳本")]
    public PlayerMove playerMoveScript;    // 拖入玩家的 PlayerMove

    private Queue<string> sentences = new Queue<string>(); // 對話佇列
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentSentence = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // 當對話開啟時，按「任意鍵」繼續下一句
        if (Input.anyKeyDown)
        {
            // 如果打字機效果還沒跑完，按下任意鍵直接顯示完整句子
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentSentence;
                isTyping = false;
                if (continuePrompt != null) continuePrompt.SetActive(true);
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    // 開始對話 (由 NPC 腳本呼叫)
    public void StartDialogue(string npcName, string[] lines)
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        // 1. 鎖定玩家移動
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false;
            
            // 清空玩家移動剩餘速度 (防止殘留慣性)
            Rigidbody2D rb2d = playerMoveScript.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.velocity = Vector2.zero;
        }

        // 2. 設定名字與對話內容
        if (nameText != null) nameText.text = npcName;

        sentences.Clear();
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    // 顯示下一句對話
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentSentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    // 逐字打字效果 (增添對話質感)
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        if (continuePrompt != null) continuePrompt.SetActive(false);

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.03f); // 打字速度
        }

        isTyping = false;
        if (continuePrompt != null) continuePrompt.SetActive(true);
    }

    // 結束對話
    public void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        // 解鎖玩家移動
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true;
        }
    }
}
