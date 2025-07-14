using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("속도 설정")]
    public float moveSpeed = 3f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float distractionSpeed = 4f;

    [Header("이동 제어")]
    public bool lockYPosition = true;
    public float fixedYPosition = -1.27f;

    [Header("디버그")]
    public bool drawDebug = false;

    [Header("벽 감지")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float stuckCheckTime = 1f;

    private Transform currentTarget;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private Transform Player => GameObject.FindGameObjectWithTag("Player")?.transform;

    public bool isMoving { get; set; }

    // 벽 막힘 판정용
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (currentTarget == null)
        {
            isMoving = false;
            return;
        }

        // 벽 충돌 판정 + 방향 전환
        if (IsHittingWall())
        {
            stuckTimer += Time.fixedDeltaTime;

            if (Vector3.Distance(transform.position, lastPosition) < 0.01f)
            {
                if (stuckTimer >= stuckCheckTime)
                {
                    Debug.Log("[EnemyMovement] 벽에 막힘 → 타겟 방향 반전");
                    ReverseTargetDirection();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
                lastPosition = transform.position;
            }

            isMoving = false;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector3 targetPos = GetTargetPosition();
        Vector3 direction = (targetPos - transform.position).normalized;

        float step = moveSpeed * Time.fixedDeltaTime;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, step);

        rb.MovePosition(newPosition);
        isMoving = true;

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            Flip();
    }

    private bool IsHittingWall()
    {
        Vector2 origin = transform.position;
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, wallCheckDistance, wallLayer);

        if (drawDebug)
            Debug.DrawRay(origin, direction * wallCheckDistance, Color.red);

        return hit.collider != null;
    }

    private void ReverseTargetDirection()
    {
        if (currentTarget == null) return;

        Vector3 offset = transform.position - currentTarget.position;
        Vector3 newTarget = transform.position + offset;

        SetTarget(newTarget);
    }

    public void SetTarget(Vector3 position)
    {
        GameObject dummy = new GameObject("TargetDummy");
        dummy.transform.position = position;

        currentTarget = dummy.transform;

        StartCoroutine(ClearTargetAfterDelay(dummy, 3f));
    }

    public void ClearTarget()
    {
        currentTarget = null;
        isMoving = false;
    }

    public bool HasReachedTarget(float threshold = 0.1f)
    {
        if (currentTarget == null) return true;
        return Vector3.Distance(transform.position, GetTargetPosition()) <= threshold;
    }

    public Vector3 GetTargetPosition()
    {
        if (currentTarget == null) return transform.position;

        Vector3 pos = currentTarget.position;
        // if (lockYPosition)  Y값 고정 변수들. 주석 해제하면 다시 사용 가능 
        // {
        //     pos.y = fixedYPosition;
        // }
        return pos;
    }

    public void MoveToTarget(float speed)
    {
        if (currentTarget == null) return;

        moveSpeed = speed;
        Vector3 targetPos = GetTargetPosition();
        Vector3 direction = (targetPos - transform.position).normalized;

        float step = speed * Time.deltaTime;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, step);

        transform.position = newPosition;
        isMoving = true;

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            Flip();
    }

    public void StopMoving()
    {
        ClearTarget();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug || currentTarget == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, GetTargetPosition());
        Gizmos.DrawSphere(GetTargetPosition(), 0.2f);
    }

    private System.Collections.IEnumerator ClearTargetAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentTarget == target.transform)
        {
            currentTarget = null;
            isMoving = false;
        }

        Destroy(target);
    }
    public void SetTargetAwayFromPlayer(float retreatDistance)
    {
        if (Player == null) return;

        Vector3 retreatDirection = (transform.position - Player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * retreatDistance;

        SetTarget(retreatTarget);
    }
}