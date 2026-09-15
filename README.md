# GoDie

Unity 遊戲專案，主要內容在 `2026gametype/`。

## 目錄與檔案備註

- `2026gametype/Assets/Scenes/`：遊戲場景；`MainMenu.unity` 為主選單，`Level1.unity`、`Level2.unity` 為關卡。
- `2026gametype/Assets/Sptite(mainplay)/`：主要遊玩邏輯，包括玩家移動、鏡頭跟隨、瞄準、鬼魂 AI／模式、樓梯與牆面互動。
- `2026gametype/Assets/Sprite(UI)/`：使用者介面與教學提示觸發器。
- `2026gametype/Assets/Sprite(message)/`：NPC 對話及對話流程管理。
- `2026gametype/Assets/Sprite(Animation)/`：角色與門的動畫控制程式。
- `2026gametype/Assets/Sprite(viewswitch)/`：主選單、關卡切換、出口、檢查點與互動按鈕邏輯。
- `2026gametype/Assets/Player/`：玩家精靈圖、動畫與 Animator Controller。
- `2026gametype/Assets/Enemy/`：敵人精靈圖、動畫與 Animator Controller。
- `2026gametype/Assets/Art/`：場景地圖、建築、走廊、門、NPC 與其他視覺素材。
- `2026gametype/Assets/Prefab/`：可重複使用的 Unity 預製物件。
- `2026gametype/Assets/TextMesh Pro/`：文字顯示所需的 TextMesh Pro 字型、資源與範例資產。
- `2026gametype/Packages/manifest.json`：Unity 套件清單；`packages-lock.json` 鎖定套件版本。
- `2026gametype/ProjectSettings/`：Unity 專案設定，例如輸入、畫質、物理、音效與建置設定。
- `*.meta`：Unity 對應資產的識別與匯入設定；必須與原資產一併版本控制。
- `.gitignore`：排除 Unity 可自動重建的快取、紀錄與 IDE 暫存檔。

## 不納入版本控制的檔案

`Library/`、`Logs/`、`UserSettings/`、建置輸出及 IDE 暫存檔都由本機或 Unity 產生；複製專案後以 Unity 開啟即可重新建立。
