using UnityEngine;

/// <summary>
/// Spawns collectibles by listening to GroundSpawner.OnTileRecycled.
/// Each item in a cluster gets its own random Y so they are scattered,
/// not in a flat horizontal line.
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [Header("Collectible Prefabs")]
    public GameObject[] collectiblePrefabs;

    [Header("Spawn Gap (in tiles)")]
    public int minTileGap = 2;
    public int maxTileGap = 5;

    [Header("Spawn Position")]
    public float tileWidth = 10f;
    public float minSpawnY = -1f;
    public float maxSpawnY = 3f;
    public float xOffset = 3f;

    [Header("Cluster")]
    public int minClusterSize = 1;
    public int maxClusterSize = 3;

    [Tooltip("Minimum X gap between each item in a cluster")]
    public float minXSpacing = 2f;

    [Tooltip("Extra random X added on top of minXSpacing for variety")]
    public float randomXExtra = 1.5f;

    [Tooltip("Minimum Y distance between items in same cluster")]
    public float minYSeparation = 1.2f;

    // ── private ────────────────────────────────────────────────────
    private int tilesReceived = 0;
    private int nextSpawnTile = 3;

    // ── Unity lifecycle ────────────────────────────────────────────
    void OnEnable() => GroundSpawner.OnTileRecycled += OnTileRecycled;

    void OnDisable() => GroundSpawner.OnTileRecycled -= OnTileRecycled;

    // ── Tile event ─────────────────────────────────────────────────

    void OnTileRecycled(float tileX)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        tilesReceived++;

        if (tilesReceived >= nextSpawnTile)
        {
            SpawnCluster(tileX);
            nextSpawnTile = tilesReceived + Random.Range(minTileGap, maxTileGap + 1);
        }
    }

    // ── Spawning ───────────────────────────────────────────────────

    void SpawnCluster(float tileX)
    {
        if (collectiblePrefabs == null || collectiblePrefabs.Length == 0)
            return;

        int count = Random.Range(minClusterSize, maxClusterSize + 1);
        float currentX = tileX + tileWidth * 0.5f + xOffset;

        // Track used Y values so items don't overlap vertically
        float[] usedY = new float[count];

        for (int i = 0; i < count; i++)
        {
            // Each item gets its own random X with spacing
            if (i > 0)
                currentX += minXSpacing + Random.Range(0f, randomXExtra);

            // Pick a Y that is far enough from previous items in this cluster
            float spawnY = GetSeparatedY(usedY, i);
            usedY[i] = spawnY;

            int idx = Random.Range(0, collectiblePrefabs.Length);
            Instantiate(
                collectiblePrefabs[idx],
                new Vector3(currentX, spawnY, 0f),
                Quaternion.identity
            );
        }
    }

    float GetSeparatedY(float[] usedY, int count)
    {
        float best = Random.Range(minSpawnY, maxSpawnY);
        float bestScore = 0f;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            float candidate = Random.Range(minSpawnY, maxSpawnY);
            float minDist = float.MaxValue;

            for (int i = 0; i < count; i++)
                minDist = Mathf.Min(minDist, Mathf.Abs(candidate - usedY[i]));

            if (count == 0 || minDist >= minYSeparation)
                return candidate;

            if (minDist > bestScore)
            {
                bestScore = minDist;
                best = candidate;
            }
        }

        return best;
    }

    // ── Reset ──────────────────────────────────────────────────────

    public void ResetSpawner()
    {
        foreach (var c in GameObject.FindGameObjectsWithTag("Collectible"))
            Destroy(c);

        tilesReceived = 0;
        nextSpawnTile = 3;
    }
}
