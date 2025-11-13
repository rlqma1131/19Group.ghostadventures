using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTETrigger : MonoBehaviour
{
    private EnemyAI enemyAI;
    public event Action<BaseDoor> OnDoorEntered;
    
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
                if (other.TryGetComponent(out PlayerCondition comp)) {
                    if (comp.IsInvincible()) enemyAI.ChangeState(enemyAI.PatrolState);
                    else enemyAI.ChangeState(enemyAI.QTEState);
                }
                
            }
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Door"))
        {
            BaseDoor door = other.GetComponent<BaseDoor>();
            if (door != null)
                OnDoorEntered?.Invoke(door);
        }
    }
}
