using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gives the player 3 hits before dying.
/// After each hit there's a short invincibility window so the player
/// can't lose multiple hearts from the same obstacle.
///
/// SETUP:
/// 1. Attach this script to the Player GameObject
/// 2. Create 3 UI Image GameObjects for hearts (assign in Inspector)
/// 3. Assign a damaged heart sprite (dim/empty) for the hit state
/// 4. Assign an optional hit flash effect
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Invincibility After Hit")]
    [Tooltip("Seconds the player can't be hit again after taking damage")]
    public float invincibilityDuration = 1.5f;

    [Tooltip("How fast the player flashes while invincible")]
    public float flashSpeed = 8f;

    [Header("Heart UI")]
    [Tooltip("Assign 3 UI Image components — one per heart")]
    public Image[] heartImages;

    [Tooltip("Sprite shown for a full heart")]
    public Sprite fullHeartSprite;

    [Tooltip("Sprite shown for an empty/lost heart")]
    public Sprite emptyHeartSprite;

    [Header("Hit Effect")]
    [Tooltip("Optional screen flash or hit effect prefab")]
    public GameObject hitEffectPrefab;

    // ── public state ───────────────────────────────────────────────
    public int CurrentHealth { get; private set; }
    public bool IsInvincible { get; private set; }

    // ── private ────────────────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;
    private float invincibilityTimer = 0f;

    // ── Unity lifecycle ────────────────────────────────────────────
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();

        CurrentHealth = maxHealth;
        UpdateHeartUI();
    }

    void Update()
    {
        if (!IsInvincible)
            return;

        // Count down invincibility
        invincibilityTimer -= Time.deltaTime;

        // Flash the player sprite while invincible
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            Color c = spriteRenderer.color;
            c.a = Mathf.Clamp(alpha, 0.2f, 1f);
            spriteRenderer.color = c;
        }

        // Invincibility over
        if (invincibilityTimer <= 0f)
        {
            IsInvincible = false;

            // Restore full opacity
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────

    /// <summary>Called by ObstacleHit when player touches an obstacle.</summary>
    public void TakeHit()
    {
        if (IsInvincible)
            return;
        if (GameManager.Instance.IsGameOver)
            return;

        CurrentHealth--;
        UpdateHeartUI();

        // Spawn hit effect if assigned
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (CurrentHealth <= 0)
        {
            // No more health — actually die
            playerController.Die();
        }
        else
        {
            // Still alive — start invincibility window
            StartInvincibility();
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsInvincible = false;
        invincibilityTimer = 0f;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        UpdateHeartUI();
    }

    // ── Private helpers ────────────────────────────────────────────

    void StartInvincibility()
    {
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    void UpdateHeartUI()
    {
        if (heartImages == null)
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
                continue;

            bool filled = i < CurrentHealth;

            // Use sprites if assigned, otherwise tint the image
            if (fullHeartSprite != null && emptyHeartSprite != null)
                heartImages[i].sprite = filled ? fullHeartSprite : emptyHeartSprite;
            else
                heartImages[i].color = filled ? Color.white : new Color(1f, 1f, 1f, 0.25f); // dim = lost heart
        }
    }
}
