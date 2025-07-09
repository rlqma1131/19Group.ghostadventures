using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("기본 순찰 설정")]
    public float patrolWaitTime = 1f;
    public float patrolRadius = 10f; // 시작 위치 기준 순찰 반경

    private Vector3 startPos;
    private Vector3 currentTarget;
    private bool isPatrolWaiting = false;
    private float waitTimer = 0f;

    private EnemyMovement movement;
    private EnemyAI enemyAI;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        startPos = transform.position;
        GeneratePatrolTarget();
    }

    public void SetNewPatrolCenter(Vector3 newCenter, float radius)
    {
        // 새로운 중심점으로 순찰 시작
        startPos = newCenter;
        Debug.Log($"새로운 순찰 중심점: {newCenter}");
        GeneratePatrolTarget();
    }

    public void UpdatePatrolling()
    {
        if (isPatrolWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isPatrolWaiting = false;
                GeneratePatrolTarget(); // 새로운 순찰 타겟 생성
                waitTimer = 0f;
            }
            return;
        }

        if (movement.HasReachedTarget())
        {
            movement.StopMoving();
            isPatrolWaiting = true;
            waitTimer = 0f;
        }
        else
        {
            movement.MoveToTarget(movement.patrolSpeed);
        }
    }

    public void SetNextPatrolTarget()
    {
        GeneratePatrolTarget();
    }

    private void GeneratePatrolTarget()
    {
        // 시작 위치 기준으로 좌우 patrolRadius 범위 내에서 랜덤 X 좌표 생성
        float randomX = startPos.x + Random.Range(-patrolRadius, patrolRadius);

        Vector3 newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

        // 벽 충돌 체크
        if (IsBlocked(newTarget))
        {
            // 충돌하면 반대 방향으로 시도
            float oppositeX = startPos.x + (startPos.x - randomX);
            newTarget = new Vector3(oppositeX, movement.fixedYPosition, transform.position.z);

            // 그래도 충돌하면 시작 위치로
            if (IsBlocked(newTarget))
            {
                newTarget = new Vector3(startPos.x, movement.fixedYPosition, transform.position.z);
            }
        }

        currentTarget = newTarget;
        movement.SetTarget(currentTarget);

        Debug.Log($"[EnemyPatrol] {gameObject.name} 순찰 타겟: {currentTarget}");
    }

    private bool IsBlocked(Vector3 pos)
    {
        Collider2D col = Physics2D.OverlapCircle(pos, 0.2f);
        return col != null && col.CompareTag("Ground");
    }

    public Vector3 GetStartPosition() => startPos;

    // 디버깅용 - 순찰 범위와 현재 타겟 시각화
    private void OnDrawGizmosSelected()
    {
        // 순찰 범위 표시 (시작 위치 기준)
        Gizmos.color = Color.blue;
        Vector3 leftBound = new Vector3(startPos.x - patrolRadius, transform.position.y, transform.position.z);
        Vector3 rightBound = new Vector3(startPos.x + patrolRadius, transform.position.y, transform.position.z);
        Gizmos.DrawLine(leftBound + Vector3.up * 2f, leftBound + Vector3.down * 2f);
        Gizmos.DrawLine(rightBound + Vector3.up * 2f, rightBound + Vector3.down * 2f);

        // 순찰 범위 전체 표시
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawCube(new Vector3(startPos.x, transform.position.y, transform.position.z),
                       new Vector3(patrolRadius * 2f, 1f, 1f));

        // 현재 타겟 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentTarget, 0.5f);

        // 시작 위치 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.3f);

        // 현재 위치에서 타겟으로의 라인
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, currentTarget);
    }
}