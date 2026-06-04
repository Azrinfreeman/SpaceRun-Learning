using UnityEngine;

/// <summary>
/// Scrolls the collectible left.
/// - Outside camera view : moves at fastCatchupSpeed to stay close
/// - Inside  camera view : moves at CurrentSpeed * speedScale (Inspector value)
///
/// On collect:
///   1. Default SFX plays (via sfxScript)
///   2. After SFX finishes, this collectible's specific sound plays (e.g. letter "A")
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Score")]
    public float scoreValue = 10f;

    [Header("Scroll Speed")]
    [Tooltip("Fraction of game speed while visible. Lower = easier to collect.")]
    [Range(0.1f, 1f)]
    public float speedScale = 0.6f;

    [Tooltip("Speed used when the object is outside the camera view.")]
    public float fastCatchupSpeed = 40f;

    [Header("Collect Effect")]
    public GameObject collectEffectPrefab;

    [Header("Bob Animation")]
    public bool bobEnabled = true;
    public float bobHeight = 0.3f;
    public float bobSpeed = 3f;

    [Header("Collectible Sound")]
    [Tooltip(
        "The specific sound for this collectible (e.g. letter A, B, C sound). "
            + "Plays after the default SFX finishes."
    )]
    public AudioClip collectibleSound;

    [Tooltip(
        "Delay between the default SFX ending and the collectible sound playing. "
            + "Increase if the SFX is long."
    )]
    public float soundDelay = 0.5f;

    // ── private ────────────────────────────────────────────────────
    private Camera mainCam;
    private Renderer rend;
    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        startPosition = transform.position;
    }

    void Update()
    {
        if (isCollected)
            return;

        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = IsVisible()
            ? GameManager.Instance.CurrentSpeed * speedScale
            : fastCatchupSpeed;

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Bob only while visible so startPosition stays in sync
        if (bobEnabled && IsVisible())
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (transform.position.x > 60f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
            return;

        if (!other.CompareTag("Player"))
            return;

        isCollected = true;

        // ── KEY FIX ───────────────────────────────────────────────
        // Destroy() is deferred to end-of-frame, so the collider stays
        // active for the rest of this physics step. Disable ALL colliders
        // immediately so this object cannot trigger anything else.
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        // ─────────────────────────────────────────────────────────

        // Score and progress
        GameManager.Instance.AddScore(scoreValue);
        ProgressSlider.instance.addProgress(5);

        // 1. Play default collect SFX
        var sfx = GetComponent<sfxScript>();
        float sfxLength = 0f;
        if (sfx != null)
        {
            sfx.PlaySFX();
            sfxLength = sfx.GetClipLength();
        }

        // 2. Play this collectible's specific sound after SFX finishes
        if (collectibleSound != null)
        {
            CollectibleSoundPlayer.Play(collectibleSound, sfxLength + soundDelay);
        }

        // Spawn effect
        if (collectEffectPrefab != null)
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // ── Debug helper — remove once bug is confirmed fixed ──────────
    // If any balloon disappears WITHOUT being collected, the Console will
    // print exactly which line of code destroyed it.
    void OnDestroy()
    {
        if (!isCollected)
        {
            Debug.LogWarning(
                $"[CollectibleItem] '{name}' was destroyed WITHOUT being collected!"
            );
            Debug.LogWarning(System.Environment.StackTrace);
        }
    }

    bool IsVisible()
    {
        if (rend == null || mainCam == null)
            return true;
        return GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(mainCam),
            rend.bounds
        );
    }
}