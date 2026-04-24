using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Infinite ground spawner — tiles scroll RIGHT (world moves right to left visually).
/// Player is on the RIGHT side of screen facing left.
/// Tiles recycle from right side to left side (leftmost tile moves to new leftmost position).
/// </summary>
public class GroundSpawner : MonoBehaviour
{
    [Header("Ground Tiles")]
    public GameObject groundTilePrefab;
    public float tileWidth = 10f;

    [Tooltip("Total tiles in the pool.")]
    public int poolSize = 8;

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;
    public int minTileGap = 2;
    public int maxTileGap = 5;

    [Header("World Y Positions")]
    public float groundY = -3f;
    public float minObstacleY = -1.5f;
    public float maxObstacleY = 1f;

    public static event System.Action<float> OnTileRecycled;

    private List<GameObject> pool = new List<GameObject>();
    private Transform playerTransform;
    private int tilesPlaced = 0;
    private int nextObstacleTile = 4;
    private bool isBuilding = false;

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

        // Tiles scroll RIGHT
        foreach (var tile in pool)
            tile.transform.Translate(Vector3.right * speed * Time.deltaTime);

        RecycleTiles();
    }

    void BuildPool()
    {
        isBuilding = true;

        // Start tiles to the LEFT of player spreading leftward
        float startX = playerTransform.position.x + tileWidth;
        for (int i = 0; i < poolSize; i++)
        {
            float x = startX - i * tileWidth;
            pool.Add(
                Instantiate(groundTilePrefab, new Vector3(x, groundY, 0f), Quaternion.identity)
            );
        }

        nextObstacleTile = 4;
        isBuilding = false;
    }

    void RecycleTiles()
    {
        // Recycle tiles that scroll past the RIGHT edge of the player
        float recycleThreshold = playerTransform.position.x + tileWidth * 2f;

        for (int i = 0; i < pool.Count; i++)
        {
            GameObject tile = pool[i];
            if (tile == null)
                continue;

            if (tile.transform.position.x - tileWidth > recycleThreshold)
            {
                // Move to the new leftmost position
                float newX = GetLeftmostX() - tileWidth;
                tile.transform.position = new Vector3(newX, groundY, 0f);

                if (tilesPlaced >= nextObstacleTile)
                {
                    SpawnObstacle(newX);
                    nextObstacleTile = tilesPlaced + Random.Range(minTileGap, maxTileGap + 1);
                }

                if (!isBuilding)
                    OnTileRecycled?.Invoke(newX);

                tilesPlaced++;
            }
        }
    }

    float GetLeftmostX()
    {
        float minX = float.MaxValue;
        foreach (var tile in pool)
            if (tile != null && tile.transform.position.x < minX)
                minX = tile.transform.position.x;
        return minX;
    }

    void SpawnObstacle(float tileX)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int idx = Random.Range(0, obstaclePrefabs.Length);
        float spawnX = tileX + tileWidth * 0.5f;
        float spawnY = Random.Range(minObstacleY, maxObstacleY);
        Instantiate(obstaclePrefabs[idx], new Vector3(spawnX, spawnY, 0f), Quaternion.identity);
    }

    public void ResetSpawner()
    {
        isBuilding = true;

        foreach (var obs in GameObject.FindGameObjectsWithTag("Obstacle"))
            Destroy(obs);

        tilesPlaced = 0;
        nextObstacleTile = 4;

        float startX = playerTransform.position.x + tileWidth;
        for (int i = 0; i < pool.Count; i++)
            if (pool[i] != null)
                pool[i].transform.position = new Vector3(startX - i * tileWidth, groundY, 0f);

        isBuilding = false;
    }
}
