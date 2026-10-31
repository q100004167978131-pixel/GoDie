using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceGroupManager : MonoBehaviour
{
    public enum GroupType { TopLeftPackage, BottomLeftDoor, FinalGhostChoice }

    [Header("群組類別")]
    public GroupType groupType;

    [Header("該群組的選擇物件列表")]
    public List<InteractiveObject> choiceObjects = new List<InteractiveObject>();

    private InteractiveObject currentlySelectedObject = null;

    void Start()
    {
        foreach (var obj in choiceObjects)
        {
            if (obj != null) obj.groupManager = this;
        }
    }

    // 💡 關鍵修復：接收物件實體與該物件真實的 ID
    public void SelectObjectWithID(InteractiveObject newSelection, int targetID)
    {
        // 1. 如果之前選過別的物件，讓舊物件重新顯現
        if (currentlySelectedObject != null && currentlySelectedObject != newSelection)
        {
            currentlySelectedObject.SetVisible(true);
        }

        // 2. 隱藏新選中的物件
        newSelection.SetVisible(false);
        currentlySelectedObject = newSelection;

        Debug.Log($"<color=yellow>【成功觸發】群組 [{groupType}] 點擊了 [{newSelection.name}]，成功傳送 ID: {targetID}</color>");

        // 3. 將真實傳入的 targetID 同步至 Level1Manager
        if (Level1Manager.Instance != null)
        {
            switch (groupType)
            {
                case GroupType.TopLeftPackage:
                    Level1Manager.Instance.SelectTopLeftPackage(targetID);
                    break;

                case GroupType.BottomLeftDoor:
                    Level1Manager.Instance.SelectBottomLeftDoor(targetID);
                    break;

                case GroupType.FinalGhostChoice:
                    Level1Manager.Instance.SelectFinalGhostChoice(targetID);
                    break;
            }
        }
    }
}