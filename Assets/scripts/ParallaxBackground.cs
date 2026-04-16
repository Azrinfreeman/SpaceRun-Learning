using UnityEngine;

/// <summary>
/// Attach to background / midground layers.
/// Set parallaxFactor: 0 = stationary, 1 = moves at full game speed.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor = 0.3f;

    public float tileWidth = 30f;   // Width of your background sprite
    private float startX;
    private Transform cam;

    void Start()
    {
        startX = transform.position.x;
        cam    = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        // Scroll leftward at a fraction of game speed
        float speed = GameManager.Instance.CurrentSpeed * parallaxFactor;
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Wrap when the layer has moved one full tile width to the left
        if (transform.position.x <= startX - tileWidth)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }
    }
}
