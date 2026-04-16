using UnityEngine;

/// <summary>
/// Controls jetpack particles and plays a jump SFX the moment the player
/// leaves the ground. Requires PlayerController on the same GameObject.
/// </summary>
public class PlayerJetpack : MonoBehaviour
{
    [Header("Particle System")]
    [Tooltip("Drag the child GameObject that has the Particle System on it")]
    public ParticleSystem jetParticles;

    [Header("Jump SFX")]
    [Tooltip("Assign your jump audio clip here (WAV or MP3)")]
    public AudioClip jumpSFX;

    [Range(0f, 1f)]
    [Tooltip("Volume of the jump sound")]
    public float jumpVolume = 0.8f;

    [Tooltip("Pitch variation for a more natural feel — 0 = none")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.1f;

    [Header("Particle Settings")]
    public bool scaleWithVelocity = true;
    public float minEmission = 20f;
    public float maxEmission = 80f;

    // ── private ────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private PlayerController playerController;
    private AudioSource audioSource;
    private bool wasGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        // Add AudioSource on this GameObject automatically
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound (no 3D falloff)

        if (jetParticles != null)
            jetParticles.Stop();
    }

    void Update()
    {
        if (playerController == null)
            return;

        bool grounded = IsGrounded();

        // ── Detect the exact frame the player leaves the ground ───
        if (wasGrounded && !grounded)
        {
            PlayJumpSFX();
        }

        // ── Particles ─────────────────────────────────────────────
        if (jetParticles != null)
        {
            if (!grounded)
            {
                if (!jetParticles.isPlaying)
                    jetParticles.Play();

                if (scaleWithVelocity)
                {
                    float factor = Mathf.InverseLerp(0f, 15f, Mathf.Abs(rb.linearVelocity.y));
                    var emission = jetParticles.emission;
                    emission.rateOverTime = Mathf.Lerp(minEmission, maxEmission, factor);
                }
            }
            else
            {
                if (jetParticles.isPlaying)
                    jetParticles.Stop();
            }
        }

        wasGrounded = grounded;
    }

    // ── SFX ────────────────────────────────────────────────────────

    void PlayJumpSFX()
    {
        if (jumpSFX == null || audioSource == null)
            return;

        // Slight random pitch so repeated jumps don't sound identical
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.volume = jumpVolume;
        audioSource.PlayOneShot(jumpSFX);
    }

    // ── Helpers ────────────────────────────────────────────────────

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            playerController.groundCheck.position,
            playerController.groundCheckRadius,
            playerController.groundLayer
        );
    }
}
