using System.Collections;
using UnityEngine;

public class EnemyQTE : MonoBehaviour
{
    [Header("QTE 설정")]
    private EnemyAI enemyAI;
    private Transform player;
    private bool isQTERunning = false;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();

        if (enemyAI == null)
            Debug.LogError("[EnemyQTE] EnemyAI 컴포넌트를 찾을 수 없습니다.", this);
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyAI == null || isQTERunning) return;

        if (other.CompareTag("Player") && enemyAI.CurrentState == EnemyAI.AIState.Chasing)
        {
            player = other.transform;
            StartQTE();
        }
    }

    public void StartQTE()
    {
        if (isQTERunning) return;

        isQTERunning = true;
        enemyAI.enemyAnimator?.SetTrigger("QTEIn");
        enemyAI.ChangeState(EnemyAI.AIState.CaughtPlayer);
        GetComponent<EnemyMovement>().StopMoving();
        StartCoroutine(HandleQTEFlow());
        PossessionSystem.Instance.canMove = false; // 플레이어 이동 비활성화
    }

    private IEnumerator HandleQTEFlow()
    {
        QTEEffectManager.Instance.playerTarget = player;
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

        enemyAI.ChangeState(EnemyAI.AIState.QTEAnimation);
        enemyAI.enemyAnimator?.SetTrigger("QTESuccess");

        PlayerLifeManager.Instance.LosePlayerLife();
        StartCoroutine(DelayedStunAfterQTE());

        PossessionSystem.Instance.canMove = true; // 플레이어 이동 활성화
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

        if (player != null && Vector2.Distance(transform.position, player.position) < enemyAI.detectionRange)
            enemyAI.ChangeState(EnemyAI.AIState.Chasing);
        else
            enemyAI.ChangeState(EnemyAI.AIState.Patrolling);

        isQTERunning = false;
    }

    private IEnumerator DelayedGameOverAfterAnimation()
    {
        yield return new WaitForSeconds(2f);
        PlayerLifeManager.Instance.HandleGameOver();
        isQTERunning = false;
    }
}