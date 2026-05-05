using UnityEngine;

/// <summary>
/// Added at runtime by Level2CollectibleSpawner to existing collectible prefabs.
/// Both correct AND wrong collects now destroy all Level2 collectibles in scene.
/// </summary>
public class Level2CollectibleItem : MonoBehaviour
{
    private GameObject sourcePrefab;
    private AudioClip letterSound;
    private bool isCorrect;
    private float speedScale;
    private float fastCatchupSpeed;
    private bool hasBeenCollected = false;

    private Camera mainCam;
    private Renderer rend;
    private Vector3 startPosition;

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

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (transform.position.x > 60f)
        {
            // Notify manager this collectible was missed (scrolled off screen)
            Level2Manager.Instance?.OnCollectibleMissed(gameObject);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (hasBeenCollected)
            return;
        hasBeenCollected = true;

        // Play sound
        if (letterSound != null)
            CollectibleSoundPlayer.Play(letterSound, 0f);

        // Report to Level2Manager
        Level2Manager.Instance?.OnCollected(sourcePrefab, isCorrect);

        // Always destroy ALL Level2 collectibles — correct or wrong
        foreach (var item in FindObjectsByType<Level2CollectibleItem>(FindObjectsSortMode.None))
            Destroy(item.gameObject);
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
