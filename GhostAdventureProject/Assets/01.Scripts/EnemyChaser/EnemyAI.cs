using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(EnemyDoorInteraction))]
[RequireComponent(typeof(EnemyQTE))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrolling, Chasing, QTE }

    [Header("감지 설정")]
    public float detectionRange = 4f;
    [Range(0, 360)] public float viewAngle = 60f;

    private Transform player;
    private EnemyMovement movement;
    private State currentState;
    private Animator animator;
    public State CurrentState { get; set; }
    private Vector3 originalPosition;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        animator = GetComponent<Animator>(); // 애니메이터 추가
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player.transform;
        }
        
        originalPosition = transform.position;
        ChangeState(State.Patrolling);
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                PatrolBehavior();
                DetectPlayer();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
            case State.QTE:
                movement.Stop();
                break;
        }
    }

    private void PatrolBehavior()
    {
        // EnemyPatrol에서 제어 예정
        animator.SetBool("IsWalking", true);
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        Vector2 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;
        Vector2 facingDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facingDir, dirToPlayer);

        if (distance <= detectionRange && angle <= viewAngle / 2f)
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

        Vector2 dir = (player.position - transform.position).normalized;
        movement.Move(dir, true);
        movement.FlipSprite(player.position);
        animator.SetBool("IsWalking", true);

        Vector2 facingDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(facingDir, player.position - transform.position);

        if (Vector2.Distance(transform.position, player.position) > detectionRange * 1.5f || angle > viewAngle)
        {
            ChangeState(State.Patrolling);
        }
    }

    public void OnCaughtPlayer()
    {
        ChangeState(State.QTE);
        movement.Stop();
        animator.SetBool("IsWalking", false);
    }

    public void OnQTESuccess()
    {
        ChangeState(State.Patrolling);
    }

    public void OnQTEFail()
    {
        PlayerLifeManager.Instance.HandleGameOver();
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
        animator.SetBool("IsWalking", newState != State.QTE);
    }
    
    public void TriggerTeleportChase(Transform playerTransform)
    {
        if (currentState == State.QTE) return;

        transform.position = playerTransform.position + (Vector3)(Random.insideUnitCircle.normalized * 1.5f); 
        ChangeState(State.Chasing);
        StartCoroutine(ChaseAndReturn(playerTransform, 5f));
    }

    private IEnumerator ChaseAndReturn(Transform playerTransform, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            movement.Move(dir, true);
            movement.FlipSprite(playerTransform.position);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 돌아감
        transform.position = originalPosition;
        ChangeState(State.Patrolling);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftDir = Quaternion.Euler(0, 0, viewAngle / 2) * transform.right;
        Vector3 rightDir = Quaternion.Euler(0, 0, -viewAngle / 2) * transform.right;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);
    }
}