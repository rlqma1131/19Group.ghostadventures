using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("속도 설정")]
    public float patrolSpeed = 4f;
    public float chaseSpeed = 7f;
    public float moveSpeed = 2f;
    public float distractionSpeed = 1f;

    [Header("Y축 고정 설정")]
    public bool lockYPosition = true;
    public float fixedYPosition = 0f;

    [Header("방향 전환 설정")]
    public bool useFlipX = true;
    public bool useScale = false;

    private Vector3 lastPosition;
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;
    public bool isMoving = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        lastPosition = transform.position;
        if (lockYPosition)
        {
            fixedYPosition = transform.position.y;
        }
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (lockYPosition)
        {
            Vector3 pos = transform.position;
            pos.y = fixedYPosition;
            transform.position = pos;
        }
    }

    public void SetTarget(Vector3 target)
    {
        if (lockYPosition)
        {
            target.y = fixedYPosition;
        }

        targetPosition = target;
        isMoving = true;
    }

    public void MoveToTarget(float speed)
    {
        if (!isMoving) return;

        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (lockYPosition)
        {
            newPosition.y = fixedYPosition;
        }

        transform.position = newPosition;
    }

    public void StopMoving() => isMoving = false;

    public bool HasReachedTarget(float threshold = 0.3f)
    {
        return Vector3.Distance(transform.position, targetPosition) <= threshold;
    }

    private void UpdateFacingDirection()
    {
        if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            float moveDirection = transform.position.x - lastPosition.x;

            if (Mathf.Abs(moveDirection) > 0.01f)
            {
                bool shouldFaceRight = moveDirection > 0;
                SetFacingDirection(shouldFaceRight);
            }

            lastPosition = transform.position;
        }
    }

    private void SetFacingDirection(bool faceRight)
    {
        if (useFlipX && spriteRenderer != null)
        {
            spriteRenderer.flipX = !faceRight;
        }
        else if (useScale)
        {

            Vector3 scale = transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}