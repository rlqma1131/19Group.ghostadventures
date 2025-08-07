using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTEHandler : MonoBehaviour
{
    private QTEUI2 qteUI;
    private QTEEffectManager qteEffect;
    private Animator animator;
    private EnemyAI enemy;
    private Rigidbody2D rb;
    private Transform player;
    private Vector3 startPosition;
    
    public float qteFreezeDuration = 3f;
    private bool isQTERunning = false;
    // private bool hasEscapedOnce = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyAI>();
        startPosition = transform.position;
        
    }

    private void Start()
    {
        qteUI = UIManager.Instance.QTE_UI_2;
        qteEffect = QTEEffectManager.Instance;
        player = GameManager.Instance.Player.transform;
    }

    public void StartQTE()
    {
        // 목숨이 1개 남았을 때 잡히면 즉시 게임오버
        if (PlayerLifeManager.Instance.GetCurrentLives() <= 1)
        {
            animator.SetTrigger("QTEFail");
            PlayerLifeManager.Instance.HandleGameOver();
            return;
        }
        
        if (!isQTERunning)
            StartCoroutine(StartQTESequence());
    }

    private IEnumerator StartQTESequence()
    {
        isQTERunning = true;
        enemy.ChangeState(enemy.QTEState);
        animator.SetTrigger("QTEIn");

        PossessionSystem.Instance.CanMove = false;
        rb.velocity = Vector2.zero;

        if (qteEffect != null&& player != null)
        {
            qteEffect.playerTarget = player;
            qteEffect.enemyTarget = transform;
            qteEffect.StartQTEEffects();
        }

        if (qteUI != null)
        {
            qteUI.gameObject.SetActive(true);
            qteUI.StartQTE();
        }
    
        yield return new WaitForSeconds(qteFreezeDuration);

        bool success = qteUI != null && qteUI.IsSuccess();

        if (success)
        {
            animator.SetTrigger("QTESuccess");
            PlayerLifeManager.Instance.LosePlayerLife();
            PossessionSystem.Instance.CanMove = true;

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            transform.position = startPosition;
            enemy.ChangeState(enemy.PatrolState);
        }
        else
        {
            animator.SetTrigger("QTEFail");
            PlayerLifeManager.Instance.HandleGameOver();
            yield break;
        }

        qteEffect?.EndQTEEffects();
        qteUI?.gameObject.SetActive(false);
        isQTERunning = false;
    }

    public void OnQTESuccessAnimationEnd()
    {
        transform.position = startPosition;
        enemy.ChangeState(enemy.PatrolState);
    }
    
    public bool IsRunning()
    {
        return isQTERunning;
    }
}
