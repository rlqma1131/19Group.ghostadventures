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

        if (other.CompareTag("Player") && enemyAI.CurrentState == EnemyAI.State.Chasing)
        {
            player = other.transform;
            StartQTE();
        }
    }

    public void StartQTE()
    {
        if (isQTERunning) return;

        int currentLives = PlayerLifeManager.Instance.GetCurrentLives();

        if (currentLives <= 1)
        {
            Debug.Log("라이프 1 → QTE 없이 Enemy_QTEFail 애니메이션 강제 재생 후 게임오버");

            enemyAI.ChangeState(EnemyAI.State.QTE);
            var anim = enemyAI.enemyAnimator;

            anim.ResetTrigger("QTEIn");
            anim.ResetTrigger("QTESuccess");
            anim.ResetTrigger("QTEFail");

            anim.Play("Enemy_QTEFail", 0, 0f);
            GetComponent<EnemyMovement>().StopMoving();

            StartCoroutine(DelayedGameOverAfterAnimation());
            isQTERunning = true;
            return;
        }

        // 일반 QTE 시작
        isQTERunning = true;
        enemyAI.enemyAnimator?.SetTrigger("QTEIn");
        enemyAI.ChangeState(EnemyAI.State.QTE);

        GetComponent<EnemyMovement>().StopMoving();
        StartCoroutine(HandleQTEFlow());
        PossessionSystem.Instance.CanMove = false; // 플레이어 이동 비활성화
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

        int currentLives = PlayerLifeManager.Instance.GetCurrentLives();

        if (currentLives <= 1)
        {
            Debug.Log("QTE 성공이지만 남은 목숨이 1 → 즉시 게임오버 처리");
            PlayerLifeManager.Instance.HandleGameOver();
            isQTERunning = false;
            return;
        }

        // 정상 QTE 성공 처리
        enemyAI.ChangeState(EnemyAI.State.Stunned);
        enemyAI.enemyAnimator?.SetTrigger("QTESuccess");

        PlayerLifeManager.Instance.LosePlayerLife();
        StartCoroutine(RecoverAfterStun());

        PossessionSystem.Instance.CanMove = true;
    }

    private void OnQTEFailure()
    {
        Debug.Log("QTE 실패! 플레이어가 잡혔습니다.");
        QTEEffectManager.Instance.EndQTEEffects();

        enemyAI.ChangeState(EnemyAI.State.QTE);
        enemyAI.enemyAnimator?.SetTrigger("QTEFail");

        StartCoroutine(DelayedGameOverAfterAnimation());
    }

    private IEnumerator RecoverAfterStun()
    {
        yield return new WaitForSeconds(2f); // 짧은 스턴 후 복귀

        if (player != null && Vector2.Distance(transform.position, player.position) < enemyAI.detectionRange)
            enemyAI.ChangeState(EnemyAI.State.Chasing);
        else
            enemyAI.ChangeState(EnemyAI.State.Patrolling);

        isQTERunning = false;
    }

    private IEnumerator DelayedGameOverAfterAnimation()
    {
        yield return new WaitForSeconds(2f);
        PlayerLifeManager.Instance.HandleGameOver();
        isQTERunning = false;
    }
}