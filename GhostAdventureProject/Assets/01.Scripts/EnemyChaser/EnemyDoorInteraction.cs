using UnityEngine;
using System.Collections;

public class EnemyDoorInteraction : MonoBehaviour
{
    // [SerializeField] private float doorDetectionRange = 1.5f;
    // [SerializeField] private float doorUseCooldown = 2f;
    //
    // private EnemyPatrol patrol;
    // private EnemyAI enemyAI;
    // private Rigidbody2D rb;
    // private bool isUsingDoor = false;
    // private float lastDoorUseTime = 0f;
    // private float ignoreDoorTime = 2f;
    // private float lastIgnoreTime = 0f;
    //
    // private void Awake()
    // {
    //     patrol = GetComponent<EnemyPatrol>();
    //     enemyAI = GetComponent<EnemyAI>();
    //     rb = GetComponent<Rigidbody2D>();
    // }
    //
    // private void Update()
    // {
    //     if (enemyAI.CurrentState == EnemyAI.State.QTE) return;
    //     if (Time.time - lastDoorUseTime < doorUseCooldown) return;
    //     if (isUsingDoor) return;
    //
    //     // ✅ Patrol이 Waypoint에 거의 도달했을 때만 문 입출 확률 체크
    //     int currentWaypointIndex = patrol.GetCurrentWaypointIndex();
    //     Transform currentWaypoint = WaypointManager.waypoints[currentWaypointIndex];
    //     float distanceToWaypoint = Vector2.Distance(transform.position, currentWaypoint.position);
    //
    //     if (distanceToWaypoint > 0.3f) return; // Waypoint에 거의 도착했을 때만 문 로직 실행
    //
    //     Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, doorDetectionRange);
    //     foreach (Collider2D col in colliders)
    //     {
    //         BaseDoor door = col.GetComponent<BaseDoor>();
    //         if (door != null)
    //         {
    //             if (!patrol.IsInsideRoom && patrol.ShouldEnterRoom())
    //             {
    //                 StartCoroutine(UseDoor(door, true));
    //                 return;
    //             }
    //             else if (patrol.IsInsideRoom && patrol.ShouldExitRoom())
    //             {
    //                 StartCoroutine(UseDoor(door, false));
    //                 return;
    //             }
    //         }
    //     }
    // }
    //
    // private IEnumerator UseDoor(BaseDoor door, bool enteringRoom)
    // {
    //     if (enemyAI.CurrentState == EnemyAI.State.QTE) yield break;
    //
    //     isUsingDoor = true;
    //     lastDoorUseTime = Time.time;
    //
    //     rb.velocity = Vector2.zero;
    //     yield return new WaitForSeconds(0.3f);
    //
    //     Vector3 teleportPosition = (door.GetTargetDoor() != null)
    //         ? door.GetTargetDoor().position
    //         : (Vector3)door.GetTargetPos();
    //
    //     rb.position = teleportPosition;
    //     rb.velocity = Vector2.zero;
    //
    //     if (enteringRoom)
    //         patrol.EnterRoom();
    //     else
    //         patrol.ExitRoom();
    //
    //     yield return new WaitForSeconds(0.5f);
    //     isUsingDoor = false;
    // }
    //
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
    //     Gizmos.DrawWireSphere(transform.position, doorDetectionRange);
    // }
}