using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public EnemyMovementController Movement { get; private set; }
    public EnemyDetection Detection { get; private set; }
    public EnemyQTEHandler QTEHandler { get; private set; }

    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public QTEState QTEState { get; private set; }

    private EnemyState currentState;
    private Vector3 startPosition;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Movement = GetComponent<EnemyMovementController>();
        Detection = GetComponent<EnemyDetection>();
        QTEHandler = GetComponent<EnemyQTEHandler>();

        IdleState = new IdleState(this);
        PatrolState = new PatrolState(this);
        ChaseState = new ChaseState(this);
        QTEState = new QTEState(this);

        startPosition = transform.position;
    }

    private void Start()
    {
        ChangeState(IdleState);
    }

    private void Update()
    {
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        if (QTEHandler.IsRunning()) return;
        
        currentState?.FixedUpdate();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public bool CurrentStateIsPatrol() => currentState == PatrolState;

    public Vector3 GetStartPosition() => startPosition;
}

// public class EnemyAI : MonoBehaviour
// {
//     public enum State { Idle, Patrol, Chase, QTE }
//     private State currentState;
//
//     [Header("Movement Settings")]
//     public float patrolSpeed = 4f;
//     public float chaseSpeed = 4.2f;
//     public float detectionRange = 4f;
//     public float detectionAngle = 60f;
//
//     private Rigidbody2D rb;
//     private Transform player;
//     private Animator animator;
//     private Vector2 moveDir;
//
//     [Header("QTE Settings")]
//     private QTEUI2 qteUI;
//     private QTEEffectManager qteEffect;
//     public float qteFreezeDuration = 3f;
//     private bool isQTERunning = false;
//     private bool hasEscapedOnce = false;
//
//     private Vector2 startPosition;
//
//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         startPosition = transform.position;
//     }
//
//     private void Start()
//     {
//         player = GameManager.Instance.Player.transform;
//
//         if (UIManager.Instance != null)
//             qteUI = UIManager.Instance.QTE_UI_2;
//
//         qteEffect = QTEEffectManager.Instance;
//         ChangeState(State.Idle);
//     }
//
//     private void FixedUpdate()
//     {
//         if (isQTERunning) return;
//         
//         switch (currentState)
//         {
//             case State.Patrol:
//                 rb.MovePosition(rb.position + moveDir * patrolSpeed * Time.fixedDeltaTime);
//                 break;
//             case State.Chase:
//                 rb.MovePosition(Vector2.MoveTowards(rb.position, player.position, chaseSpeed * Time.fixedDeltaTime));
//                 break;
//         }
//     }
//
//     private void Update()
//     {
//         switch (currentState)
//         {
//             case State.Patrol:
//                 UpdateFlip();
//                 if (CanSeePlayer()) ChangeState(State.Chase);
//                 break;
//
//             case State.Chase:
//                 UpdateFlip();
//                 if (!CanSeePlayer())
//                     ChangeState(State.Patrol);
//                 break;
//         }
//     }
//
//     private void ChangeState(State newState)
//     {
//         currentState = newState;
//
//         animator.SetBool("IsIdle", newState == State.Idle);
//         animator.SetBool("IsWalking", newState == State.Patrol || newState == State.Chase);
//
//         if (newState == State.Idle)
//         {
//             StartCoroutine(IdleWait());
//         }
//         else if (newState == State.Patrol)
//         {
//             if (moveDir == Vector2.zero)
//             {
//                 PickRandomDirection();
//             }
//         }
//     }
//
//     private IEnumerator IdleWait()
//     {
//         yield return new WaitForSeconds(Random.Range(1f, 3f));
//         ChangeState(State.Patrol);
//     }
//
//     private bool CanSeePlayer()
//     {
//         Vector2 dirToPlayer = player.position - transform.position;
//         float angle = (transform.localScale.x > 0)
//             ? Vector2.Angle(Vector2.right, dirToPlayer)
//             : Vector2.Angle(Vector2.left, dirToPlayer);
//
//         return dirToPlayer.magnitude <= detectionRange && angle <= detectionAngle / 2;
//     }
//
//     private void PickRandomDirection()
//     {
//         float angle = Random.Range(0f, 360f);
//         Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
//         
//         if (Mathf.Abs(dir.x) < 0.5f)
//         {
//             dir.x = Mathf.Sign(dir.x) * 0.5f;
//             dir = dir.normalized;
//         }
//
//         moveDir = dir;
//     }
//
//     private void UpdateFlip()
//     {
//         if (moveDir.x > 0.01f)
//             transform.localScale = new Vector3(1, 1, 1);
//         else if (moveDir.x < -0.01f)
//             transform.localScale = new Vector3(-1, 1, 1);
//     }
//     
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (currentState != State.Patrol) return;
//
//         BaseDoor door = other.GetComponent<BaseDoor>();
//         if (door != null)
//         {
//             if (Random.value < 0.5f)
//             {
//                 Transform targetDoor = door.GetTargetDoor();
//                 Vector3 targetPos = targetDoor != null
//                     ? targetDoor.position
//                     : (Vector3)door.GetTargetPos();
//
//                 transform.position = targetPos;  // 사신 순간이동
//                 ChangeState(State.Idle);
//                 StartCoroutine(ResumePatrolAfterDelay());
//             }
//         }
//     }
//
//     private IEnumerator ResumePatrolAfterDelay()
//     {
//         yield return new WaitForSeconds(1f);
//         ChangeState(State.Patrol);
//     }
//
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         if (collision.collider.GetComponent<EnemyVolumeTrigger>() != null)
//             return;
//
//         Vector2 normal = collision.contacts[0].normal;
//         moveDir = Vector2.Reflect(moveDir, normal).normalized;
//     }
//
//     // ================= QTE =================
//     public void StartQTEFromTrigger()
//     {
//         if (currentState == State.QTE || isQTERunning) return;
//
//         if (hasEscapedOnce)
//         {
//             animator.SetTrigger("QTEFail");
//             PlayerLifeManager.Instance.HandleGameOver();
//             ChangeState(State.Idle);
//             return;
//         }
//         
//         StartCoroutine(StartQTESequence());
//     }
//
//     private IEnumerator StartQTESequence()
//     {
//         isQTERunning = true;
//         ChangeState(State.QTE);
//         animator.SetTrigger("QTEIn");
//
//         PossessionSystem.Instance.CanMove = false;
//         rb.velocity = Vector2.zero;
//
//         if (qteEffect != null)
//         {
//             qteEffect.playerTarget = player;
//             qteEffect.enemyTarget = transform;
//             qteEffect.StartQTEEffects();
//         }
//
//         if (qteUI != null)
//         {
//             qteUI.gameObject.SetActive(true);
//             qteUI.StartQTE();
//         }
//
//         yield return new WaitForSeconds(qteFreezeDuration);
//
//         bool success = qteUI != null && qteUI.IsSuccess();
//
//         if (success)
//         {
//             animator.SetTrigger("QTESuccess");
//             hasEscapedOnce = true;
//             PossessionSystem.Instance.CanMove = true;
//             yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
//             transform.position = startPosition;
//             ChangeState(State.Patrol);
//         }
//         else
//         {
//             animator.SetTrigger("QTEFail");
//             PlayerLifeManager.Instance.HandleGameOver();
//             yield break;
//         }
//
//         qteEffect?.EndQTEEffects();
//         qteUI?.gameObject.SetActive(false);
//         isQTERunning = false;
//     }
//     
//     public void OnQTESuccessAnimationEnd()
//     {
//         transform.position = startPosition;
//         ChangeState(State.Patrol);
//     }
//     
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, detectionRange);
//
//         // 좌우 시야 각도 시각화
//         Vector3 rightDir = Quaternion.Euler(0, 0, detectionAngle / 2) * (transform.localScale.x > 0 ? Vector3.right : Vector3.left);
//         Vector3 leftDir = Quaternion.Euler(0, 0, -detectionAngle / 2) * (transform.localScale.x > 0 ? Vector3.right : Vector3.left);
//
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);
//         Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
//     }
// }