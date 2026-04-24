using UnityEngine;

/// <summary>
/// Generic left-scroller for background decoration objects (clouds, props, etc.).
///
/// ⚠️ DO NOT attach this to ground tile prefabs.
/// GroundSpawner scrolls tiles itself each frame. Adding WorldScroller
/// to a tile causes double-movement and breaks the pool recycling.
///
/// Safe to use on: parallax layers, loose background props, one-off decorations.
/// Set tileWidth to the width of the sprite/image so it wraps seamlessly.
/// Leave tileWidth at 0 to disable wrapping (one-shot objects like obstacles).
/// </summary>
public class WorldScroller : MonoBehaviour
{
    [Tooltip("Width of this object's tile. Set to sprite width for seamless looping. 0 = no wrap.")]
    public float tileWidth = 0f;

    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = GameManager.Instance.CurrentSpeed;
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Wrap leftward when tileWidth is set
        if (tileWidth > 0f && transform.position.x <= startX - tileWidth)
        {
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }
    }
}

