using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Infinite ground spawner with object pooling.
/// OnTileRecycled fires ONLY when a tile is recycled to the front
/// (never during initial pool build) so collectibles never spawn
/// behind or at the player's starting position.
/// </summary>
public class GroundSpawner : MonoBehaviour
{
    [Header("Ground Tiles")]
    public GameObject groundTilePrefab;
    public float tileWidth = 10f;

    [Tooltip("Total tiles in the pool. Must cover screen width + buffer.")]
    public int poolSize = 8;

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;
    public int minTileGap = 2;
    public int maxTileGap = 5;

    [Header("World Y Positions")]
    public float groundY = -3f;
    public float minObstacleY = -1.5f;
    public float maxObstacleY = 1f;

    /// <summary>
    /// Fired ONLY when a tile is recycled ahead of the player during gameplay.
    /// Never fires during BuildPool or ResetSpawner.
    /// </summary>
    public static event System.Action<float> OnTileRecycled;

    // ── private ────────────────────────────────────────────────────
    private List<GameObject> pool = new List<GameObject>();
    private Transform playerTransform;
    private int tilesPlaced = 0;
    private int nextObstacleTile = 4;
    private bool isBuilding = false; // suppresses event during setup

    // ── Unity lifecycle ────────────────────────────────────────────
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        BuildPool();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = GameManager.Instance.CurrentSpeed;
        foreach (var tile in pool)
            tile.transform.Translate(Vector3.left * speed * Time.deltaTime);

        RecycleTiles();
    }

    // ── Setup ──────────────────────────────────────────────────────

    void BuildPool()
    {
        isBuilding = true; // block OnTileRecycled during initial fill

        float startX = playerTransform.position.x - tileWidth;
        for (int i = 0; i < poolSize; i++)
        {
            float x = startX + i * tileWidth;
            pool.Add(
                Instantiate(groundTilePrefab, new Vector3(x, groundY, 0f), Quaternion.identity)
            );
        }

        // tilesPlaced stays 0 — recycled tile count starts from gameplay
        nextObstacleTile = 4;
        isBuilding = false;
    }

    // ── Recycling ──────────────────────────────────────────────────

    void RecycleTiles()
    {
        float recycleThreshold = playerTransform.position.x - tileWidth * 2f;

        for (int i = 0; i < pool.Count; i++)
        {
            GameObject tile = pool[i];
            if (tile == null)
                continue;

            if (tile.transform.position.x + tileWidth < recycleThreshold)
            {
                float newX = GetRightmostX() + tileWidth;
                tile.transform.position = new Vector3(newX, groundY, 0f);

                // Obstacle check
                if (tilesPlaced >= nextObstacleTile)
                {
                    SpawnObstacle(newX);
                    nextObstacleTile = tilesPlaced + Random.Range(minTileGap, maxTileGap + 1);
                }

                // Fire event — CollectibleSpawner only hears tiles
                // that are being placed ahead during live gameplay
                if (!isBuilding)
                    OnTileRecycled?.Invoke(newX);

                tilesPlaced++;
            }
        }
    }

    float GetRightmostX()
    {
        float maxX = float.MinValue;
        foreach (var tile in pool)
            if (tile != null && tile.transform.position.x > maxX)
                maxX = tile.transform.position.x;
        return maxX;
    }

    // ── Obstacles ──────────────────────────────────────────────────

    void SpawnObstacle(float tileX)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int idx = Random.Range(0, obstaclePrefabs.Length);
        float spawnX = tileX + tileWidth * 0.5f;
        float spawnY = Random.Range(minObstacleY, maxObstacleY);
        Instantiate(obstaclePrefabs[idx], new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
    }

    // ── Reset ──────────────────────────────────────────────────────

    public void ResetSpawner()
    {
        isBuilding = true; // suppress events during reset too

        foreach (var obs in GameObject.FindGameObjectsWithTag("Obstacle"))
            Destroy(obs);

        tilesPlaced = 0;
        nextObstacleTile = 4;

        float startX = playerTransform.position.x - tileWidth;
        for (int i = 0; i < pool.Count; i++)
            if (pool[i] != null)
                pool[i].transform.position = new Vector3(startX + i * tileWidth, groundY, 0f);

        isBuilding = false;
    }
}
