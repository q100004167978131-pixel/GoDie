using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class GhostAnimationController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    // 記錄最後一次移動的方向 (防止停止移動時動畫方向瞬間歸零)
    private Vector2 lastMoveDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (rb == null || anim == null) return;

        // 取得目前的物理移動速度
        Vector2 velocity = rb.velocity;

        // 如果移動速度大於微小值，更新移動方向
        if (velocity.sqrMagnitude > 0.01f)
        {
            lastMoveDir = velocity.normalized;
        }

        // 傳遞方向參數給 Animator Blend Tree
        anim.SetFloat("MoveX", lastMoveDir.x);
        anim.SetFloat("MoveY", lastMoveDir.y);
    }
}
