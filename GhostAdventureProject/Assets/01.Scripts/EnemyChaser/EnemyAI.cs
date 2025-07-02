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

    [Header("유인되고 난 후")]
    public float distractionSpeed = 1f;
    public float distractionTimer = 0f;
    public float distractionDuration = 5f;

    [Header("QTE 설정")]
    public float catchRange = 1.5f;

    [Header("애니메이션 설정")]
    public Animator enemyAnimator;

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

    [Header("유인 감지 거리")]
    public float distractionRange = 15f;

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
    private Transform currentDistraction;

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
        CaughtPlayer,
        DistractedByDecoy,
        StunnedAfterQTE
    }

    private AIState currentState = AIState.Patrolling;

    private void Awake()
    {
        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        startPos = transform.position;
        if (Player == null) Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (Player != null) playerHide = Player.GetComponent<PlayerHide>();

        SetupPatrolPoints();
        ChangeState(AIState.Patrolling);
    }

    private void Update()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (Player != null)
                playerHide = Player.GetComponent<PlayerHide>();
            else
                return;
        }

        stateTimer += Time.deltaTime;

        if (currentState == AIState.Chasing &&
            Vector3.Distance(transform.position, Player.position) <= catchRange)
        {
            TryCatchPlayer();
            return;
        }

        UpdateCurrentState();
        CheckStateTransitions();
    }

    private void ChangeState(AIState newState)
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

    private void TryCatchPlayer()
    {
        StopMoving();
        ChangeState(AIState.CaughtPlayer);
        enemyAnimator?.SetTrigger("QTEIn");
        StartCoroutine(HandleQTEFlow());
    }

    private IEnumerator HandleQTEFlow()
    {
        QTEEffectManager.Instance.playerTarget = Player;
        QTEEffectManager.Instance.enemyTarget = this.transform;
        QTEEffectManager.Instance.StartQTEEffects();

        var qte2 = UIManager.Instance.QTE_UI_2;
        qte2.StartQTE();

        while (qte2.IsQTERunning())
            yield return null;

        if (qte2.IsSuccess())
            OnQTESuccess();
        else
            OnQTEFailure();
    }

    private void OnQTESuccess()
    {
        Debug.Log("QTE 성공! 플레이어가 탈출했습니다.");
        QTEEffectManager.Instance.EndQTEEffects();
        enemyAnimator?.SetTrigger("QTESuccess");

        // 외부 생명 시스템 호출
        PlayerLifeManager.Instance.LosePlayerLife();
    }

    private void OnQTEFailure()
    {
        Debug.Log("QTE 실패! 플레이어가 잡혔습니다.");
        QTEEffectManager.Instance.EndQTEEffects();
        enemyAnimator?.SetTrigger("QTEFail");

        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(1.5f);
        PlayerLifeManager.Instance.HandleGameOver();
    }

    public void OnSoundDetected(Vector3 soundPosition)
    {
        if (currentState == AIState.CaughtPlayer || currentState == AIState.StunnedAfterQTE)
            return;

        Debug.Log("소리를 감지했습니다! 해당 위치로 이동합니다.");
        currentDistraction = null;
        SetTarget(soundPosition);
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

    private IEnumerator StunAfterQTE()
    {
        ChangeState(AIState.StunnedAfterQTE);
        enemyAnimator?.SetBool("QTEFail", true);
        yield return new WaitForSeconds(2f);
        enemyAnimator?.SetBool("QTEFail", false);

        if (Player != null && Vector3.Distance(transform.position, Player.position) < detectionRange)
            ChangeState(AIState.Chasing);
        else
            ChangeState(AIState.Patrolling);
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrolling: UpdatePatrolling(); break;
            case AIState.Chasing: UpdateChasing(); break;
            case AIState.SearchWaiting:
            case AIState.LostTarget:
            case AIState.Waiting:
            case AIState.CaughtPlayer:
            case AIState.StunnedAfterQTE:
                StopMoving(); break;
            case AIState.Searching: UpdateSearching(); break;
            case AIState.SearchComplete: UpdateSearchComplete(); break;
            case AIState.Returning: MoveToTarget(moveSpeed * 0.9f); break;
            case AIState.DistractedByDecoy: UpdateDistractedState(); break;
        }

        enemyAnimator?.SetBool("IsMoving",
            currentState == AIState.Patrolling ||
            currentState == AIState.Chasing ||
            currentState == AIState.Searching ||
            currentState == AIState.SearchComplete ||
            currentState == AIState.Returning);
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
                if (hiding) { FindCurrentHideArea(); ChangeState(AIState.SearchWaiting); }
                else if (inRange) { ChangeState(AIState.Chasing); }
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
                else if (Vector3.Distance(transform.position, startPos) <= 0.5f)
                    ChangeState(AIState.Waiting);
                break;
            case AIState.Waiting:
                if (!hiding && inRange) ChangeState(AIState.Chasing);
                else if (stateTimer >= returnedWaitTime)
                    ChangeState(AIState.Patrolling);
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

    private void UpdatePatrolling()
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
        else MoveToTarget(moveSpeed * 0.7f);
    }

    private void UpdateChasing()
    {
        if (Player != null)
        {
            SetTarget(Player.position);
            MoveToTarget(moveSpeed);
            Ch1_HideAreaEvent.Instance.UnTagAllHideAreas(); // 퍼즐 방 은신처 무효화
        }
    }

    private void UpdateSearching()
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
        else MoveToTarget(moveSpeed * 0.6f);
    }

    private void UpdateSearchComplete()
    {
        if (Vector3.Distance(transform.position, searchCenter) > 0.3f)
            MoveToTarget(moveSpeed * 0.6f);
    }

    private void UpdateDistractedState()
    {
        distractionTimer += Time.deltaTime;
        if (currentDistraction != null)
        {
            SetTarget(currentDistraction.position);
            MoveToTarget(distractionSpeed);
        }

        if (distractionTimer >= distractionDuration)
            EndDistraction();
    }

    private void SetTarget(Vector3 target) => (targetPosition, isMoving) = (target, true);

    private void MoveToTarget(float speed)
    {
        if (!isMoving) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void StopMoving() => isMoving = false;

    private void SetNextPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        SetTarget(patrolPoints[currentPatrolIndex]);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void SetupPatrolPoints()
    {
        Vector3 left = startPos + Vector3.left * patrolDistance;
        Vector3 right = startPos + Vector3.right * patrolDistance;
        List<Vector3> valid = new();
        if (!IsBlocked(left)) valid.Add(left);
        if (!IsBlocked(right)) valid.Add(right);
        patrolPoints = valid.Count switch
        {
            2 => new[] { valid[0], valid[1] },
            1 => new[] { startPos, valid[0] },
            _ => new[] { startPos + Vector3.left * 0.5f, startPos + Vector3.right * 0.5f }
        };
    }

    private bool IsBlocked(Vector3 pos)
    {
        Collider2D col = Physics2D.OverlapCircle(pos, 0.2f);
        return col != null && col.CompareTag("Ground");
    }

    private void SetupSearchPattern()
    {
        searchCenter = transform.position;
        currentSearchLap = 0;
        searchingRight = true;
        SetNextSearchTarget();
    }

    private void SetNextSearchTarget()
    {
        if (currentSearchLap >= searchLaps)
        {
            SetTarget(searchCenter);
            ChangeState(AIState.SearchComplete);
            return;
        }

        Vector3 next = searchingRight
            ? searchCenter + Vector3.right * searchDistance
            : searchCenter + Vector3.left * searchDistance;

        searchingRight = !searchingRight;
        if (!searchingRight) currentSearchLap++;
        SetTarget(next);
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
}
