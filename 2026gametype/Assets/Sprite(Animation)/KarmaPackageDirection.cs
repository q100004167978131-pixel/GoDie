using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KarmaPackageDirection : MonoBehaviour
{
    [Header("8 方向包裹 Sprite 陣列")]
    [Tooltip("請依序放入：0:右, 1:右上, 2:上, 3:左上, 4:左, 5:左下, 6:下, 7:右下")]
    public Sprite[] directionSprites = new Sprite[8];

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 根據丟出的向量自動切換包裹 8 方向圖案
    /// </summary>
    public void SetDirection(Vector2 throwDirection)
    {
        if (throwDirection == Vector2.zero) return;

        // 計算投擲方向的角度 (-180 到 180)
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;

        // 將角度映射至 0 ~ 7 的 index (每 45 度一個方向)
        // +22.5f 是為了讓 0 度剛好落在大約 -22.5 ~ +22.5 的正右方區間
        float normalizedAngle = angle + 22.5f;
        if (normalizedAngle < 0) normalizedAngle += 360f;

        int index = Mathf.FloorToInt(normalizedAngle / 45f) % 8;

        if (sr != null && directionSprites != null && index < directionSprites.Length && directionSprites[index] != null)
        {
            sr.sprite = directionSprites[index];
        }
    }
}
