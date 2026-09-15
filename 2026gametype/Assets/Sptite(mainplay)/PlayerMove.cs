using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public enum MovementState { Foot, Scooter }

    [Header("模式設定")]
    public MovementState currentState = MovementState.Foot;

    [Header("徒步參數")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;

    [Header("機車參數")]
    public float acceleration = 8f;      // 油門加速度
    public float maxScooterSpeed = 12f;  // 極速 (需飆破特定時速破除鬼打牆)
    public float turnSpeed = 180f;       // 壓車轉向速度
    public float driftDrag = 2f;         // 甩尾阻力 (模擬輪胎抓地力)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isSprinting;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 2D 俯視角關閉重力
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // 1. 讀取玩家輸入 (WASD / 方向鍵)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // 2. 測試用的模式切換鍵 (按 E 鍵切換 徒步/機車)
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchMovementState(currentState == MovementState.Foot ? MovementState.Scooter : MovementState.Foot);
        }

        // 3. 轉向邏輯 (僅機車模式有效)
        HandleRotation();
    }

    void FixedUpdate()
    {
        // 根據目前狀態執行不同的物理移動
        switch (currentState)
        {
            case MovementState.Foot:
                HandleFootMovement();
                break;

            case MovementState.Scooter:
                HandleScooterMovement();
                break;
        }
    }

    // --- 徒步移動邏輯 ---
    void HandleFootMovement()
    {
        float targetSpeed = isSprinting ? runSpeed : walkSpeed;
        rb.velocity = moveInput * targetSpeed;
    }

    // --- 機車移動邏輯 (含物理慣性與壓車) ---
    void HandleScooterMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // 按下方向鍵催油門，向車頭前方施力
            float currentMaxSpeed = isSprinting ? maxScooterSpeed * 1.3f : maxScooterSpeed;
            Vector2 force = transform.up * moveInput.magnitude * acceleration;
            rb.AddForce(force);

            // 限制最高時速
            if (rb.velocity.magnitude > currentMaxSpeed)
            {
                rb.velocity = rb.velocity.normalized * currentMaxSpeed;
            }
        }
        else
        {
            // 放開油門時自然減速
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * driftDrag);
        }

        // 模擬機車甩尾與橫向阻力 (側滑限制)
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        rb.velocity = forwardVelocity + rightVelocity * 0.1f; // 保持少許側滑甩尾感
    }

    // --- 朝向旋轉處理 ---
    void HandleRotation()
    {
        // 💡 徒步時 (Foot)：移除跟隨滑鼠旋轉，保持 Quaternion.identity 或交給 4 方向 Blend Tree 處理動畫

        if (currentState == MovementState.Scooter)
        {
            // 機車時 (Scooter)：保留根據 WASD 輸入的方向進行平滑壓車轉向
            if (moveInput != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    // 切換狀態供外部呼叫
    public void SwitchMovementState(MovementState newState)
    {
        currentState = newState;
        rb.velocity = Vector2.zero; // 切換時清空速度避免暴衝

        // 切換回徒步模式時，重置玩家物件的 Rotation 角度為 0 (防止機車狀態殘留的角度影響徒步 Sprite)
        if (newState == MovementState.Foot)
        {
            transform.rotation = Quaternion.identity;
        }
    }
}