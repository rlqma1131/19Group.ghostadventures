using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyDoorInteraction))]
[RequireComponent(typeof(EnemySearch))]
[RequireComponent(typeof(EnemyQTE))]
public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Patrolling,
        Chasing,
        Searching,
        QTE,
        Stunned
    }

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 5f;
    [Range(0, 360)] public float viewAngle = 60f;

    [Header("Animation")]
    public Animator enemyAnimator;

    private State currentState;
    private EnemyMovement movement;
    private EnemyPatrol patrol;
    private EnemyDoorInteraction doorInteraction;
    private EnemySearch search;
    private EnemyQTE qteHandler;

    // 전역 감지
    private bool isAlerted = false;
    private float alertTimer = 0f;
    public float alertDuration = 5f;
    public float alertChaseSpeed = 5f;

    // 호환용 프로퍼티 (기존 스크립트 사용)
    public State CurrentState => currentState;
    public Transform Player => player;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        patrol = GetComponent<EnemyPatrol>();
        doorInteraction = GetComponent<EnemyDoorInteraction>();
        search = GetComponent<EnemySearch>();
        qteHandler = GetComponent<EnemyQTE>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (player == null && GameManager.Instance != null)
            player = GameManager.Instance.Player?.transform;

        qteHandler.SetPlayer(player);
        ChangeState(State.Patrolling);
    }

    private void Update()
    {
        HandleAlertMode();

        if (isAlerted) return;

        switch (currentState)
        {
            case State.Patrolling:
                patrol.UpdatePatrolling();
                DetectPlayer();
                break;

            case State.Chasing:
                ChasePlayer();
                break;

            case State.Searching:
                search.UpdateSearching();
                DetectPlayer();
                break;

            case State.QTE:
                break;

            case State.Stunned:
                break;
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        Vector2 dir = player.position - transform.position;
        float angle = Vector2.Angle(transform.right, dir);

        if (dir.magnitude <= detectionRange && angle <= viewAngle / 2f)
        {
            ChangeState(State.Chasing);
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            ChangeState(State.Patrolling);
            return;
        }

        movement.SetTarget(player.position);
        movement.MoveToTarget(movement.chaseSpeed);

        Vector2 dir = player.position - transform.position;
        float angle = Vector2.Angle(transform.right, dir);

        if (dir.magnitude > detectionRange * 1.5f || angle > viewAngle)
        {
            search.SetupSearchPattern();
            ChangeState(State.Searching);
        }
    }

    public void OnCaughtPlayer()
    {
        ChangeState(State.QTE);
        movement.StopMoving();
    }

    public void OnQTESuccess()
    {
        ChangeState(State.Stunned);
        StartCoroutine(RecoverFromStun());
    }

    public void OnQTEFail()
    {
        PlayerLifeManager.Instance.HandleGameOver();
    }

    private IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(2f);
        ChangeState(State.Patrolling);
    }

    public void GetDistractedBy(Transform soundSource)
    {
        isAlerted = true;
        alertTimer = alertDuration;
        ChangeState(State.Chasing);
        Debug.Log($"[EnemyAI] 사운드 감지 - {alertDuration}초 동안 전역 추격 모드!");
    }

    private void HandleAlertMode()
    {
        if (!isAlerted) return;

        alertTimer -= Time.deltaTime;

        if (player != null)
        {
            movement.SetTarget(player.position);
            movement.MoveToTarget(alertChaseSpeed);
        }

        if (alertTimer <= 0f)
        {
            isAlerted = false;
            ChangeState(State.Patrolling);
            Debug.Log("[EnemyAI] 전역 추격 모드 종료");
        }
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (enemyAnimator == null) return;

        enemyAnimator.SetBool("IsWalking",
            currentState == State.Patrolling ||
            currentState == State.Chasing);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 rightBoundary = Quaternion.Euler(0, 0, viewAngle / 2f) * transform.right;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, -viewAngle / 2f) * transform.right;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectionRange);
    }
}