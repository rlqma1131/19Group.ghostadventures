using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 4f;
    public float detectionAngle = 60f;
    private Transform enemyTransform;

    private void Awake()
    {
        enemyTransform = transform.root;
    }

    public bool CanSeePlayer()
    {
        if (GameManager.Instance.Player == null) return false;

        Vector2 directionToPlayer = (GameManager.Instance.Player.transform.position - enemyTransform.position).normalized;
        float distance = Vector2.Distance(enemyTransform.position, GameManager.Instance.Player.transform.position);

        if (distance > detectionRange) return false;

        float angle = Vector2.Angle(enemyTransform.right * enemyTransform.localScale.x, directionToPlayer);

        return angle <= detectionAngle * 0.5f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}
