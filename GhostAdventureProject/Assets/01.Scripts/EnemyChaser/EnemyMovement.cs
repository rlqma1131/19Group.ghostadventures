using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("속도 설정")]
    public float moveSpeed = 3f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float distractionSpeed = 4f;

    [Header("디버그")]
    public bool drawDebug = false;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private bool isFacingRight = true;

    public bool isMoving { get; private set; }

    private void Update()
    {
        if (!hasTarget)
        {
            isMoving = false;
            return;
        }

        Vector3 direction = (targetPosition - transform.position).normalized;
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        isMoving = true;

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            Flip();
    }

    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
    }

    public void StopMoving()
    {
        hasTarget = false;
        isMoving = false;
    }

    public bool HasReachedTarget(float threshold = 0.1f)
    {
        if (!hasTarget) return true;
        return Vector3.Distance(transform.position, targetPosition) <= threshold;
    }

    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }

    public void MoveToTarget(float speed)
    {
        if (!hasTarget) return;

        moveSpeed = speed;
        Vector3 direction = (targetPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        isMoving = true;

        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            Flip();
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
        if (!drawDebug || !hasTarget) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.DrawSphere(targetPosition, 0.2f);
    }
}