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

    private Transform currentTarget;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    //  수정: 외부에서 접근 가능한 isMoving (getter & setter 모두 public)
    public bool isMoving { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (currentTarget == null)
        {
            isMoving = false;
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

    public void SetTarget(Vector3 position)
    {
        GameObject dummy = new GameObject("TargetDummy");
        dummy.transform.position = position;
        currentTarget = dummy.transform;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
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
        if (lockYPosition)
        {
            pos.y = fixedYPosition;
        }
        return pos;
    }

    public void MoveToTarget(float speed)
    {
        if (currentTarget == null) return;

        moveSpeed = speed;  // 이 줄은 없어도 되긴 합니다 (단, 디버깅 용도로는 좋습니다)
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
}
