using UnityEngine;
using System.Collections;

public class EnemyDoorInteraction : MonoBehaviour
{
    [Header("문 상호작용 설정")]
    [SerializeField] private float doorDetectionRange = 1.5f;
    [SerializeField] private float doorUseCooldown = 2f;

    private EnemyAI enemyAI;
    private EnemyMovement movement;
    private bool isUsingDoor = false;
    private float lastDoorUseTime = 0f;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (Time.time - lastDoorUseTime < doorUseCooldown) return;
        if (isUsingDoor) return;

        if (enemyAI.CurrentState == EnemyAI.State.Chasing)
        {
            CheckForNearbyDoors();
        }
    }

    private void CheckForNearbyDoors()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, doorDetectionRange);

        foreach (Collider2D col in nearbyColliders)
        {
            BaseDoor door = col.GetComponent<BaseDoor>();
            if (door != null)
            {
                StartCoroutine(UseDoor(door));
                return;
            }
        }
    }

    private IEnumerator UseDoor(BaseDoor door)
    {
        isUsingDoor = true;
        lastDoorUseTime = Time.time;

        movement.StopMoving();
        yield return new WaitForSeconds(0.3f);

        Vector3 teleportPosition = (door.GetTargetDoor() != null)
            ? door.GetTargetDoor().position
            : (Vector3)door.GetTargetPos();

        transform.position = teleportPosition;

        yield return new WaitForSeconds(0.5f);

        if (enemyAI.CurrentState == EnemyAI.State.Chasing && enemyAI.Player != null)
        {
            movement.SetTarget(enemyAI.Player.position);
        }

        isUsingDoor = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, doorDetectionRange);
    }
}