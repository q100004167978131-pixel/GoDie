using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;
    private PlayerMove playerMove;
    private GhostMode ghostMode;

    // 記錄最後輸入的方向 (停下按鍵時保持該方向的站立姿勢)
    private Vector2 lastInputDir = Vector2.down;

    void Awake()
    {
        anim = GetComponent<Animator>();
        playerMove = GetComponent<PlayerMove>();
        ghostMode = GetComponent<GhostMode>();
    }

    void Update()
    {
        if (anim == null) return;

        // 1. 取得玩家當前的鍵盤輸入 (WASD / 方向鍵)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector2 inputDir = new Vector2(inputX, inputY).normalized;
        bool isMoving = inputDir.sqrMagnitude > 0f;

        // 2. 如果玩家有按按鍵，更新最後面向方向
        if (isMoving)
        {
            lastInputDir = inputDir;
        }

        // 3. 傳遞參數至 Animator (若有按鍵用當前方向，沒按鍵用最後面向的方向)
        anim.SetFloat("InputX", isMoving ? inputDir.x : lastInputDir.x);
        anim.SetFloat("InputY", isMoving ? inputDir.y : lastInputDir.y);
        anim.SetBool("IsMoving", isMoving);

        // 4. 幽靈模式動畫播放修正 (不受 Time.timeScale = 0 影響)
        if (ghostMode != null && ghostMode.IsGhostState())
        {
            // 當遊戲暫停時，手動更新 Animator 的時間
            anim.Update(Time.unscaledDeltaTime);
        }
    }
}
