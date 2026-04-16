using UnityEngine;

/// <summary>
/// Added at runtime by Level3CollectibleSpawner to word prefabs.
/// Mirrors Level2CollectibleItem but reports to Level3Manager.
/// </summary>
public class Level3CollectibleItem : MonoBehaviour
{
    private GameObject sourcePrefab;
    private AudioClip wordSound;
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
        wordSound = sound;
        isCorrect = correct;
        speedScale = speed;
        fastCatchupSpeed = catchup;
    }

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
        startPosition = transform.position;

        // Disable original CollectibleItem if present so Level 1 logic doesn't fire
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (hasBeenCollected)
            return;
        hasBeenCollected = true;

        // Play word sound
        if (wordSound != null)
            CollectibleSoundPlayer.Play(wordSound, 0f);

        // Report to Level3Manager
        Level3Manager.Instance?.OnCollected(sourcePrefab, isCorrect);

        if (isCorrect)
        {
            // Correct — destroy all Level 3 collectibles
            foreach (var item in FindObjectsByType<Level3CollectibleItem>(FindObjectsSortMode.None))
                Destroy(item.gameObject);
        }
        else
        {
            // Wrong — only destroy this one
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
