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

    [Header(" TargetDummy 최적화")]
    [SerializeField] private bool useOptimizedTarget = true;
    [SerializeField] private float targetCleanupTime = 1f; // 3초 → 1초로 단축

    //  최적화된 타겟 시스템
    private Vector3 targetPosition = Vector3.zero;
    private bool hasTarget = false;
    private Transform currentTarget; // 레거시 호환용

    //  TargetDummy 재사용 시스템
    private GameObject reusableTargetDummy;
    private Coroutine currentCleanupCoroutine;

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

        // 재사용 가능한 TargetDummy 미리 생성
        if (useOptimizedTarget)
        {
            CreateReusableTargetDummy();
        }
    }

    private void OnDestroy()
    {
        //  컴포넌트 파괴 시 TargetDummy 정리
        if (reusableTargetDummy != null)
        {
            Destroy(reusableTargetDummy);
        }
    }

    /// <summary>
    /// 재사용 가능한 TargetDummy 생성
    /// </summary>
    private void CreateReusableTargetDummy()
    {
        if (reusableTargetDummy == null)
        {
            reusableTargetDummy = new GameObject($"TargetDummy_{gameObject.name}");
            reusableTargetDummy.hideFlags = HideFlags.HideInHierarchy; // 하이어라키에서 숨김
            Debug.Log($"[EnemyMovement] 재사용 TargetDummy 생성: {reusableTargetDummy.name}");
        }
    }

    private void FixedUpdate()
    {
        //  최적화된 타겟 시스템 사용
        if (useOptimizedTarget)
        {
            if (!hasTarget)
            {
                isMoving = false;
                return;
            }
        }
        else
        {
            // 레거시 시스템
            if (currentTarget == null)
            {
                isMoving = false;
                return;
            }
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
        // 최적화된 시스템 사용
        if (useOptimizedTarget)
        {
            if (!hasTarget) return;

            Vector3 offset = transform.position - targetPosition;
            Vector3 newTarget = transform.position + offset;
            SetTarget(newTarget);
        }
        else
        {
            // 레거시 시스템
            if (currentTarget == null) return;

            Vector3 offset = transform.position - currentTarget.position;
            Vector3 newTarget = transform.position + offset;
            SetTarget(newTarget);
        }
    }

    /// <summary>
    ///  최적화된 타겟 설정 (TargetDummy 생성 최소화)
    /// </summary>
    public void SetTarget(Vector3 position)
    {
        if (useOptimizedTarget)
        {
            //  Vector3만 사용하는 최적화된 방식
            targetPosition = position;
            hasTarget = true;

            Debug.Log($"[EnemyMovement]  최적화 타겟 설정: {position} (GameObject 생성 없음)");
        }
        else
        {
            //  기존 방식 개선 (재사용 시스템)
            SetTargetLegacy(position);
        }
    }

    /// <summary>
    ///  레거시 호환을 위한 개선된 TargetDummy 시스템
    /// </summary>
    private void SetTargetLegacy(Vector3 position)
    {
        // 기존 정리 코루틴 중단
        if (currentCleanupCoroutine != null)
        {
            StopCoroutine(currentCleanupCoroutine);
        }

        // 재사용 가능한 TargetDummy 사용
        if (reusableTargetDummy == null)
        {
            CreateReusableTargetDummy();
        }

        reusableTargetDummy.transform.position = position;
        currentTarget = reusableTargetDummy.transform;

        //  더 짧은 시간 후 정리 (3초 → 1초)
        currentCleanupCoroutine = StartCoroutine(ClearTargetAfterDelay(targetCleanupTime));

        Debug.Log($"[EnemyMovement]  TargetDummy 재사용: {position}");
    }

    public void ClearTarget()
    {
        if (useOptimizedTarget)
        {
            hasTarget = false;
            targetPosition = Vector3.zero;
        }
        else
        {
            currentTarget = null;
        }

        isMoving = false;

        // 정리 코루틴 중단
        if (currentCleanupCoroutine != null)
        {
            StopCoroutine(currentCleanupCoroutine);
            currentCleanupCoroutine = null;
        }
    }

    public bool HasReachedTarget(float threshold = 0.1f)
    {
        if (useOptimizedTarget)
        {
            if (!hasTarget) return true;
            return Vector3.Distance(transform.position, targetPosition) <= threshold;
        }
        else
        {
            if (currentTarget == null) return true;
            return Vector3.Distance(transform.position, GetTargetPosition()) <= threshold;
        }
    }

    public Vector3 GetTargetPosition()
    {
        if (useOptimizedTarget)
        {
            if (!hasTarget) return transform.position;

            Vector3 pos = targetPosition;
            if (lockYPosition)
            {
                pos.y = fixedYPosition;
            }
            return pos;
        }
        else
        {
            // 레거시 시스템
            if (currentTarget == null) return transform.position;

            Vector3 pos = currentTarget.position;
            if (lockYPosition)
            {
                pos.y = fixedYPosition;
            }
            return pos;
        }
    }

    public void MoveToTarget(float speed)
    {
        //  최적화된 시스템 사용
        if (useOptimizedTarget)
        {
            if (!hasTarget) return;
        }
        else
        {
            if (currentTarget == null) return;
        }

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
        if (!drawDebug) return;

        //  최적화된 시스템 디버그
        if (useOptimizedTarget && hasTarget)
        {
            Gizmos.color = Color.green; // 최적화 시스템은 초록색
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.2f);

            // 최적화 상태 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
        }
        else if (!useOptimizedTarget && currentTarget != null)
        {
            Gizmos.color = Color.yellow; // 레거시 시스템은 노란색
            Gizmos.DrawLine(transform.position, GetTargetPosition());
            Gizmos.DrawSphere(GetTargetPosition(), 0.2f);
        }
    }

    /// <summary>
    ///  개선된 정리 코루틴 (더 짧은 시간)
    /// </summary>
    private System.Collections.IEnumerator ClearTargetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        //  TargetDummy는 재사용하므로 파괴하지 않음
        if (!useOptimizedTarget)
        {
            currentTarget = null;
            isMoving = false;
        }

        currentCleanupCoroutine = null;
        Debug.Log($"[EnemyMovement] 타겟 정리 완료 ({delay}초 후)");
    }

    public void SetTargetAwayFromPlayer(float retreatDistance)
    {
        if (Player == null) return;

        Vector3 retreatDirection = (transform.position - Player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * retreatDistance;

        SetTarget(retreatTarget);
    }

    /// <summary>
    ///  현재 시스템 상태 확인 (디버그용)
    /// </summary>
    public void PrintSystemStatus()
    {
        Debug.Log($"[EnemyMovement] 시스템 상태 - 최적화모드: {useOptimizedTarget}, " +
                  $"타겟보유: {(useOptimizedTarget ? hasTarget : currentTarget != null)}, " +
                  $"재사용Dummy: {reusableTargetDummy != null}");
    }

    /// <summary>
    ///  런타임에서 최적화 모드 전환
    /// </summary>
    public void ToggleOptimizedMode()
    {
        useOptimizedTarget = !useOptimizedTarget;
        ClearTarget();

        if (useOptimizedTarget && reusableTargetDummy == null)
        {
            CreateReusableTargetDummy();
        }

        Debug.Log($"[EnemyMovement] 최적화 모드 전환: {useOptimizedTarget}");
    }
}