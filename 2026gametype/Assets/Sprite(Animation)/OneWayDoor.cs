using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A vertical door that can only be opened from one world-space side.
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class OneWayDoor : MonoBehaviour
{
    public enum OpeningSide { Left, Right }

    [Header("允許開門的方向（遊戲畫面的左右）")]
    public OpeningSide allowedSide = OpeningSide.Left;
    [Min(0.01f)] public float interactDistance = 0.8f;
    [Tooltip("可留空，會尋找 Tag 為 Player 的玩家。")]
    public Transform player;
    [Tooltip("左右判定中心；留空使用門本身的位置。")]
    public Transform sideOrigin;

    [Header("門的阻擋與動畫")]
    public Collider2D doorSolidCollider;
    [Tooltip("依序放入關閉到開啟的十格圖片。")]
    public Sprite[] openingFrames;
    [Min(1f)] public float framesPerSecond = 12f;

    [Header("提示文字")]
    public string openPrompt = "按 【F】 開門";
    public string blockedPrompt = "不能從這邊開門";
    [Tooltip("場景沒有 UIManager 時，自動建立此共用提示介面。")]
    public UIManager promptUIPrefab;

    public bool IsOpen { get; private set; }
    public bool IsOpening { get; private set; }

    private SpriteRenderer doorRenderer;
    private Coroutine openingRoutine;
    private static readonly List<OneWayDoor> activeDoors = new List<OneWayDoor>();
    private static OneWayDoor promptOwner;
    private static int lastInteractionFrame = -1;
    private string displayedPrompt;

    private Vector3 Origin => sideOrigin != null ? sideOrigin.position : transform.position;

    private void Awake()
    {
        doorRenderer = GetComponent<SpriteRenderer>();
        if (doorSolidCollider == null)
        {
            foreach (Collider2D candidate in GetComponents<Collider2D>())
            {
                if (!candidate.isTrigger) { doorSolidCollider = candidate; break; }
            }
        }
        if (openingFrames != null && openingFrames.Length > 0)
            doorRenderer.sprite = openingFrames[0];
    }

    private void OnEnable() { activeDoors.Add(this); }

    private void OnDisable()
    {
        activeDoors.Remove(this);
        ReleasePrompt();
        if (openingRoutine != null)
        {
            StopCoroutine(openingRoutine);
            openingRoutine = null;
            // Finish an interrupted opening so a disabled component cannot leave a stuck door.
            FinishOpening();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (!CanInteract(player) || FindNearestDoor(player) != this)
        {
            ReleasePrompt();
            return;
        }

        ShowPrompt(CanOpenFrom(player.position) ? openPrompt : blockedPrompt);
        if (Input.GetKeyDown(KeyCode.F) && lastInteractionFrame != Time.frameCount)
        {
            lastInteractionFrame = Time.frameCount;
            TryOpen(player);
        }
    }

    public bool CanOpenFrom(Vector3 position)
    {
        float side = position.x - Origin.x;
        // Standing exactly on the dividing line is neither the left nor the right side.
        return allowedSide == OpeningSide.Left ? side < -0.001f : side > 0.001f;
    }

    private bool CanInteract(Transform actor)
    {
        return isActiveAndEnabled && !IsOpen && !IsOpening && actor != null &&
               ((Vector2)(actor.position - Origin)).sqrMagnitude <= interactDistance * interactDistance;
    }

    private static OneWayDoor FindNearestDoor(Transform actor)
    {
        OneWayDoor nearest = null;
        float bestDistance = float.PositiveInfinity;
        foreach (OneWayDoor door in activeDoors)
        {
            if (door == null || !door.CanInteract(actor)) continue;
            float distance = ((Vector2)(actor.position - door.Origin)).sqrMagnitude;
            if (distance < bestDistance ||
                (Mathf.Approximately(distance, bestDistance) &&
                 nearest != null && door.GetInstanceID() < nearest.GetInstanceID()))
            {
                bestDistance = distance;
                nearest = door;
            }
        }
        return nearest;
    }

    // Also usable by other player interaction scripts. Re-checks both distance and side.
    public bool TryOpen(Transform actor)
    {
        if (!CanInteract(actor)) return false;
        if (!CanOpenFrom(actor.position))
        {
            ShowPrompt(blockedPrompt);
            return false;
        }
        if (doorSolidCollider == null || doorSolidCollider.isTrigger)
        {
            Debug.LogError("【單向門】請指定未勾選 Is Trigger 的阻擋 Collider2D。", this);
            return false;
        }
        IsOpening = true;
        ReleasePrompt();
        openingRoutine = StartCoroutine(PlayOpening());
        return true;
    }

    private IEnumerator PlayOpening()
    {
        int count = openingFrames == null ? 0 : openingFrames.Length;
        float elapsed = 0f;
        float duration = Mathf.Max(1, count) / Mathf.Max(1f, framesPerSecond);
        while (elapsed < duration)
        {
            if (count > 0)
            {
                int index = Mathf.Min(count - 1, Mathf.FloorToInt(elapsed * Mathf.Max(1f, framesPerSecond)));
                doorRenderer.sprite = openingFrames[index];
            }
            // Ghost mode freezes timeScale; interactions and this animation still work.
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        FinishOpening();
        openingRoutine = null;
    }

    private void FinishOpening()
    {
        if (openingFrames != null && openingFrames.Length > 0 && doorRenderer != null)
            doorRenderer.sprite = openingFrames[openingFrames.Length - 1];
        if (doorSolidCollider != null) doorSolidCollider.enabled = false;
        IsOpening = false;
        IsOpen = true;
    }

    private void ShowPrompt(string message)
    {
        if (UIManager.Instance == null && promptUIPrefab != null)
            Instantiate(promptUIPrefab);
        if (UIManager.Instance == null) return;
        if (promptOwner == this && displayedPrompt == message) return;
        promptOwner = this;
        displayedPrompt = message;
        UIManager.Instance.ShowTip(message);
    }

    private void ReleasePrompt()
    {
        if (promptOwner != this) return;
        if (UIManager.Instance != null) UIManager.Instance.HideTip();
        promptOwner = null;
        displayedPrompt = null;
    }

    private void OnValidate()
    {
        interactDistance = Mathf.Max(0.01f, interactDistance);
        framesPerSecond = Mathf.Max(1f, framesPerSecond);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Origin, interactDistance);
        Gizmos.color = Color.green;
        Vector3 direction = allowedSide == OpeningSide.Left ? Vector3.left : Vector3.right;
        Gizmos.DrawLine(Origin, Origin + direction * interactDistance);
    }
}
