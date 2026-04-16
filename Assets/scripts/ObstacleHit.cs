using UnityEngine;

/// <summary>
/// Attach to every obstacle prefab alongside ObstacleScroller.
/// Calls PlayerHealth.TakeHit() — the health script decides
/// whether to flash/damage or trigger a full game over.
/// </summary>
public class ObstacleHit : MonoBehaviour
{
    [Header("Effect")]
    [Tooltip("Drag the ObstacleEffect prefab here")]
    public GameObject hitEffectPrefab;

    [Header("Timing")]
    [Tooltip("Delay between obstacle disappearing and it being destroyed")]
    public float destroyDelay = 0.4f;

    private bool hasBeenHit = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (hasBeenHit)
            return;
        if (!col.gameObject.CompareTag("Player"))
            return;
        Hit(col.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenHit)
            return;
        if (!other.CompareTag("Player"))
            return;
        Hit(other.gameObject);
    }

    void Hit(GameObject playerObj)
    {
        hasBeenHit = true;
        GetComponent<sfxScript>().PlaySFX();
        // 1. Spawn debris burst
        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        // 2. Hide sprite + disable collider immediately
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // 3. Tell the health system — it handles damage, flash, and death
        var health = playerObj.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeHit();

        // 4. Clean up obstacle
        Destroy(gameObject, destroyDelay);
    }
}
