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
    [SerializeField] private bool unlockYDuringChase = true; // 추격 중 Y축 해제

    [Header("방향 전환 설정")]
    public bool useFlipX = true;
    public bool useScale = false;

    private Vector3 lastPosition;
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;
    public bool isMoving = false;

    // Y축 제어를 위한 참조
    private EnemyAI enemyAI;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
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

        // Y축 고정 여부를 동적으로 결정
        bool shouldLockY = ShouldLockYPosition();

        if (shouldLockY)
        {
            Vector3 pos = transform.position;
            pos.y = fixedYPosition;
            transform.position = pos;
        }
    }

    // Y축을 고정해야 하는지 확인
    private bool ShouldLockYPosition()
    {
        if (!lockYPosition) return false; // 기본 설정이 해제되어 있으면 항상 자유

        if (!unlockYDuringChase) return true; // 추격 중 Y축 해제가 비활성화되어 있으면 항상 고정

        // 추격 중이면 Y축 해제, 그 외에는 고정
        if (enemyAI != null)
        {
            return enemyAI.CurrentState != EnemyAI.AIState.Chasing;
        }

        return true; // 기본값은 고정
    }

    public void SetTarget(Vector3 target)
    {
        // Y축 고정 여부를 동적으로 확인
        bool shouldLockY = ShouldLockYPosition();

        if (shouldLockY)
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

        // Y축 고정 여부를 동적으로 확인
        bool shouldLockY = ShouldLockYPosition();

        if (shouldLockY)
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

    // Enemy가 현재 목표 위치를 가져올 수 있도록 추가
    public Vector3 GetTargetPosition() => targetPosition;

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