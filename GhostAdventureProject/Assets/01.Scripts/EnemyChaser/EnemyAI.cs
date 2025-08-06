using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

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
    public InvestigateState InvestigateState { get; private set; }

    private protected EnemyState currentState;
    public Vector3 startPosition;
    public static bool IsAnyQTERunning = false;

    private Coroutine soundChaseCoroutine;
    public bool isTeleporting { get; private protected set; }
    
    [SerializeField] private float normalDetectionRange = 4f;
    [SerializeField] private float soundDetectionRange = 8f;

    [SerializeField] private float normalDetectionAngle = 60f;
    [SerializeField] private float soundDetectionAngle = 360f;
    
    public static bool IsPaused { get; private set; } = false;
    public static void PauseAllEnemies()  => IsPaused = true;
    public static void ResumeAllEnemies() => IsPaused = false;
    
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
        InvestigateState = new InvestigateState(this);

        startPosition = transform.position;
        
        if (Detection != null)
        {
            Detection.detectionRange = normalDetectionRange;
            Detection.detectionAngle = normalDetectionAngle;
        }
    }

    protected virtual void Start()
    {
        ChangeState(IdleState);
    }

    protected virtual void Update()
    {
        if (IsPaused) return;
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        if (IsPaused) return;
        if (IsAnyQTERunning || (QTEHandler != null && QTEHandler.IsRunning())) return;
        
        currentState?.FixedUpdate();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public bool CurrentStateIsPatrol() => currentState == PatrolState;

    public void StartSoundTeleport(Vector3 playerPos, float offsetDistance, float chaseDuration)
    {
        if (soundChaseCoroutine != null)
            StopCoroutine(soundChaseCoroutine);
        soundChaseCoroutine = StartCoroutine(SoundTeleportRoutine(playerPos, offsetDistance, chaseDuration));
    }

    private IEnumerator SoundTeleportRoutine(Vector3 playerPos, float offsetDistance, float chaseDuration)
    {
        isTeleporting = true;
        
        float originalRange = Detection.detectionRange;
        float originalAngle = Detection.detectionAngle;

        Detection.detectionRange = soundDetectionRange;
        Detection.detectionAngle = soundDetectionAngle;

        float facing = GameManager.Instance.Player.transform.localScale.x;
        Vector3 targetPos = playerPos + new Vector3(facing * offsetDistance, 2f, 0);
        transform.position = targetPos;

        Animator.SetTrigger("SoundTeleport");
        
        float animLength = Animator.runtimeAnimatorController.animationClips
                                   .FirstOrDefault(c => c.name == "Enemy_SoundTeleport")?.length ?? 1f;

        yield return new WaitForSeconds(animLength);

        Movement.MarkTeleported();
        isTeleporting = false;
        ChangeState(ChaseState);

        yield return new WaitForSeconds(chaseDuration);

        isTeleporting = true;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        yield return sr.DOFade(0f, 1f).WaitForCompletion();
        transform.position = startPosition;
        yield return sr.DOFade(1f, 0.5f).WaitForCompletion();
        
        Detection.detectionRange = originalRange;
        Detection.detectionAngle = originalAngle;

        isTeleporting = false;
        ChangeState(PatrolState);
    }
    
    /// <summary>오브젝트 활성화 시 호출</summary>
    public void StartInvestigate(Transform target)
    {
        transform.position = target.position;

        // InvestigateState에 목표 위치 전달 (필요 시)
        InvestigateState.Setup(target.position);

        // 상태 변경
        ChangeState(InvestigateState);
    }

    /// <summary>오브젝트 비활성화 시 호출</summary>
    public void StopInvestigate()
    {
        // 맵 경계나 텔레포트 막힌 상태에서 Patrol로 복귀
        ChangeState(PatrolState);
        // (필요하다면) 원위치 복귀 코루틴 호출
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart()
    {
        transform.position = startPosition;

        // 약간의 대기(선택사항) 후 Patrol 상태로 복귀
        yield return null;

        ChangeState(PatrolState);
    }
    
    // public void StartSoundTeleport(Vector3 playerPos, float offsetDistance, float chaseDuration)
    // {
    //     float facing = GameManager.Instance.Player.transform.localScale.x;
    //     Vector3 targetPos = playerPos + new Vector3(facing * offsetDistance, 0, 0);
    //
    //     SoundTeleportState = new SoundTeleportState(this, targetPos, chaseDuration);
    //     ChangeState(SoundTeleportState);
    // }
    //
    // public void OnSoundTeleportAnimationEnd()
    // {
    //     if (currentState is SoundTeleportState teleportState)
    //     {
    //         teleportState.OnTeleportAnimationEnd();
    //     }
    // }

    // private IEnumerator SoundTeleportRoutine(Vector3 playerPos, float offsetDistance)
    // {
    //     
    //     // 위치 순간이동
    //     float facing = GameManager.Instance.Player.transform.localScale.x;
    //     Vector3 targetPos = playerPos + new Vector3(facing * offsetDistance, 0, 0);
    //     transform.position = targetPos;
    //     // 텔레포트 애니메이션 재생
    //     Animator.SetTrigger("SoundTeleport");
    //
    //     // 애니메이션 이벤트가 끝날 때까지 대기
    //     // while (isSoundTeleporting)
    //     //     yield return null;
    //
    //
    //     // 추격 모드로 전환
    //     ChangeState(ChaseState);
    //
    //     // 일정 시간 후 Patrol 복귀
    //     yield return new WaitForSeconds(soundChaseDuration);
    //     
    // }
    //
    // public void OnTeleportAnimationEnd()
    // {
    //     // 애니메이션 이벤트에서 호출
    //     //isSoundTeleporting = false;
    //     ChangeState(PatrolState);
    // }
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