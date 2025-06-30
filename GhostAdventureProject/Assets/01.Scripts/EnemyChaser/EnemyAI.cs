using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform Player;
    public float detectionRange = 5f;
    public float moveSpeed = 2f;

    [Header("QTE 설정")]
    public float catchRange = 1.5f; // 플레이어를 잡는 범위

    [Header("애니메이션 설정")]
    public Animator enemyAnimator; // 적 애니메이터

    [Header("순찰 설정")]
    public float patrolDistance = 3f;
    public float patrolWaitTime = 1f;

    [Header("수색 설정")]
    public float searchWaitTime = 2f;
    public float searchEndWaitTime = 2f;
    public float searchDistance = 1.5f;
    public int searchLaps = 2;

    [Header("추적 종료 설정")]
    public float lostTargetWaitTime = 2f;
    public float returnedWaitTime = 1f;

    [Header("유인 오브젝트 설정")]
    public float distractionRange = 15f; // 유인 감지 범위

    [Header("생명 시스템 설정")]
    public int maxPlayerLives = 2; // 챕터당 목숨 2개
    private int currentPlayerLives;

    private Vector3 startPos;
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float stateTimer = 0f;
    private bool isPatrolWaiting = false;

    private Vector3 searchCenter;
    private int currentSearchLap = 0;
    private bool searchingRight = true;
    private bool isSearchWaiting = false;

    private PlayerHide playerHide;
    private Transform currentHideArea;
    private Transform currentDistraction; // 현재 끌린 유인 오브젝트

    private enum AIState
    {
        Patrolling,
        Chasing,
        SearchWaiting,
        Searching,
        SearchComplete,
        LostTarget,
        Returning,
        Waiting,
        CaughtPlayer,        // 플레이어를 잡은 상태
        DistractedByDecoy,   // 유인 오브젝트에 끌린 상태
        StunnedAfterQTE      // QTE 후 스턴 상태
    }

    private AIState currentState = AIState.Patrolling;

    void Start()
    {
        startPos = transform.position;
        currentPlayerLives = maxPlayerLives;

        // GameManager를 통해 실제 생성된 플레이어 찾기
        if (Player == null && GameManager.Instance != null)
        {
            GameObject playerObj = GameManager.Instance.Player;
            if (playerObj != null)
            {
                Player = playerObj.transform;
                playerHide = Player.GetComponent<PlayerHide>();
                Debug.Log("GameManager를 통해 플레이어를 찾았습니다!");
            }
        }

        SetupPatrolPoints();
        ChangeState(AIState.Patrolling);
    }

    void Update()
    {
        // 플레이어가 없으면 다시 찾기 시도
        if (Player == null && GameManager.Instance != null)
        {
            GameObject playerObj = GameManager.Instance.Player;
            if (playerObj != null)
            {
                Player = playerObj.transform;
                playerHide = Player.GetComponent<PlayerHide>();
                Debug.Log("런타임에 플레이어를 찾았습니다!");
            }
            return; // 플레이어가 없으면 AI 로직 실행 안함
        }

        // 플레이어를 잡는 범위 체크 (기존 코드)
        if (currentState == AIState.Chasing &&
            Vector3.Distance(transform.position, Player.position) <= catchRange)
        {
            TryCatchPlayer();
            return;
        }

        stateTimer += Time.deltaTime;
        UpdateCurrentState();
        CheckStateTransitions();
    }

    void ChangeState(AIState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case AIState.Patrolling:
                SetNextPatrolTarget();
                isPatrolWaiting = false;
                break;
            case AIState.Searching:
                SetupSearchPattern();
                isSearchWaiting = false;
                break;
            case AIState.Returning:
                SetTarget(startPos);
                break;
            default:
                StopMoving();
                break;
        }
    }

    // ================================
    // QTE 시스템 관련 메서드들
    // ================================

    void TryCatchPlayer()
    {
        StopMoving();
        ChangeState(AIState.CaughtPlayer);

        // 플레이어를 잡았을 때 애니메이션 재생
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("CatchPlayer");
        }

        Debug.Log("플레이어를 잡았습니다! QTE 시작!");

        // TODO: QTE UI 연결 (팀원이 구현 예정)
        /*
        QTEUI qte = FindObjectOfType<QTEUI>();
        if (qte != null)
        {
            qte.ShowQTEUI((bool success) =>
            {
                if (success)
                {
                    OnQTESuccess();
                }
                else
                {
                    OnQTEFailure();
                }
            });
        }
        */

        // 임시로 QTE 성공 처리 (테스트용)
        OnQTESuccess();
    }

    // QTE 성공 시 호출되는 메서드
    void OnQTESuccess()
    {
        Debug.Log("QTE 성공! 플레이어가 탈출했습니다.");

        // 플레이어가 탈출했을 때 애니메이션 재생
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("PlayerEscaped");
        }

        // 생명 감소 및 스턴 처리
        LosePlayerLife();  // 생명 관리 스크립트도 이름 바꿔야함 
    }

    // QTE 실패 시 호출되는 메서드
    void OnQTEFailure()
    {
        Debug.Log("QTE 실패! 플레이어가 잡혔습니다.");

        // 플레이어를 잡았을 때 애니메이션 재생
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("PlayerCaught");
        }

        // 게임오버 처리
        HandleGameOver(); // 나중에 이름 바꿔야함 
    }

    // ================================
    // 생명 시스템 관련 메서드들
    // ================================

    void LosePlayerLife()
    {
        currentPlayerLives--;
        Debug.Log($"생명 감소! 남은 생명: {currentPlayerLives}");

        if (currentPlayerLives <= 0)
        {
            HandleGameOver();
        }
        else
        {
            // 2초 스턴 후 다시 추적
            StartCoroutine(StunAfterQTE());
        }
    }

    void HandleGameOver()
    {
        Debug.Log("게임오버!");

        // TODO: 게임오버 UI 표시 (팀원이 구현 예정)
        /*
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        */

        // 임시로 게임 일시정지
        Time.timeScale = 0f;
    }

    IEnumerator StunAfterQTE()
    {
        ChangeState(AIState.StunnedAfterQTE);

        // 적 스턴 애니메이션 재생
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsStunned", true);
        }

        Debug.Log("적이 2초간 스턴됩니다.");
        yield return new WaitForSeconds(2f); // 2초 스턴

        // 스턴 애니메이션 해제
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsStunned", false);
        }

        // 플레이어가 감지 범위에 있는지 확인 후 추격 또는 순찰
        if (Player != null && Vector3.Distance(transform.position, Player.position) < detectionRange)
        {
            ChangeState(AIState.Chasing);
            Debug.Log("스턴 해제! 플레이어를 다시 추격합니다.");
        }
        else
        {
            ChangeState(AIState.Patrolling);
            Debug.Log("스턴 해제! 순찰을 재개합니다.");
        }
    }

    // ================================
    // 유인 오브젝트 관련 메서드들
    // ================================

    // 유인 오브젝트에 끌리는 메서드
    public void GetDistractedBy(Transform distractionObject)
    {
        // QTE 중이거나 스턴 중일 때는 유인 안됨
        if (currentState == AIState.CaughtPlayer || currentState == AIState.StunnedAfterQTE)
            return;

        currentDistraction = distractionObject;
        ChangeState(AIState.DistractedByDecoy);

        Debug.Log("적이 유인 오브젝트에 끌렸습니다!");
    }

    // 유인 해제 메서드
    public void EndDistraction()
    {
        if (currentState == AIState.DistractedByDecoy)
        {
            currentDistraction = null;
            ChangeState(AIState.Patrolling);
            Debug.Log("유인 효과가 끝났습니다.");
        }
    }

    // 소리 기반 유인 메서드
    public void OnSoundDetected(Vector3 soundPosition)
    {
        // 소리 감지 범위 체크
        if (currentState == AIState.CaughtPlayer || currentState == AIState.StunnedAfterQTE)
        {
            Debug.Log("적이 QTE 중이거나 스턴 상태라 소리를 무시합니다.");
            return;
        }

        Debug.Log("소리를 감지했습니다! 해당 위치로 이동합니다.");

        // TODO: 소리 위치로 이동하는 로직 구현
        /*
        SetTarget(soundPosition);
        ChangeState(AIState.DistractedByDecoy);
        */

    }

    void UpdateCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                UpdatePatrolling();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.SearchWaiting:
                StopMoving();
                break;
            case AIState.Searching:
                UpdateSearching();
                break;
            case AIState.SearchComplete:
                UpdateSearchComplete();
                break;
            case AIState.LostTarget:
                StopMoving();
                break;
            case AIState.Returning:
                MoveToTarget(moveSpeed * 0.9f);
                break;
            case AIState.Waiting:
                StopMoving();
                break;
            case AIState.CaughtPlayer:
                StopMoving(); // QTE 진행 중에는 움직이지 않음
                break;
            case AIState.DistractedByDecoy:
                UpdateDistractedState();
                break;
            case AIState.StunnedAfterQTE:
                StopMoving(); // 스턴 중에는 움직이지 않음
                break;
        }
    }

    void UpdateDistractedState()
    {
        if (currentDistraction != null)
        {
            SetTarget(currentDistraction.position);
            MoveToTarget(moveSpeed * 0.8f); // 조금 느리게 이동
        }
    }

    void CheckStateTransitions()
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
                if (hiding) { FindCurrentHideArea(); ChangeState(AIState.SearchWaiting); }
                else if (inRange) ChangeState(AIState.Chasing);
                break;
            case AIState.Chasing:
                if (hiding) { FindCurrentHideArea(); ChangeState(AIState.SearchWaiting); }
                else if (!inRange) ChangeState(AIState.LostTarget);
                break;
            case AIState.SearchWaiting:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (stateTimer >= searchWaitTime) ChangeState(AIState.Searching);
                break;
            case AIState.Searching:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                break;
            case AIState.SearchComplete:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (stateTimer >= searchEndWaitTime) ChangeState(AIState.Returning);
                break;
            case AIState.LostTarget:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (stateTimer >= lostTargetWaitTime) ChangeState(AIState.Returning);
                break;
            case AIState.Returning:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (Vector3.Distance(transform.position, startPos) <= 0.5f) ChangeState(AIState.Waiting);
                break;
            case AIState.Waiting:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (stateTimer >= returnedWaitTime) ChangeState(AIState.Patrolling);
                break;
            case AIState.DistractedByDecoy:
                // 플레이어가 감지되면 유인 해제하고 추격
                if (!hiding && inRange)
                {
                    currentDistraction = null;
                    ChangeState(AIState.Chasing);
                }
                break;
                // CaughtPlayer와 StunnedAfterQTE는 코루틴에서 상태 변경 처리
        }
    }
    void UpdatePatrolling()
    {
        if (isPatrolWaiting)
        {
            if (stateTimer >= patrolWaitTime)
            {
                isPatrolWaiting = false;
                SetNextPatrolTarget();
                stateTimer = 0f;
            }
            return;
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 0.3f)
        {
            StopMoving();
            isPatrolWaiting = true;
            stateTimer = 0f;
        }
        else
        {
            MoveToTarget(moveSpeed * 0.7f);
        }
    }

    void UpdateChasing()
    {
        if (Player != null)
        {
            SetTarget(Player.position);
            MoveToTarget(moveSpeed);
        }
    }

    void UpdateSearching()
    {
        if (isSearchWaiting)
        {
            if (stateTimer >= 1f)
            {
                isSearchWaiting = false;
                stateTimer = 0f;
                SetNextSearchTarget();
            }
            return;
        }

        if (Vector3.Distance(transform.position, targetPosition) <= 0.3f)
        {
            isSearchWaiting = true;
            stateTimer = 0f;
        }
        else
        {
            MoveToTarget(moveSpeed * 0.6f);
        }
    }

    void UpdateSearchComplete()
    {
        if (Vector3.Distance(transform.position, searchCenter) > 0.3f)
        {
            MoveToTarget(moveSpeed * 0.6f);
        }
    }

    void SetTarget(Vector3 target)
    {
        targetPosition = target;
        isMoving = true;
    }

    void MoveToTarget(float speed)
    {
        if (!isMoving) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    void StopMoving()
    {
        isMoving = false;
    }

    void SetNextPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        SetTarget(patrolPoints[currentPatrolIndex]);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void SetupPatrolPoints()
    {
        Vector3 leftPoint = startPos + Vector3.left * patrolDistance;
        Vector3 rightPoint = startPos + Vector3.right * patrolDistance;

        List<Vector3> validPoints = new List<Vector3>();
        if (!IsBlocked(leftPoint)) validPoints.Add(leftPoint);
        if (!IsBlocked(rightPoint)) validPoints.Add(rightPoint);

        if (validPoints.Count == 2)
            patrolPoints = new Vector3[] { validPoints[0], validPoints[1] };
        else if (validPoints.Count == 1)
            patrolPoints = new Vector3[] { startPos, validPoints[0] };
        else
            patrolPoints = new Vector3[] {
                startPos + Vector3.left * 0.5f,
                startPos + Vector3.right * 0.5f
            };
    }

    bool IsBlocked(Vector3 position)
    {
        Collider2D obstacle = Physics2D.OverlapCircle(position, 0.2f);
        return obstacle != null && obstacle.CompareTag("Ground");
    }

    void SetupSearchPattern()
    {
        searchCenter = transform.position;
        currentSearchLap = 0;
        searchingRight = true;
        SetNextSearchTarget();
    }

    void SetNextSearchTarget()
    {
        if (currentSearchLap >= searchLaps)
        {
            SetTarget(searchCenter);
            ChangeState(AIState.SearchComplete);
            return;
        }

        Vector3 nextTarget = searchingRight
            ? searchCenter + Vector3.right * searchDistance
            : searchCenter + Vector3.left * searchDistance;

        searchingRight = !searchingRight;
        if (!searchingRight) currentSearchLap++;

        SetTarget(nextTarget);
    }

    void FindCurrentHideArea()
    {
        GameObject[] hideAreas = GameObject.FindGameObjectsWithTag("HideArea");
        float closest = float.MaxValue;
        Transform closestArea = null;

        foreach (GameObject obj in hideAreas)
        {
            float dist = Vector3.Distance(Player.position, obj.transform.position);
            if (dist < closest)
            {
                closest = dist;
                closestArea = obj.transform;
            }
        }

        currentHideArea = closestArea;
    }

    // ================================
    // 공개 메서드들 (외부에서 호출 가능)
    // ================================

    // 현재 생명 수 반환
    public int GetCurrentLives()
    {
        return currentPlayerLives;
    }

    // 생명 리셋 (챕터 시작 시 호출)
    public void ResetLives()
    {
        currentPlayerLives = maxPlayerLives;
        Debug.Log("생명이 리셋되었습니다.");
    }
}