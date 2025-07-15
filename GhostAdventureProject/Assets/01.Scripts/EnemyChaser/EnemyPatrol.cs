using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("기본 순찰 설정")]
    public float patrolWaitTime = 1f;
    public float patrolRadius = 3f; // 시작 위치 기준 순찰 반경 (10 → 3으로 줄임)

    [Header("맵 중앙 복귀 시스템")]
    public Vector3 mapCenter = Vector3.zero; // 맵 중심점
    public float maxDistanceFromCenter = 30f; // 중심에서 최대 거리
    public float centerReturnForce = 0.7f; // 중심으로 복귀하는 강도 (0~1)

    [Header("문 탈출 설정")]
    public float doorEscapeTime = 7f; // 7초 후 문으로 탈출
    public float doorSearchRadius = 15f; // 문 검색 반경
    public float doorCooldownTime = 30f; // 사용한 문 쿨다운 시간
    public int nearDoorCandidates = 3; // 가까운 문 후보 개수

    [Header("🎯 EnemyMovement 연동 최적화")]
    public bool forceOptimizedMovement = true; // EnemyMovement 최적화 모드 강제 활성화

    [Header("🎯 TargetDummy 최적화")]
    public float targetUpdateInterval = 2f; // 타겟 업데이트 간격 (초)
    public float targetReachThreshold = 0.5f; // 타겟 도달 판정 거리

    private Vector3 startPos;
    private Vector3 currentTarget;
    private bool isPatrolWaiting = false;
    private float waitTimer = 0f;

    // 🎯 TargetDummy 생성 최소화
    private float lastTargetUpdateTime = 0f;
    private bool hasValidTarget = false;
    private Vector3 lastSetTarget = Vector3.zero;

    // 문 탈출 관련
    private float patrolTimer = 0f; // 순찰 상태 지속 시간
    private bool isEscapingToDoor = false;
    private GameObject targetDoor = null;

    // 문 사용 쿨다운 관리
    private System.Collections.Generic.Dictionary<GameObject, float> usedDoors =
        new System.Collections.Generic.Dictionary<GameObject, float>();

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
        patrolTimer = 0f; // 타이머 초기화
        lastTargetUpdateTime = Time.time;

        // 🎯 EnemyMovement 최적화 모드 강제 활성화
        if (forceOptimizedMovement && movement != null)
        {
            // EnemyMovement의 useOptimizedTarget을 true로 설정
            var optimizedField = movement.GetType().GetField("useOptimizedTarget",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (optimizedField != null)
            {
                optimizedField.SetValue(movement, true);
                Debug.Log($"[EnemyPatrol] EnemyMovement 최적화 모드 강제 활성화!");
            }
        }

        GeneratePatrolTarget();
    }

    public void SetNewPatrolCenter(Vector3 newCenter, float radius)
    {
        // 새로운 중심점으로 순찰 시작
        startPos = newCenter;
        patrolTimer = 0f; // 타이머 리셋
        isEscapingToDoor = false;
        targetDoor = null;
        lastTargetUpdateTime = Time.time; // 🎯 타겟 업데이트 시간 리셋
        hasValidTarget = false;
        Debug.Log($"[EnemyPatrol] 새로운 순찰 중심점: {newCenter}");
        GeneratePatrolTarget();
    }

    public void UpdatePatrolling()
    {
        // ===== 중요: 순찰 상태일 때만 타이머 증가 =====
        if (enemyAI.CurrentState == EnemyAI.AIState.Patrolling)
        {
            patrolTimer += Time.deltaTime;

            // 7초가 지나면 문 탈출 시도
            if (patrolTimer >= doorEscapeTime && !isEscapingToDoor)
            {
                Debug.Log($"[EnemyPatrol] {gameObject.name} - 7초 경과! 문 탈출 시도");
                TryEscapeToDoor();
                return;
            }
        }

        // 문 탈출 중이면 문 탈출 로직 실행
        if (isEscapingToDoor)
        {
            UpdateDoorEscape();
            return;
        }

        // 🎯 최적화된 순찰 로직
        if (isPatrolWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isPatrolWaiting = false;
                // 🎯 타겟 업데이트 간격 체크
                if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
                {
                    GeneratePatrolTarget(); // 새로운 순찰 타겟 생성
                    lastTargetUpdateTime = Time.time;
                }
                else
                {
                    // 기존 타겟이 유효하면 재사용
                    if (hasValidTarget && Vector3.Distance(transform.position, currentTarget) > targetReachThreshold)
                    {
                        Debug.Log($"[EnemyPatrol] 기존 타겟 재사용: {currentTarget}");
                        // SetTarget 호출 없이 기존 타겟 사용
                    }
                    else
                    {
                        GeneratePatrolTarget();
                        lastTargetUpdateTime = Time.time;
                    }
                }
                waitTimer = 0f;
            }
            return;
        }

        // 🎯 도달 판정 최적화
        if (movement.HasReachedTarget(targetReachThreshold))
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

    private void TryEscapeToDoor()
    {
        GameObject nearestDoor = FindNearestDoor();

        if (nearestDoor != null)
        {
            Debug.Log($"[EnemyPatrol] {gameObject.name} - 가장 가까운 문 발견: {nearestDoor.name}");
            targetDoor = nearestDoor;
            isEscapingToDoor = true;
            hasValidTarget = false; // 🎯 기존 타겟 무효화

            // 🎯 최적화된 문 이동
            SetTargetSafely(targetDoor.transform.position);
        }
        else
        {
            Debug.Log($"[EnemyPatrol] {gameObject.name} - 근처에 문을 찾을 수 없음. 순찰 계속");
            patrolTimer = 0f; // 타이머만 리셋하고 계속 순찰
        }
    }

    private void UpdateDoorEscape()
    {
        if (targetDoor == null)
        {
            // 타겟 문이 없어졌으면 순찰 모드로 복귀
            Debug.Log("[EnemyPatrol] 타겟 문이 사라짐. 순찰 재개");
            isEscapingToDoor = false;
            patrolTimer = 0f;
            hasValidTarget = false;
            lastTargetUpdateTime = Time.time;
            GeneratePatrolTarget();
            return;
        }

        // 문에 도착했는지 확인 (넉넉하게 0.8f)
        if (movement.HasReachedTarget(0.8f))
        {
            Debug.Log($"[EnemyPatrol] {gameObject.name} - 문에 도착! 문 사용");
            UseDoor();
        }
        else
        {
            // 문으로 이동 (조금 더 빠르게)
            movement.MoveToTarget(movement.patrolSpeed * 1.5f);
        }
    }

    private void UseDoor()
    {
        if (targetDoor == null) return;

        BaseDoor doorScript = targetDoor.GetComponent<BaseDoor>();
        if (doorScript == null)
        {
            Debug.LogWarning($"[EnemyPatrol] {targetDoor.name}에 BaseDoor 스크립트가 없음!");
            isEscapingToDoor = false;
            patrolTimer = 0f;
            hasValidTarget = false;
            GeneratePatrolTarget();
            return;
        }

        // 사용한 문 기록 (쿨다운 적용)
        usedDoors[targetDoor] = Time.time;
        Debug.Log($"[EnemyPatrol] {targetDoor.name} 사용 기록 - 쿨다운 시작");

        // 문의 목적지 가져오기
        Vector3 teleportPosition = Vector3.zero;

        if (doorScript.GetTargetDoor() != null)
        {
            teleportPosition = doorScript.GetTargetDoor().position;
            Debug.Log($"[EnemyPatrol] {doorScript.GetTargetDoor().name}로 텔레포트!");
        }
        else if (doorScript.GetTargetPos() != Vector2.zero)
        {
            teleportPosition = doorScript.GetTargetPos();
            Debug.Log($"[EnemyPatrol] {doorScript.GetTargetPos()}로 텔레포트!");
        }
        else
        {
            Debug.LogWarning($"[EnemyPatrol] {targetDoor.name} 문의 목적지가 설정되지 않음!");
            isEscapingToDoor = false;
            patrolTimer = 0f;
            hasValidTarget = false;
            GeneratePatrolTarget();
            return;
        }

        // Y축 고정 적용
        if (movement.lockYPosition)
        {
            teleportPosition.y = movement.fixedYPosition;
        }

        // 텔레포트 실행!
        transform.position = teleportPosition;
        Debug.Log($"[EnemyPatrol] {gameObject.name} - 텔레포트 완료: {teleportPosition}");

        // ===== 핵심: 새로운 방의 위치를 맵 중심으로 업데이트 =====
        Vector3 oldCenter = mapCenter;
        mapCenter = teleportPosition;
        Debug.Log($"[EnemyPatrol] 맵 중심점 업데이트: {oldCenter} → {mapCenter}");

        // 새로운 위치에서 순찰 재시작
        SetNewPatrolCenter(teleportPosition, patrolRadius);
    }

    private GameObject FindNearestDoor()
    {
        // 현재 위치와 맵 중심 사이의 거리 계산
        float distanceFromCenter = Vector3.Distance(transform.position, mapCenter);

        // 동적 검색 범위: 중심에서 멀수록 검색 범위 확대
        float dynamicSearchRadius = doorSearchRadius + (distanceFromCenter * 0.3f);

        // 최대 검색 범위 제한 (너무 커지지 않도록)
        dynamicSearchRadius = Mathf.Min(dynamicSearchRadius, doorSearchRadius * 3f);

        Debug.Log($"[EnemyPatrol] 동적 문 검색: 기본범위={doorSearchRadius}, 중심거리={distanceFromCenter:F1}, 확장범위={dynamicSearchRadius:F1}");

        // "Door" 태그를 가진 모든 오브젝트 찾기
        GameObject[] allDoors = GameObject.FindGameObjectsWithTag("Door");

        if (allDoors.Length == 0)
        {
            Debug.LogWarning("[EnemyPatrol] 'Door' 태그를 가진 오브젝트가 없습니다!");
            return null;
        }

        // 사용 가능한 문들만 필터링 (쿨다운 체크)
        System.Collections.Generic.List<GameObject> availableDoors =
            new System.Collections.Generic.List<GameObject>();

        foreach (GameObject door in allDoors)
        {
            // BaseDoor 스크립트가 있는지 확인
            BaseDoor doorScript = door.GetComponent<BaseDoor>();
            if (doorScript == null) continue;

            float distance = Vector3.Distance(transform.position, door.transform.position);

            // 동적 검색 반경 내에 있는지 확인
            if (distance > dynamicSearchRadius) continue;

            // 쿨다운 체크
            if (usedDoors.ContainsKey(door))
            {
                float timeSinceUsed = Time.time - usedDoors[door];
                if (timeSinceUsed < doorCooldownTime)
                {
                    Debug.Log($"[EnemyPatrol] {door.name} - 쿨다운 중 (남은시간: {doorCooldownTime - timeSinceUsed:F1}초)");
                    continue; // 쿨다운 중이면 제외
                }
                else
                {
                    // 쿨다운이 끝났으면 딕셔너리에서 제거
                    usedDoors.Remove(door);
                }
            }

            availableDoors.Add(door);
        }

        if (availableDoors.Count == 0)
        {
            Debug.Log($"[EnemyPatrol] 사용 가능한 문이 없음 (모두 쿨다운 중이거나 범위 밖) - 검색범위: {dynamicSearchRadius:F1}");
            return null;
        }

        // 거리순으로 정렬
        availableDoors.Sort((door1, door2) =>
        {
            float dist1 = Vector3.Distance(transform.position, door1.transform.position);
            float dist2 = Vector3.Distance(transform.position, door2.transform.position);
            return dist1.CompareTo(dist2);
        });

        // 가까운 문들 중에서 랜덤 선택 (최대 nearDoorCandidates 개)
        int candidateCount = Mathf.Min(nearDoorCandidates, availableDoors.Count);
        int randomIndex = Random.Range(0, candidateCount);
        GameObject selectedDoor = availableDoors[randomIndex];

        float selectedDistance = Vector3.Distance(transform.position, selectedDoor.transform.position);
        Debug.Log($"[EnemyPatrol] 선택된 문: {selectedDoor.name} (거리: {selectedDistance:F1}, {candidateCount}개 후보 중 선택, 검색범위: {dynamicSearchRadius:F1})");

        return selectedDoor;
    }

    public void SetNextPatrolTarget()
    {
        // 🎯 간격 체크 후 타겟 생성
        if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
        {
            GeneratePatrolTarget();
            lastTargetUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 🎯 안전한 타겟 설정 (중복 호출 방지)
    /// </summary>
    private void SetTargetSafely(Vector3 newTarget)
    {
        // 이전 타겟과 너무 가까우면 SetTarget 호출 생략
        if (Vector3.Distance(lastSetTarget, newTarget) < 0.1f)
        {
            Debug.Log($"[EnemyPatrol] 타겟 중복 방지: {newTarget}");
            return;
        }

        lastSetTarget = newTarget;
        movement.SetTarget(newTarget);
        hasValidTarget = true;
        Debug.Log($"[EnemyPatrol] 🎯 최적화된 타겟 설정: {newTarget} (TargetDummy 생성 최소화)");
    }

    private void GeneratePatrolTarget()
    {
        // 🎯 타겟 생성 빈도 제한
        if (Time.time - lastTargetUpdateTime < targetUpdateInterval)
        {
            Debug.Log($"[EnemyPatrol] 타겟 업데이트 간격 미충족 ({Time.time - lastTargetUpdateTime:F1}초 < {targetUpdateInterval}초)");
            return;
        }

        // 현재 위치를 기준으로 순찰 (startPos 대신 현재 위치 사용)
        Vector3 basePos = transform.position;

        // 맵 중심에서 너무 멀어졌는지 확인
        float distanceFromCenter = Vector3.Distance(basePos, mapCenter);
        bool tooFarFromCenter = distanceFromCenter > maxDistanceFromCenter;

        float randomX;

        if (tooFarFromCenter)
        {
            // 중심으로 복귀하도록 방향 조정
            Vector3 directionToCenter = (mapCenter - basePos).normalized;
            float centerBias = directionToCenter.x * patrolRadius * centerReturnForce;

            // 중심 방향으로 편향된 랜덤 값 생성
            float randomRange = patrolRadius * (1f - centerReturnForce);
            randomX = basePos.x + centerBias + Random.Range(-randomRange, randomRange);

            Debug.Log($"[EnemyPatrol] 중심에서 너무 멀음 (거리: {distanceFromCenter:F1}) → 중심 방향으로 편향 이동");
        }
        else
        {
            // 일반적인 랜덤 순찰
            randomX = basePos.x + Random.Range(-patrolRadius, patrolRadius);
        }

        Vector3 newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

        // 벽 충돌 체크 - 범위를 더 엄격하게 체크
        if (IsBlocked(newTarget))
        {
            Debug.Log($"[EnemyPatrol] 첫 번째 타겟 {newTarget} 충돌 감지");

            // 충돌하면 더 가까운 범위에서 다시 시도
            float smallerRadius = patrolRadius * 0.5f;

            if (tooFarFromCenter)
            {
                // 중심 방향으로 다시 시도
                Vector3 directionToCenter = (mapCenter - basePos).normalized;
                randomX = basePos.x + directionToCenter.x * smallerRadius;
            }
            else
            {
                randomX = basePos.x + Random.Range(-smallerRadius, smallerRadius);
            }

            newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

            if (IsBlocked(newTarget))
            {
                Debug.Log($"[EnemyPatrol] 두 번째 타겟 {newTarget} 충돌 감지");

                // 그래도 충돌하면 현재 위치 근처에서 아주 작은 범위로
                float tinyRadius = 1f;
                randomX = basePos.x + Random.Range(-tinyRadius, tinyRadius);
                newTarget = new Vector3(randomX, movement.fixedYPosition, transform.position.z);

                if (IsBlocked(newTarget))
                {
                    // 마지막으로도 충돌하면 현재 위치 그대로
                    newTarget = new Vector3(basePos.x, movement.fixedYPosition, transform.position.z);
                    Debug.Log($"[EnemyPatrol] 모든 시도 실패, 현재 위치에서 대기: {newTarget}");
                }
            }
        }

        currentTarget = newTarget;
        // 🎯 최적화된 EnemyMovement와 연동
        SetTargetSafely(currentTarget);

        float distance = Vector3.Distance(transform.position, currentTarget);
        Debug.Log($"[EnemyPatrol] {gameObject.name} 🎯 최적화 순찰 타겟: {currentTarget} (거리: {distance:F1}) [기준위치: {basePos}, 중심거리: {distanceFromCenter:F1}]");

        // 거리가 비정상적으로 크면 경고
        if (distance > patrolRadius + 1f)
        {
            Debug.LogWarning($"[EnemyPatrol] 순찰 타겟이 너무 멀음! 거리: {distance:F1}, 설정 반경: {patrolRadius}");
        }
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

        // 문 검색 범위 표시 (동적)
        if (Application.isPlaying)
        {
            float distanceFromCenter = Vector3.Distance(transform.position, mapCenter);
            float dynamicSearchRadius = doorSearchRadius + (distanceFromCenter * 0.3f);
            dynamicSearchRadius = Mathf.Min(dynamicSearchRadius, doorSearchRadius * 3f);

            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, dynamicSearchRadius);

            // 기본 검색 범위도 표시 (비교용)
            Gizmos.color = new Color(1, 0.5f, 0, 0.1f);
            Gizmos.DrawWireSphere(transform.position, doorSearchRadius);
        }
        else
        {
            // 편집 모드에서는 기본 범위만 표시
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, doorSearchRadius);
        }

        // 맵 중심점과 최대 거리 표시
        Gizmos.color = new Color(0, 1, 1, 0.5f); // 시안색
        Gizmos.DrawWireSphere(mapCenter, 0.8f); // 맵 중심점
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(mapCenter, maxDistanceFromCenter); // 최대 거리 범위

        // 현재 위치에서 맵 중심까지의 라인
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawLine(transform.position, mapCenter);

        // 현재 타겟 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentTarget, 0.5f);

        // 시작 위치 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.3f);

        // 현재 위치에서 타겟으로의 라인
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, currentTarget);

        // 문 탈출 중이면 타겟 문 표시
        if (isEscapingToDoor && targetDoor != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetDoor.transform.position);
            Gizmos.DrawWireSphere(targetDoor.transform.position, 1f);
        }

        // 7초 타이머 표시 (Scene 뷰에서 확인용)
        if (Application.isPlaying && enemyAI != null && enemyAI.CurrentState == EnemyAI.AIState.Patrolling)
        {
            float timerRatio = patrolTimer / doorEscapeTime;
            Gizmos.color = Color.Lerp(Color.green, Color.red, timerRatio);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f + timerRatio * 0.5f);
        }

        // 🎯 TargetDummy 최적화 상태 표시
        if (Application.isPlaying)
        {
            float timeSinceLastUpdate = Time.time - lastTargetUpdateTime;
            float updateProgress = timeSinceLastUpdate / targetUpdateInterval;

            Gizmos.color = hasValidTarget ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * (0.5f + updateProgress * 0.5f));
        }
    }
}