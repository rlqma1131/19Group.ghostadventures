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
    public bool returnToOriginalPosition = false;  // 원래 위치로 돌아갈지 여부
    public float randomPatrolRadius = 5f;  // 랜덤 순찰 반경

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
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (Player != null)
        {
            playerHide = Player.GetComponent<PlayerHide>();
            qteSystem.SetPlayer(Player);
        }

        ChangeState(AIState.Patrolling);
    }

    private void Update()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (Player != null)
            {
                playerHide = Player.GetComponent<PlayerHide>();
                qteSystem.SetPlayer(Player);
            }
            else
                return;
        }

        stateTimer += Time.deltaTime;


        UpdateCurrentState();
        CheckStateTransitions();
        UpdateAnimations();
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

                    movement.StopMoving();                    // 추적 중단
                    movement.SetTargetAwayFromPlayer(4f);     // ← 여기!
                    FindCurrentHideArea();                    // 숨은 장소 파악
                    ChangeState(AIState.SearchWaiting);       // 수색 대기
                }
                else if (!inRange)
                {
                    if (teleportSystem.canUseTeleport && teleportSystem.TryTeleportChase(Player))
                        return;

                    StartCoroutine(RunChaseResidual(Player.position));
                }
                break;

            case AIState.SearchWaiting:
                if (movement.HasReachedTarget()) // 숨은 위치에 도착했는지 확인
                {
                    Debug.Log("[EnemyAI] 숨은 위치 도착 → 복귀 시작");
                    movement.SetTarget(patrol.GetStartPosition()); // 순찰 시작 위치로 복귀
                    ChangeState(AIState.Returning);
                }
                break;

            case AIState.Returning:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (movement.HasReachedTarget(0.5f))
                    ChangeState(AIState.Waiting); // 도착 후 대기
                break;

            case AIState.Waiting:
                if (!hiding && inRange)
                    ChangeState(AIState.Chasing);
                else if (stateTimer >= returnedWaitTime)
                    ChangeState(AIState.Patrolling); // 다시 순찰 시작
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

        Ch1_HideAreaEvent.Instance.RestoreHideAreaTags();
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

        //  soundPosition을 따라갈 수 있게 dummy 오브젝트 생성
        GameObject tempTarget = new GameObject("TempDistractionTarget");
        tempTarget.transform.position = soundPosition;
        GameObject.Destroy(tempTarget, 6f); // 6초 후 제거

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
        // 현재 위치를 새로운 순찰 시작점으로 설정
        patrol.SetNewPatrolCenter(transform.position, randomPatrolRadius);
        ChangeState(AIState.Patrolling);
        Debug.Log("플레이어를 놓쳐서 현재 위치에서 랜덤 순찰 시작!");
    }

    private void FindCurrentHideArea()
    {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("HideArea");
        float closest = float.MaxValue;
        Transform nearest = null;

        foreach (GameObject obj in areas)
        {
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
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}