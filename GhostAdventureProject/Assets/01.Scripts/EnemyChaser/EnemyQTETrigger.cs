using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyQTETrigger : MonoBehaviour
{
    private EnemyQTE parentQTE;

    private void Awake()
    {
        parentQTE = GetComponentInParent<EnemyQTE>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            parentQTE.TryStartQTE(other.transform);
        }
    }
}
