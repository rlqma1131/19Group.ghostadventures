using System.Collections;
using UnityEngine;

public class EnemyQTE : MonoBehaviour
{
    private EnemyAI enemyAI;
    private Transform player;
    private bool isQTERunning = false;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // InteractionCollider(Trigger)에서만 감지
        if (isQTERunning) return;

        if (other.CompareTag("Player") && enemyAI.CurrentState == EnemyAI.State.Chasing)
        {
            player = other.transform;
            StartQTE();
        }
    }

    private void StartQTE()
    {
        int currentLives = PlayerLifeManager.Instance.GetCurrentLives();
        enemyAI.ChangeState(EnemyAI.State.QTE);

        if (currentLives <= 1)
        {
            GetComponent<Animator>().Play("Enemy_QTEFail", 0, 0f);
            StartCoroutine(DelayedGameOver());
            isQTERunning = true;
            return;
        }

        isQTERunning = true;
        GetComponent<Animator>().SetTrigger("QTEIn");
        StartCoroutine(HandleQTEFlow());
        PossessionSystem.Instance.CanMove = false;
    }

    private IEnumerator HandleQTEFlow()
    {
        QTEEffectManager.Instance.playerTarget = player;
        QTEEffectManager.Instance.enemyTarget = transform;
        QTEEffectManager.Instance.StartQTEEffects();

        var qteUI = UIManager.Instance.QTE_UI_2;
        qteUI.StartQTE();

        while (qteUI.IsQTERunning())
            yield return null;

        if (qteUI.IsSuccess())
            OnQTESuccess();
        else
            OnQTEFailure();
    }

    private void OnQTESuccess()
    {
        QTEEffectManager.Instance.EndQTEEffects();
        GetComponent<Animator>().SetTrigger("QTESuccess");
        PlayerLifeManager.Instance.LosePlayerLife();
        StartCoroutine(RecoverAfterQTE());
        PossessionSystem.Instance.CanMove = true;
    }

    private void OnQTEFailure()
    {
        QTEEffectManager.Instance.EndQTEEffects();
        GetComponent<Animator>().SetTrigger("QTEFail");
        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator RecoverAfterQTE()
    {
        yield return new WaitForSeconds(2f);
        enemyAI.OnQTESuccess();
        isQTERunning = false;
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(2f);
        PlayerLifeManager.Instance.HandleGameOver();
        isQTERunning = false;
    }
    
    public void TryStartQTE(Transform playerTransform)
    {
        if (isQTERunning) return;

        player = playerTransform;
        GetComponent<EnemyMovement>().Stop();
        StartQTE();
    }
}