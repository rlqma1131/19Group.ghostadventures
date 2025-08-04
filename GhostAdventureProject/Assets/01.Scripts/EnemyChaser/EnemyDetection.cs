using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public float detectionRange = 4f;
    public float detectionAngle = 60f;
    private Transform player;

    private void Start()
    {
        player = GameManager.Instance.Player.transform;
    }

    public bool CanSeePlayer()
    {
        Vector2 dirToPlayer = player.position - transform.position;
         float angle = (transform.localScale.x > 0)
             ? Vector2.Angle(Vector2.right, dirToPlayer)
             : Vector2.Angle(Vector2.left, dirToPlayer);

         return dirToPlayer.magnitude <= detectionRange && angle <= detectionAngle / 2;
    }
    
    private void OnDrawGizmosSelected()
     {
         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(transform.position, detectionRange);

         // 좌우 시야 각도 시각화
         Vector3 rightDir = Quaternion.Euler(0, 0, detectionAngle / 2) * (transform.localScale.x > 0 ? Vector3.right : Vector3.left);
         Vector3 leftDir = Quaternion.Euler(0, 0, -detectionAngle / 2) * (transform.localScale.x > 0 ? Vector3.right : Vector3.left);

         Gizmos.color = Color.yellow;
         Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);
         Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
     }
}
