using UnityEngine;

public class ObstacleRandomSpin : MonoBehaviour
{
    [Header("Spin Settings")]
    public float minSpinSpeed = 40f;
    public float maxSpinSpeed = 120f;

    private float spinSpeed;

    void Start()
    {
        // Random clockwise or counter-clockwise direction
        float direction = Random.value < 0.5f ? -1f : 1f;

        spinSpeed = Random.Range(minSpinSpeed, maxSpinSpeed) * direction;

        // Random starting rotation
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // Rotate only. This does not change position or movement.
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
    }
}
