using System.Collections;
using UnityEngine;

public class EnemyQTE : MonoBehaviour
{
    [Header("QTE 설정")]
    public float catchRange = 1.5f; // 백업용으로 유지

    private EnemyAI enemyAI;
    private Transform player;
    private bool isQTERunning = false; // QTE 중복 실행 방지

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();

        // 콜라이더가 없으면 추가 (트리거용)
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // SphereCollider를 기본으로 추가
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = catchRange;
            sphereCol.isTrigger = true;
        }
        else
        {
            // 기존 콜라이더를 트리거로 설정
            col.isTrigger = true;
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    // 콜라이더 충돌 감지
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌하고, QTE가 실행 중이 아닐 때만 실행
        if (other.CompareTag("Player") && !isQTERunning)
        {
            // Enemy가 추적 상태일 때만 QTE 실행
            if (enemyAI.currentState == EnemyAI.AIState.Chasing)
            {
                StartQTE();
            }
        }
    }

    // 기존 방식도 유지 (필요시 사용)
    public bool CanCatchPlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= catchRange;
    }

    public void StartQTE()
    {
        if (isQTERunning) return; // 이미 실행 중이면 무시

        isQTERunning = true;
        enemyAI.enemyAnimator?.SetTrigger("QTEIn");
        enemyAI.ChangeState(EnemyAI.AIState.CaughtPlayer);
        GetComponent<EnemyMovement>().StopMoving();
        StartCoroutine(HandleQTEFlow());
    }

    private IEnumerator HandleQTEFlow()
    {
        QTEEffectManager.Instance.playerTarget = player;
        QTEEffectManager.Instance.enemyTarget = this.transform;
        QTEEffectManager.Instance.StartQTEEffects();

        var qte2 = UIManager.Instance.QTE_UI_2;
        qte2.StartQTE();

        while (qte2.IsQTERunning())
        {
            yield return null;
        }

        if (qte2.IsSuccess())
        {
            OnQTESuccess();
        }
        else
        {
            OnQTEFailure();
        }
    }

    private void OnQTESuccess()
    {
        Debug.Log("QTE 성공! 플레이어가 탈출했습니다.");
        QTEEffectManager.Instance.EndQTEEffects();

        enemyAI.ChangeState(EnemyAI.AIState.QTEAnimation);
        enemyAI.enemyAnimator?.SetTrigger("QTESuccess");

        PlayerLifeManager.Instance.LosePlayerLife();
        StartCoroutine(DelayedStunAfterQTE());
    }

    private void OnQTEFailure()
    {
        Debug.Log("QTE 실패! 플레이어가 잡혔습니다.");
        QTEEffectManager.Instance.EndQTEEffects();

        enemyAI.ChangeState(EnemyAI.AIState.QTEAnimation);
        enemyAI.enemyAnimator?.SetTrigger("QTEFail");

        StartCoroutine(DelayedGameOverAfterAnimation());
    }

    private IEnumerator DelayedStunAfterQTE()
    {
        yield return new WaitForSeconds(2f);

        enemyAI.ChangeState(EnemyAI.AIState.StunnedAfterQTE);
        enemyAI.enemyAnimator?.SetBool("IsIdle", true);

        yield return new WaitForSeconds(2f);

        enemyAI.enemyAnimator?.SetBool("IsIdle", false);

        if (player != null && Vector3.Distance(transform.position, player.position) < enemyAI.detectionRange)
            enemyAI.ChangeState(EnemyAI.AIState.Chasing);
        else
            enemyAI.ChangeState(EnemyAI.AIState.Patrolling);

        isQTERunning = false; // QTE 완료 후 플래그 리셋
    }

    private IEnumerator DelayedGameOverAfterAnimation()
    {
        yield return new WaitForSeconds(2f);
        PlayerLifeManager.Instance.HandleGameOver();
        isQTERunning = false; // QTE 완료 후 플래그 리셋
    }

    // 디버깅용 - 콜라이더 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRange);
    }
}