using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public string nextSceneName = "Level2";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判定是否為肉身玩家
        GhostMode ghost = other.GetComponent<GhostMode>();

        if (other.CompareTag("Player") || ghost != null)
        {
            // 只有當不是幽靈（肉身玩家本體）時才過關
            if (ghost == null || !ghost.IsGhostState())
            {
                Debug.Log("<color=green>【過關】玩家本體踏上樓梯，進入下一關！</color>");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
