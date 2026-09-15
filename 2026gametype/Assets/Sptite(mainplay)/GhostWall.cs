using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class GhostWall : MonoBehaviour
{
    private Collider2D wallCollider;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 檢查碰撞對象是否為幽靈型態
        GhostMode ghost = collision.gameObject.GetComponent<GhostMode>();
        if (ghost != null && ghost.IsGhostState())
        {
            // 💡 僅無視該幽靈與幽靈牆之間的碰撞，允許穿牆！
            Physics2D.IgnoreCollision(collision.collider, wallCollider, true);
        }
    }
}
