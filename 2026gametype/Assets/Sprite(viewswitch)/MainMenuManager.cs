using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
   [Header("UI 面板引用")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject keybindingsPanel;

    [Header("關卡跳轉設定")]
    public string firstLevelName = "Level1"; // 第一關的場景名稱

    [Header("設定 UI 組件")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    void Start()
    {
        // 確保初始只顯示主選單
        ShowMainMenu();

        // 載入與初始化玩家先前的音量與全螢幕設定 (PlayerPrefs)
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // 顯示並解鎖滑鼠游標
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- 1. 開始遊戲 ---
    public void StartGame()
    {
        Debug.Log("【主選單】開始遊戲，載入場景：" + firstLevelName);
        SceneManager.LoadScene(firstLevelName);
    }

    // --- 2. 面板切換邏輯 ---
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (keybindingsPanel != null) keybindingsPanel.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (keybindingsPanel != null) keybindingsPanel.SetActive(false);
    }

    public void ShowKeybindingsMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (keybindingsPanel != null) keybindingsPanel.SetActive(true);
    }

    // --- 3. 設定選單功能 ---
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume); // 自動保存音量設定
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0); // 自動保存全螢幕設定
    }

    // --- 4. 結束遊戲 ---
    public void QuitGame()
    {
        Debug.Log("【主選單】結束遊戲！");
        Application.Quit();

        #if UNITY_EDITOR
        // 如果在 Unity 編輯器測試，停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}