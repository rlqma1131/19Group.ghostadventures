using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTETrigger : MonoBehaviour
{
    private EnemyAI enemyAI;
    
    public bool PlayerInInteractionRange { get; private set; }

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInInteractionRange = true;
        // 원래 QTE 시작 로직
        if (enemyAI != null)
            enemyAI.ChangeState(enemyAI.QTEState);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerInInteractionRange = false;
    }
}
