using UnityEngine;

/// <summary>
/// Added at runtime by Level2CollectibleSpawner to existing collectible prefabs.
/// Handles Level 2 collect logic — reports correct/wrong to Level2Manager.
/// Does NOT require any changes to the original prefab.
/// </summary>
public class Level2CollectibleItem : MonoBehaviour
{
    // ── Set by Level2CollectibleSpawner.Setup() ────────────────────
    private GameObject sourcePrefab;
    private AudioClip letterSound;
    private bool isCorrect;
    private float speedScale;
    private float fastCatchupSpeed;

    // ── private ────────────────────────────────────────────────────
    private Camera mainCam;
    private Renderer rend;
    private Vector3 startPosition;

    [Header("Collect Effect")]
    public GameObject collectEffectPrefab;

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

    public void Setup(GameObject prefab, AudioClip sound, bool correct, float speed, float catchup)
    {
        sourcePrefab = prefab;
        letterSound = sound;
        isCorrect = correct;
        speedScale = speed;
        fastCatchupSpeed = catchup;
    }

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        startPosition = transform.position;

        // Disable the original CollectibleItem script if present
        // so it doesn't double-collect or call Level 1 logic
        var original = GetComponent<CollectibleItem>();
        if (original != null)
            original.enabled = false;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = IsVisible()
            ? GameManager.Instance.CurrentSpeed * speedScale
            : fastCatchupSpeed;

        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < -60f)
            Destroy(gameObject);
    }

    // Guard so one collectible can't fire twice (e.g. player clips through it)
    private bool hasBeenCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (hasBeenCollected)
            return;
        hasBeenCollected = true;
        // 1. Play default collect SFX
        var sfx = GetComponent<sfxScript>();
        float sfxLength = 0f;
        if (sfx != null)
        {
            sfx.PlaySFX();
            sfxLength = sfx.GetClipLength(); // see note below
        }

        // Spawn effect
        if (collectEffectPrefab != null)
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

        // Play this letter's sound immediately via persistent player
        if (letterSound != null)
            CollectibleSoundPlayer.Play(letterSound, 0f);

        // Report to Level2Manager
        Level2Manager.Instance?.OnCollected(sourcePrefab, isCorrect);

        if (isCorrect)
        {
            // Correct — destroy ALL Level 2 collectibles in scene
            foreach (var item in FindObjectsByType<Level2CollectibleItem>(FindObjectsSortMode.None))
                Destroy(item.gameObject);
        }
        else
        {
            // Wrong — only destroy this one, others stay until ReplaySoundOnly clears them
            Destroy(gameObject);
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
