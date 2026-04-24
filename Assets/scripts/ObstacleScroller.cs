using UnityEngine;

/// <summary>
/// Scrolls the obstacle left.
/// - Outside camera view : moves at fastCatchupSpeed to stay close
/// - Inside  camera view : moves at CurrentSpeed * speedScale (Inspector value)
/// </summary>
public class ObstacleScroller : MonoBehaviour
{
    [Tooltip("Fraction of game speed while visible. Lower = easier to avoid.")]
    [Range(0.1f, 1f)]
    public float speedScale = 0.8f;

    [Tooltip("Speed used when the object is outside the camera view.")]
    public float fastCatchupSpeed = 40f;

    private Camera mainCam;
    private Renderer rend;

    void Start()
    {
        mainCam = Camera.main;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        float speed = IsVisible()
            ? GameManager.Instance.CurrentSpeed * speedScale
            : fastCatchupSpeed;

        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (transform.position.x > 60f)
            Destroy(gameObject);
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
