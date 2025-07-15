using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform Player;
    public float detectionRange = 5f;

    [Header("애니메이션 설정")]
    public Animator enemyAnimator;

    [Header("추적 종료 설정")]
    public float lostTargetWaitTime = 2f;
    public float returnedWaitTime = 1f;
    public bool returnToOriginalPosition = false;
    public float randomPatrolRadius = 5f;

    [Header("유인 설정")]
    public float distractionDuration = 5f;
    public float distractionRange = 15f;

    [Header("추격 잔상 설정")]
    public float residualChaseDuration = 2f;

    // 컴포넌트 참조
    private EnemyMovement movement;
    private EnemyTeleportSystem teleportSystem;
    private EnemyQTE qteSystem;
    private EnemyPatrol patrol;
    private EnemySearch search;

    // 상태 관리
    private float stateTimer = 0f;
    private Transform currentHideArea;
    private Transform currentDistraction;
    private Transform currentTarget;
    private float distractionTimer = 0f;

    private PlayerHide playerHide;

    // 최적화: Player 찾기 관련
    private static Transform cachedPlayer; // 모든 Enemy가 공유
    private static PlayerHide cachedPlayerHide;
    private float lastPlayerCheckTime = 0f;
    private const float PLAYER_CHECK_INTERVAL = 2f; // 2초마다만 체크



    public AIState CurrentState => currentState;

    // Y축 고정 (다른 컴포넌트에서 접근 가능하도록 public)
    public bool lockYPosition => movement.lockYPosition;
    public float fixedYPosition => movement.fixedYPosition;
    public bool isMoving
    {
        get => movement.isMoving;
        set => movement.isMoving = value;
    }

    public enum AIState
    {
        Patrolling,
        Chasing,
        SearchWaiting,
        Searching,
        SearchComplete,
        LostTarget,
        Returning,
        Waiting,
        CaughtPlayer,
        DistractedByDecoy,
        StunnedAfterQTE,
        QTEAnimation,
        ChaseResidual
    }

    private AIState currentState = AIState.Patrolling;

    private void Awake()
    {
        // 컴포넌트 가져오기
        movement = GetComponent<EnemyMovement>();
        teleportSystem = GetComponent<EnemyTeleportSystem>();
        qteSystem = GetComponent<EnemyQTE>();
        patrol = GetComponent<EnemyPatrol>();
        search = GetComponent<EnemySearch>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // 캐시된 Player가 있으면 사용, 없으면 찾기
        if (cachedPlayer != null)
        {
            Player = cachedPlayer;
            playerHide = cachedPlayerHide;
        }
        else
        {
            FindAndCachePlayer();
        }

        if (Player != null)
            qteSystem.SetPlayer(Player);

        ChangeState(AIState.Patrolling);
    }

    private void Update()
    {
        // Player 유효성 검사 및 재검색 로직
        if (!ValidatePlayer())
            return;

        stateTimer += Time.deltaTime;
        UpdateCurrentState();
        CheckStateTransitions();
        UpdateAnimations();
    }

    /// <summary>
    /// Player 유효성 검사 및 필요시 재검색
    /// </summary>
    private bool ValidatePlayer()
    {
        // Player 객체가 파괴되었는지 체크
        if (Player != null && Player.gameObject == null)
        {
            Player = null;
            playerHide = null;
            cachedPlayer = null;
            cachedPlayerHide = null;
        }

        // Player가 null이고 충분한 시간이 지났을 때만 재검색
        if (Player == null && Time.time - lastPlayerCheckTime > PLAYER_CHECK_INTERVAL)
        {
            FindAndCachePlayer();
            lastPlayerCheckTime = Time.time;
        }

        return Player != null;
    }

    /// <summary>
    /// Player를 찾고 캐싱하는 메서드
    /// </summary>
    private void FindAndCachePlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            cachedPlayer = playerObj.transform;
            cachedPlayerHide = playerObj.GetComponent<PlayerHide>();

            Player = cachedPlayer;
            playerHide = cachedPlayerHide;

            if (qteSystem != null)
                qteSystem.SetPlayer(Player);

            Debug.Log($"[EnemyAI] Player 찾음: {playerObj.name}");
        }
    }

    /// <summary>
    /// 외부에서 Player 참조를 업데이트할 때 사용
    /// </summary>
    public static void UpdatePlayerReference(Transform newPlayer)
    {
        if (newPlayer != null)
        {
            cachedPlayer = newPlayer;
            cachedPlayerHide = newPlayer.GetComponent<PlayerHide>();
        }
        else
        {
            cachedPlayer = null;
            cachedPlayerHide = null;
        }
    }

    /// <summary>
    /// 씬 전환 시 모든 캐시 초기화
    /// </summary>
    public static void ClearAllCaches()
    {
        cachedPlayer = null;
        cachedPlayerHide = null;
        cachedHideAreas = null;
        lastHideAreaCacheTime = 0f;
    }

    private void OnDestroy()
    {
        // 이 인스턴스가 마지막 EnemyAI라면 캐시 정리
        if (FindObjectsOfType<EnemyAI>().Length <= 1)
        {
            ClearAllCaches();
        }
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case AIState.Patrolling:
                patrol.SetNextPatrolTarget();
                break;
            case AIState.Chasing:
                enemyAnimator?.SetBool("IsWalking", true);
                break;
            case AIState.CaughtPlayer:
                movement.StopMoving();
                break;
            case AIState.Searching:
                search.SetupSearchPattern();
                break;
            case AIState.Returning:
                movement.SetTarget(patrol.GetStartPosition());
                enemyAnimator?.SetBool("IsWalking", true);
                break;
            default:
                movement.StopMoving();
                enemyAnimator?.SetBool("IsWalking", false);
                break;
        }
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                patrol.UpdatePatrolling();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.ChaseResidual:
                break;
            case AIState.SearchWaiting:
            case AIState.LostTarget:
            case AIState.Waiting:
            case AIState.CaughtPlayer:
            case AIState.StunnedAfterQTE:
            case AIState.QTEAnimation:
                movement.StopMoving();
                break;
            case AIState.Searching:
                search.UpdateSearching();
                break;
            case AIState.SearchComplete:
                search.UpdateSearchComplete();
                break;
            case AIState.Returning:
                movement.MoveToTarget(movement.moveSpeed * 0.9f);
                break;
            case AIState.DistractedByDecoy:
                UpdateDistractedState();
                break;
        }
    }

    private void UpdateChasing()
    {
        if (Player != null)
        {
            movement.SetTarget(Player.position);
            movement.MoveToTarget(movement.chaseSpeed);

            // null 체크 추가
            if (Ch1_HideAreaEvent.Instance != null)
                Ch1_HideAreaEvent.Instance.UnTagAllHideAreas();
        }
    }

    private void UpdateDistractedState()
    {
        distractionTimer += Time.deltaTime;

        if (currentDistraction != null)
        {
            movement.lockYPosition = false;

            movement.SetTarget(currentDistraction.position);
            movement.MoveToTarget(movement.distractionSpeed);

            // 도착하면 종료
            if (movement.HasReachedTarget())
            {
                Debug.Log("[Distracted] 목표 지점 도착! 유인 상태 종료");
                EndDistraction();
            }
        }
    }

    private void CheckStateTransitions()
    {
        if (Player == null)
        {
            if (currentState != AIState.Patrolling)
                ChangeState(AIState.Patrolling);
            return;
        }

        bool hiding = playerHide != null && playerHide.IsHiding;
        float dist = Vector3.Distance(transform.position, Player.position);
        bool inRange = dist < detectionRange;

        switch (currentState)
        {
            case AIState.Patrolling:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                break;

            case AIState.Chasing:
                if (hiding)
                {
                    Debug.Log("[EnemyAI] 플레이어가 숨었습니다 → 숨은 위치까지 접근 후 복귀");

                    movement.StopMoving();
                    movement.SetTargetAwayFromPlayer(4f);
                    FindCurrentHideArea();
                    ChangeState(AIState.SearchWaiting);
                }
                else if (!inRange)
                {
                    if (teleportSystem.canUseTeleport && teleportSystem.TryTeleportChase(Player))
                        return;

                    StartCoroutine(RunChaseResidual(Player.position));
                }
                break;

            case AIState.SearchWaiting:
                if (movement.HasReachedTarget())
                {
                    Debug.Log("[EnemyAI] 숨은 위치 도착 → 복귀 시작");
                    movement.SetTarget(patrol.GetStartPosition());
                    ChangeState(AIState.Returning);
                }
                break;

            case AIState.Returning:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (movement.HasReachedTarget(0.5f))
                    ChangeState(AIState.Waiting);
                break;

            case AIState.Waiting:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (stateTimer >= returnedWaitTime)
                    ChangeState(AIState.Patrolling);
                break;

            case AIState.Searching:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                break;

            case AIState.SearchComplete:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (stateTimer >= search.GetSearchEndWaitTime())
                    ChangeState(AIState.Returning);
                break;

            case AIState.LostTarget:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (stateTimer >= lostTargetWaitTime)
                {
                    if (returnToOriginalPosition)
                        ChangeState(AIState.Returning);
                    else
                        StartRandomPatrol();
                }
                break;

            case AIState.DistractedByDecoy:
                if (!hiding && inRange)
                {
                    currentDistraction = null;
                    ChangeState(AIState.Chasing);
                }
                break;
        }
    }

    private IEnumerator RunChaseResidual(Vector3 targetPos)
    {
        ChangeState(AIState.ChaseResidual);
        movement.SetTarget(targetPos);

        float timer = 0f;
        while (timer < residualChaseDuration)
        {
            movement.MoveToTarget(movement.chaseSpeed);
            timer += Time.deltaTime;
            yield return null;
        }

        Ch1_HideAreaEvent.Instance?.RestoreHideAreaTags(); // null 체크 추가
        ChangeState(AIState.LostTarget);
    }

    private void UpdateAnimations()
    {
        if (currentState != AIState.CaughtPlayer && currentState != AIState.QTEAnimation)
        {
            bool shouldWalk = movement.isMoving && (
                currentState == AIState.Patrolling ||
                currentState == AIState.Chasing ||
                currentState == AIState.ChaseResidual ||
                currentState == AIState.Returning ||
                currentState == AIState.DistractedByDecoy ||
                (currentState == AIState.Searching) ||
                currentState == AIState.SearchComplete);

            enemyAnimator?.SetBool("IsWalking", shouldWalk);
        }
    }

    public void OnSoundDetected(Vector3 soundPosition)
    {
        if (currentState == AIState.CaughtPlayer || currentState == AIState.StunnedAfterQTE)
            return;

        Debug.Log("[EnemyAI] 소리를 감지했습니다! 해당 위치로 이동합니다.");

        GameObject tempTarget = new GameObject("TempDistractionTarget");
        tempTarget.transform.position = soundPosition;
        GameObject.Destroy(tempTarget, 6f);

        currentDistraction = tempTarget.transform;
        movement.SetTarget(soundPosition);
        distractionTimer = 0f;

        ChangeState(AIState.DistractedByDecoy);
    }

    public void GetDistractedBy(Transform distractionObject)
    {
        if (currentState == AIState.CaughtPlayer || currentState == AIState.StunnedAfterQTE)
            return;

        currentDistraction = distractionObject;
        distractionTimer = 0f;
        ChangeState(AIState.DistractedByDecoy);
    }

    public void EndDistraction()
    {
        if (currentState == AIState.DistractedByDecoy)
        {
            currentDistraction = null;
            ChangeState(AIState.Patrolling);
        }
    }

    public void StopMoving() => movement.StopMoving();

    private void StartRandomPatrol()
    {
        patrol.SetNewPatrolCenter(transform.position, randomPatrolRadius);
        ChangeState(AIState.Patrolling);
        Debug.Log("플레이어를 놓쳐서 현재 위치에서 랜덤 순찰 시작!");
    }

    // HideArea 캐싱
    private static GameObject[] cachedHideAreas;
    private static float lastHideAreaCacheTime = 0f;
    private const float HIDE_AREA_CACHE_INTERVAL = 5f; // 5초마다 갱신

    private void FindCurrentHideArea()
    {
        // HideArea 캐시 업데이트
        if (cachedHideAreas == null || Time.time - lastHideAreaCacheTime > HIDE_AREA_CACHE_INTERVAL)
        {
            cachedHideAreas = GameObject.FindGameObjectsWithTag("HideArea");
            lastHideAreaCacheTime = Time.time;
        }

        if (cachedHideAreas == null || cachedHideAreas.Length == 0 || Player == null)
        {
            currentHideArea = null;
            return;
        }

        float closest = float.MaxValue;
        Transform nearest = null;

        foreach (GameObject obj in cachedHideAreas)
        {
            if (obj == null) continue; // null 체크

            float d = Vector3.Distance(Player.position, obj.transform.position);
            if (d < closest)
            {
                closest = d;
                nearest = obj.transform;
            }
        }

        currentHideArea = nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}