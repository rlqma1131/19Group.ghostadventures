using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTETrigger : MonoBehaviour
{
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemyAI != null)
            {
                enemyAI.ChangeState(enemyAI.QTEState);
            }
        }
    }
}
