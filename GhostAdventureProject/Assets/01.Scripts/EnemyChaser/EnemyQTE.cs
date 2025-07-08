using System.Collections;
using UnityEngine;

public class EnemyQTE : MonoBehaviour
{
    [Header("QTE 설정")]
    public float catchRange = 1.5f;

    private EnemyAI enemyAI;
    private Transform player;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    public bool CanCatchPlayer()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= catchRange;
    }

    public void StartQTE()
    {
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
    }

    private IEnumerator DelayedGameOverAfterAnimation()
    {
        yield return new WaitForSeconds(2f);
        PlayerLifeManager.Instance.HandleGameOver();
    }
}