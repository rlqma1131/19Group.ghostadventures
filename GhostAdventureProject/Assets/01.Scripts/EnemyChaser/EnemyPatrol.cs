using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("랜덤 순찰 설정")]
    public float patrolWaitTime = 1f;

    [Header("스테이지 범위")]
    public float stageMinX = -21f; // 스테이지 왼쪽 끝
    public float stageMaxX = 16.5f;  // 스테이지 오른쪽 끝

    [Header("랜덤 이동 설정")]
    public float minMoveDistance = 3f;  // 최소 이동 거리
    public float maxMoveDistance = 10f; // 최대 이동 거리

    private Vector3 startPos;
    private Vector3 currentTarget;
    private bool isPatrolWaiting = false;
    private float waitTimer = 0f;
    private bool hasBeenInChaseMode = false; // 추격 상태를 경험했는지 추적

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

        // 처음에는 기본 순찰 (시작 위치 근처)
        if (!hasBeenInChaseMode)
        {
            GenerateInitialPatrolTarget();
        }
        else
        {
            GenerateRandomTarget();
        }
    }

    public void SetNewPatrolCenter(Vector3 newCenter, float radius)
    {
        // 랜덤 패트롤에서는 중심점 개념이 없으므로 그냥 새로운 랜덤 타겟 생성
        Debug.Log($"새로운 위치에서 랜덤 순찰 시작: {newCenter}");
        GenerateRandomTarget();
    }

    public void UpdatePatrolling()
    {
        // 추격 상태를 경험했는지 체크
        if (!hasBeenInChaseMode && enemyAI.CurrentState == EnemyAI.AIState.Chasing)
        {
            hasBeenInChaseMode = true;
            Debug.Log($"[EnemyPatrol] {gameObject.name} 추격 상태 경험 - 이제 랜덤 순찰 활성화");
        }

        if (isPatrolWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isPatrolWaiting = false;

                // 추격 상태를 경험했다면 랜덤 타겟, 아니면 기본 순찰
                if (hasBeenInChaseMode)
                {
                    GenerateRandomTarget();
                }
                else
                {
                    GenerateInitialPatrolTarget();
                }

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
        // 추격 상태를 경험했다면 랜덤 타겟, 아니면 기본 순찰
        if (hasBeenInChaseMode)
        {
            GenerateRandomTarget();
        }
        else
        {
            GenerateInitialPatrolTarget();
        }
    }

    // 추격 상태 경험 전 기본 순찰 (시작 위치 근처)
    private void GenerateInitialPatrolTarget()
    {
        float patrolRange = 5f; // 시작 위치 근처 순찰 범위
        float randomX = startPos.x + Random.Range(-patrolRange, patrolRange);

        // 스테이지 범위 내로 제한
        randomX = Mathf.Clamp(randomX, stageMinX, stageMaxX);

        Vector3 newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

        // 벽 충돌 체크
        if (IsBlocked(newTarget))
        {
            // 충돌하면 반대 방향으로 시도
            float oppositeX = startPos.x + (startPos.x - randomX);
            oppositeX = Mathf.Clamp(oppositeX, stageMinX, stageMaxX);
            newTarget = new Vector3(oppositeX, movement.fixedYPosition, transform.position.z);
        }

        currentTarget = newTarget;
        movement.SetTarget(currentTarget);

        Debug.Log($"[EnemyPatrol] {gameObject.name} 기본 순찰 타겟: {currentTarget}");
    }

    private void GenerateRandomTarget()
    {
        Vector3 newTarget;
        int maxAttempts = 10; // 무한 루프 방지
        int attempts = 0;

        do
        {
            // 현재 위치에서 최소/최대 거리 범위 내에서 랜덤 X 좌표 생성
            float currentX = transform.position.x;
            float minX = Mathf.Max(stageMinX, currentX - maxMoveDistance);
            float maxX = Mathf.Min(stageMaxX, currentX + maxMoveDistance);

            float randomX = Random.Range(minX, maxX);

            // 최소 이동 거리 보장
            if (Mathf.Abs(randomX - currentX) < minMoveDistance)
            {
                // 최소 거리가 안 되면 방향을 정해서 최소 거리만큼 이동
                float direction = Random.Range(0f, 1f) > 0.5f ? 1f : -1f;
                randomX = currentX + (direction * minMoveDistance);

                // 스테이지 범위 벗어나지 않게 조정
                randomX = Mathf.Clamp(randomX, stageMinX, stageMaxX);
            }

            newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

            attempts++;
        }
        while (IsBlocked(newTarget) && attempts < maxAttempts);

        // 모든 시도가 실패하면 기본 위치로
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"[EnemyPatrol] {gameObject.name} 랜덤 타겟 생성 실패, 기본 위치 사용");
            newTarget = new Vector3(
                Mathf.Clamp(transform.position.x + Random.Range(-3f, 3f), stageMinX, stageMaxX),
                movement.fixedYPosition,
                transform.position.z
            );
        }

        currentTarget = newTarget;
        movement.SetTarget(currentTarget);

        Debug.Log($"[EnemyPatrol] {gameObject.name} 새로운 랜덤 타겟: {currentTarget}");
    }

    private bool IsBlocked(Vector3 pos)
    {
        Collider2D col = Physics2D.OverlapCircle(pos, 0.2f);
        return col != null && col.CompareTag("Ground");
    }

    public Vector3 GetStartPosition() => startPos;

    // 디버깅용 - 스테이지 범위와 현재 타겟 시각화
    private void OnDrawGizmosSelected()
    {
        // 스테이지 범위 표시
        Gizmos.color = Color.blue;
        Vector3 stageLeft = new Vector3(stageMinX, transform.position.y, transform.position.z);
        Vector3 stageRight = new Vector3(stageMaxX, transform.position.y, transform.position.z);
        Gizmos.DrawLine(stageLeft + Vector3.up * 2f, stageLeft + Vector3.down * 2f);
        Gizmos.DrawLine(stageRight + Vector3.up * 2f, stageRight + Vector3.down * 2f);

        // 현재 타겟 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentTarget, 0.5f);

        // 현재 위치에서 타겟으로의 라인
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, currentTarget);
    }
}