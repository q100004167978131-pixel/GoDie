using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 導入 UI 點擊監聽介面

public class GreenImageClicker : MonoBehaviour, IPointerClickHandler
{
   [Header("關卡跳轉腳本")]
    public NextLevel nextLevelScript; // 在 Inspector 中把掛載 NextLevel 的物件拉進來

    // 當玩家用滑鼠點擊這個 UI Image 時，Unity 會自動觸發這個函式
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("<color=green>【2D 介面】玩家點擊了綠色物件，進入下一關！</color>");

        if (nextLevelScript != null)
        {
            // 直接呼叫你 NextLevel 腳本裡的 LoadNextLevel() 方法
            nextLevelScript.LoadNextLevel();
        }
        else
        {
            Debug.LogError("【錯誤】GreenImageClicker 尚未設定 NextLevel 腳本引用！");
        }
    }
}