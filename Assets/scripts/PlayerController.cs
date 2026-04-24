using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Animation")]
    public Animator animator;

    public Rigidbody2D rb;
    private bool isGrounded;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Player is on right side of screen facing left
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = true;
    }

    void Update()
    {
        if (isDead)
            return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && isGrounded)
            Jump();

        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
            animator.SetFloat("velocityY", rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (animator != null)
            animator.SetTrigger("jump");
    }

    public void Die()
    {
        if (isDead)
            return; // prevent double-call
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        if (animator != null)
            animator.SetTrigger("die");
        GameManager.Instance.GameOver();
    }

    // ── Obstacle collision now handled by ObstacleHit.cs ──────────
    // Keeping OnCollisionEnter2D removed to avoid double Game Over.

    public void ResetPlayer()
    {
        isDead = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
