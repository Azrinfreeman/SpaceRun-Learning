using UnityEngine;

/// <summary>
/// Attach to background / midground layers.
/// Scrolls RIGHT (left to right) to match the visual direction of the game.
/// Player faces left, so the world moves right past them.
/// Set parallaxFactor: 0 = stationary, 1 = moves at full game speed.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Range(-10f, 1f)]
    public float parallaxFactor = 0.3f;

    public float tileWidth = 30f;
    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = GameManager.Instance.CurrentSpeed * parallaxFactor;
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Wrap rightward (positive parallaxFactor — layer scrolls right)
        if (transform.position.x >= startX + tileWidth)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }

        // Wrap leftward (negative parallaxFactor — layer scrolls left)
        if (transform.position.x <= startX - tileWidth)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }
    }
}
