using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("속도 설정")]
    public float patrolSpeed = 4f;
    public float chaseSpeed = 4.2f;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Move(Vector2 direction, bool isChasing)
    {
        moveDirection = direction.normalized;
        float speed = isChasing ? chaseSpeed : patrolSpeed;
        rb.velocity = moveDirection * speed;
    }

    public void Stop()
    {
        rb.velocity = Vector2.zero;
    }

    public void FlipSprite(Vector2 targetPos)
    {
        if ((targetPos.x > transform.position.x && transform.localScale.x < 0) ||
            (targetPos.x < transform.position.x && transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}