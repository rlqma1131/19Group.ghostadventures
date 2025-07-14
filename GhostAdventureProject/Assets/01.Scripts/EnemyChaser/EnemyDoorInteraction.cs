using UnityEngine;
using System.Collections;

public class EnemyDoorInteraction : MonoBehaviour
{
    [Header("문 상호작용 설정")]
    [SerializeField] private float doorDetectionRange = 1.5f;
    [SerializeField] private float doorUseCooldown = 2f; // 문 사용 후 쿨다운
    [SerializeField] private bool canUseDoors = true;
    [SerializeField] private float playerTrackingRange = 10f; // 플레이어 추적 범위
    [SerializeField] private float playerLostCheckTime = 3f; // 플레이어를 놓쳤을 때 문 체크 시간

    private EnemyAI enemyAI;
    private EnemyMovement movement;
    private bool isUsingDoor = false;
    private float lastDoorUseTime = 0f;
    private Vector3 lastKnownPlayerPosition; // 플레이어의 마지막 위치
    private float playerLostTime = 0f; // 플레이어를 놓친 시간

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        // 플레이어 위치 추적
        if (enemyAI.Player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, enemyAI.Player.position);

            if (distToPlayer <= enemyAI.detectionRange)
            {
                lastKnownPlayerPosition = enemyAI.Player.position;
                playerLostTime = 0f;
            }
            else
            {
                playerLostTime += Time.deltaTime;
            }
        }

        if (!canUseDoors || isUsingDoor) return;

        // 문 사용 쿨다운 체크
        if (Time.time - lastDoorUseTime < doorUseCooldown) return;

        // ===== 수정: 추격 중이거나 유인 중일 때만 문 사용 (순찰 제외) =====
        if (enemyAI.CurrentState == EnemyAI.AIState.Chasing ||
            enemyAI.CurrentState == EnemyAI.AIState.DistractedByDecoy)
        {
            CheckForNearbyDoors();
        }
    }

    private void CheckForNearbyDoors()
    {
        // 근처의 모든 문 오브젝트 찾기
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
            transform.position, doorDetectionRange);

        foreach (Collider2D col in nearbyColliders)
        {
            BaseDoor door = col.GetComponent<BaseDoor>();
            if (door != null)
            {
                // 문을 향해 이동 중인지 확인
                if (IsMovingTowardsDoor(door.transform))
                {
                    StartCoroutine(UseDoor(door));
                    return;
                }
            }
        }
    }

    private bool IsMovingTowardsDoor(Transform doorTransform)
    {
        if (!movement.isMoving) return false;

        // 추격 중일 때는 특별한 조건 적용
        if (enemyAI.CurrentState == EnemyAI.AIState.Chasing)
        {
            return ShouldUseDoordDuringChase(doorTransform);
        }

        // 유인 상태에서는 기존 로직 사용
        Vector3 toDoor = (doorTransform.position - transform.position).normalized;
        Vector3 movementDirection = (movement.GetTargetPosition() - transform.position).normalized;

        // 문 방향과 이동 방향이 비슷한지 확인 (각도 45도 이내)
        float dot = Vector3.Dot(toDoor, movementDirection);
        return dot > 0.7f; // cos(45°) ≈ 0.7
    }

    // 추격 중 문을 사용해야 하는지 판단
    private bool ShouldUseDoordDuringChase(Transform doorTransform)
    {
        if (enemyAI.Player == null) return false;

        Vector3 playerPos = enemyAI.Player.position;
        Vector3 enemyPos = transform.position;
        Vector3 doorPos = doorTransform.position;

        // 1. 플레이어가 문 반대편에 있는지 확인
        BaseDoor door = doorTransform.GetComponent<BaseDoor>();
        if (door == null) return false;

        Vector3 targetPos;
        if (door.GetTargetDoor() != null)
            targetPos = door.GetTargetDoor().position;
        else
            targetPos = door.GetTargetPos();

        // 2. 플레이어를 최근에 놓쳤는지 확인 (문을 통과했을 가능성)
        bool recentlyLostPlayer = playerLostTime > 0f && playerLostTime < playerLostCheckTime;

        if (recentlyLostPlayer)
        {
            // 플레이어의 마지막 위치가 문 근처였다면 따라가야 함
            float distLastPosToTarget = Vector3.Distance(lastKnownPlayerPosition, targetPos);
            float distLastPosToDoor = Vector3.Distance(lastKnownPlayerPosition, doorPos);

            if (distLastPosToTarget < distLastPosToDoor)
            {
                Debug.Log("[EnemyDoorInteraction] 플레이어가 문을 통과한 것 같습니다. 따라갑니다!");
                return true;
            }
        }

        // 3. 플레이어가 목적지 쪽에 더 가까운지 확인 (기존 로직)
        float distPlayerToDoor = Vector3.Distance(playerPos, doorPos);
        float distPlayerToTarget = Vector3.Distance(playerPos, targetPos);

        // 플레이어가 목적지에 더 가깝다면 문을 통과해야 함
        if (distPlayerToTarget < distPlayerToDoor)
        {
            // 4. Enemy가 문 쪽으로 이동 중인지 확인
            Vector3 toDoor = (doorPos - enemyPos).normalized;
            Vector3 toPlayer = (playerPos - enemyPos).normalized;

            float dot = Vector3.Dot(toDoor, toPlayer);
            return dot > 0.5f; // 플레이어 방향과 문 방향이 비슷할 때만
        }

        return false;
    }

    private IEnumerator UseDoor(BaseDoor door)
    {
        isUsingDoor = true;
        lastDoorUseTime = Time.time;

        Debug.Log($"[EnemyDoorInteraction] {gameObject.name}이 문 {door.name}을 사용합니다! (추격/유인 중)");

        // 잠시 멈춤
        Vector3 originalTarget = movement.GetTargetPosition();
        movement.StopMoving();

        yield return new WaitForSeconds(0.3f);

        // 문 사용 (잠긴 문이든 열린 문이든 상관없이)
        TeleportThroughDoor(door);

        yield return new WaitForSeconds(0.5f);

        // 원래 목표로 계속 이동 (플레이어 추적 중이었다면 플레이어 위치로 업데이트)
        if (enemyAI.CurrentState == EnemyAI.AIState.Chasing && enemyAI.Player != null)
        {
            movement.SetTarget(enemyAI.Player.position);
            movement.isMoving = true;
            Debug.Log("[EnemyDoorInteraction] 추격 계속 - 플레이어 위치로 이동");
        }
        else if (enemyAI.CurrentState == EnemyAI.AIState.DistractedByDecoy)
        {
            // 유인 상태라면 원래 유인 목표로 복귀
            Vector3 newTarget = originalTarget;
            if (door.GetTargetDoor() != null)
            {
                // 목적지 문 근처로 목표 재설정
                Vector3 doorPos = door.GetTargetDoor().position;
                Vector3 direction = (originalTarget - transform.position).normalized;
                newTarget = doorPos + direction * 2f; // 문에서 2유닛 떨어진 곳
            }

            movement.SetTarget(newTarget);
            movement.isMoving = true;
            Debug.Log("[EnemyDoorInteraction] 유인 계속 - 목표 위치로 이동");
        }

        isUsingDoor = false;
    }

    private void TeleportThroughDoor(BaseDoor door)
    {
        Vector3 teleportPosition;

        // BaseDoor의 텔레포트 로직과 동일
        if (door.GetTargetDoor() != null)
        {
            teleportPosition = door.GetTargetDoor().position;
            Debug.Log($"[EnemyDoorInteraction] {door.GetTargetDoor().name}로 텔레포트!");
        }
        else
        {
            teleportPosition = door.GetTargetPos();
            Debug.Log($"[EnemyDoorInteraction] {door.GetTargetPos()}로 텔레포트!");
        }

        // ===== Y축 고정 기능 - 팀원들과 상담 후 사용 안하기로 결정하여 주석 처리 =====
        // if (movement.lockYPosition)
        // {
        //     teleportPosition.y = movement.fixedYPosition;
        // }

        transform.position = teleportPosition;
    }

    // 문 사용 활성화/비활성화
    public void SetCanUseDoors(bool canUse)
    {
        canUseDoors = canUse;
    }

    // 디버깅용 - 문 감지 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, doorDetectionRange);
    }
}