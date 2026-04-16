using UnityEngine;

/// <summary>
/// Generic left-scroller for background decoration objects (clouds, props, etc.).
///
/// ⚠️ DO NOT attach this to ground tile prefabs.
/// GroundSpawner scrolls tiles itself each frame. Adding WorldScroller
/// to a tile causes double-movement and breaks the pool recycling.
///
/// Safe to use on: parallax layers, loose background props, one-off decorations.
/// </summary>
public class WorldScroller : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = GameManager.Instance.CurrentSpeed;
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }
}
