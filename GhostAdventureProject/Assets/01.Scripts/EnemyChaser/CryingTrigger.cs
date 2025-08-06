using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryingTrigger : MonoBehaviour
{
    private EnemyAI[] enemies;

    private void OnEnable()
    {
        enemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in enemies)
        {
            enemy.StartInvestigate(transform);
        }
    }

    private void OnDisable()
    {
        foreach (var enemy in enemies)
        {
            enemy.StopInvestigate();
        }
    }
}
