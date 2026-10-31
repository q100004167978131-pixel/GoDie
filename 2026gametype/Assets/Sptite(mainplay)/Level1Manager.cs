using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Manager : MonoBehaviour
{
    public static Level1Manager Instance { get; private set; }

    [Header("正確答案設定 (預設從 0 開始: 0, 1, 2)")]
    public int correctTopLeftPackageID = 0;  // 正確的左上包裹 ID
    public int correctBottomLeftDoorID = 0;  // 正確的左下單向門 ID
    public int correctFinalGhostID = 0;      // 正確的幽靈 2 選 1 機關 ID

    [Header("玩家當前選擇 (自動紀錄)")]
    public int currentPackageID = -1;
    public int currentDoorID = -1;
    public int currentFinalID = -1;

    [Header("終點通關設定")]
    public GameObject finalSmallHouse; // 右側最小的那棟房子

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. 左上角包裹選擇
    public void SelectTopLeftPackage(int packageID)
    {
        currentPackageID = packageID;
        Debug.Log($"<color=cyan>【Level1Manager】玩家選擇了左上包裹 #{packageID}</color>");
        CheckAllSelections();
    }

    // 2. 左下角門選擇
    public void SelectBottomLeftDoor(int doorID)
    {
        currentDoorID = doorID;
        Debug.Log($"<color=cyan>【Level1Manager】玩家選擇了左下門 #{doorID}</color>");
        CheckAllSelections();
    }

    // 3. 幽靈 2 選 1 機關選擇
    public void SelectFinalGhostChoice(int finalID)
    {
        currentFinalID = finalID;
        Debug.Log($"<color=cyan>【Level1Manager】幽靈選擇了最終機關 #{finalID}</color>");
        CheckAllSelections();
    }

    // 💡 核心驗證函數：每次玩家按 F 做任何選擇時，自動檢查 3 個區域是否同時正確
    public void CheckAllSelections()
    {
        // 必須三個區域都已經做出選擇 (!= -1)
        if (currentPackageID == -1 || currentDoorID == -1 || currentFinalID == -1)
        {
            return;
        }

        bool isPackageCorrect = (currentPackageID == correctTopLeftPackageID);
        bool isDoorCorrect = (currentDoorID == correctBottomLeftDoorID);
        bool isFinalCorrect = (currentFinalID == correctFinalGhostID);

        Debug.Log($"【關卡比對】包裹: {isPackageCorrect} | 門: {isDoorCorrect} | 幽靈機關: {isFinalCorrect}");

        if (isPackageCorrect && isDoorCorrect && isFinalCorrect)
        {
            OnLevel1Completed();
        }
        else
        {
            // 如果三個都選了但有錯，跳出提示
            if (currentFinalID != -1 && UIManager.Instance != null)
            {
                UIManager.Instance.ShowTip("機關沒有反應...似乎先前的選擇有誤？");
            }
        }
    }

    void OnLevel1Completed()
    {
        Debug.Log("<color=green>【第一關通關】完全正確！右側小房子消失，通路開啟！</color>");

        if (finalSmallHouse != null)
        {
            finalSmallHouse.SetActive(false); // 房子消失！
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTip("一股神秘的力量散去，通路打開了...");
        }
    }
}
