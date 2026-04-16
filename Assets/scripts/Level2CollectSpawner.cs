using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns Level 2 collectibles.
/// Each collectible gets its own dedicated X lane spaced far apart
/// so the player cannot accidentally touch multiple answers at once.
/// </summary>
public class Level2CollectibleSpawner : MonoBehaviour
{
    public static Level2CollectibleSpawner Instance { get; private set; }

    [Header("Spawn Position")]
    [Tooltip("How far ahead of the player the FIRST collectible spawns")]
    public float spawnAheadX = 10f;

    [Tooltip("X gap between each collectible lane — increase to spread them out more")]
    public float laneXSpacing = 8f;

    [Tooltip("Random extra X added per lane for variety")]
    public float randomXExtra = 2f;

    [Tooltip("Y range each collectible can spawn in")]
    public float minSpawnY = -1.5f;
    public float maxSpawnY = 2.5f;

    [Tooltip("Min Y distance between collectibles so they dont stack vertically")]
    public float minYSeparation = 1.8f;

    [Header("Scroll Speed")]
    [Range(0.1f, 1f)]
    public float speedScale = 0.6f;
    public float fastCatchupSpeed = 40f;

    private Transform playerTransform;
    private bool spawnRequested = false;
    private bool hasSpawned = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void TriggerSpawn()
    {
        if (hasSpawned)
            return;
        spawnRequested = true;
    }

    public void ResetSpawn()
    {
        hasSpawned = false;
        spawnRequested = false;
    }

    void Update()
    {
        if (!spawnRequested)
            return;
        if (Level2Manager.Instance == null || !Level2Manager.Instance.IsActive)
            return;
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        spawnRequested = false;
        hasSpawned = true;
        SpawnChoices();
    }

    void SpawnChoices()
    {
        var manager = Level2Manager.Instance;
        if (manager == null || manager.CurrentQuestion == null)
            return;

        var spawnList = new List<(GameObject prefab, AudioClip sound, bool isCorrect)>();
        spawnList.Add((manager.CurrentQuestion.prefab, manager.CurrentQuestion.sound, true));

        foreach (var w in manager.GetWrongLetters(2))
            spawnList.Add((w.prefab, w.sound, false));

        // Shuffle so correct is not always in the same lane
        for (int i = spawnList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (spawnList[i], spawnList[j]) = (spawnList[j], spawnList[i]);
        }

        float baseX = playerTransform.position.x + spawnAheadX;
        var usedY = new List<float>();

        for (int i = 0; i < spawnList.Count; i++)
        {
            var entry = spawnList[i];
            float laneX = baseX + i * (laneXSpacing + Random.Range(0f, randomXExtra));
            float spawnY = GetSeparatedY(usedY);
            usedY.Add(spawnY);

            var go = Instantiate(entry.prefab, new Vector3(laneX, spawnY, 0f), Quaternion.identity);
            var item = go.AddComponent<Level2CollectibleItem>();
            item.Setup(entry.prefab, entry.sound, entry.isCorrect, speedScale, fastCatchupSpeed);
        }
    }

    float GetSeparatedY(List<float> usedY)
    {
        float best = Random.Range(minSpawnY, maxSpawnY);
        float bestScore = 0f;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            float candidate = Random.Range(minSpawnY, maxSpawnY);
            float minDist = float.MaxValue;

            foreach (float y in usedY)
                minDist = Mathf.Min(minDist, Mathf.Abs(candidate - y));

            if (usedY.Count == 0 || minDist >= minYSeparation)
                return candidate;

            if (minDist > bestScore)
            {
                bestScore = minDist;
                best = candidate;
            }
        }

        return best;
    }
}
