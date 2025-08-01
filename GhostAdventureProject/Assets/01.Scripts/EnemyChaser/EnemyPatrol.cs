using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    public float minRoomStayTime = 5f;
    public float doorCooldown = 5f;

    [Range(0f, 1f)] public float enterDoorChance = 0.3f;
    [Range(0f, 1f)] public float exitDoorChance = 0.6f;

    private EnemyMovement movement;
    private EnemyAI enemyAI;
    private int currentWaypointIndex = 0;
    private int savedWaypointIndex;

    private bool insideRoom = false;
    private float roomStayTimer = 0f;
    private float lastDoorUseTime = 0f;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if (enemyAI.CurrentState == EnemyAI.State.QTE)
        {
            movement.Stop();
            return;
        }

        if (insideRoom)
        {
            HandleRoomStay();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (WaypointManager.waypoints == null || WaypointManager.waypoints.Length == 0) return;

        Transform targetWaypoint = WaypointManager.waypoints[currentWaypointIndex];
        Vector2 dir = (targetWaypoint.position - transform.position).normalized;

        movement.Move(dir, false);
        movement.FlipSprite(targetWaypoint.position);

        // Waypoint 도착 시
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            // ✅ 문 입출 여부를 여기서만 판단
            if (!insideRoom && Time.time - lastDoorUseTime > doorCooldown && Random.value <= enterDoorChance)
            {
                BaseDoor door = FindNearbyDoor();
                if (door != null)
                {
                    EnterRoom();
                    TeleportToRoom(door);
                    return;
                }
            }

            currentWaypointIndex = (currentWaypointIndex + 1) % WaypointManager.waypoints.Length;
        }
    }

    private void HandleRoomStay()
    {
        roomStayTimer += Time.deltaTime;

        if (roomStayTimer >= minRoomStayTime)
        {
            // 방에서 나올 확률
            if (Random.value <= exitDoorChance)
            {
                BaseDoor door = FindNearbyDoor();
                if (door != null)
                {
                    ExitRoom();
                    TeleportToHallway(door);
                    return;
                }
            }
            else
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                movement.Move(randomDir * 0.3f, false);
            }
        }
    }

    public void EnterRoom()
    {
        insideRoom = true;
        roomStayTimer = 0f;
        savedWaypointIndex = currentWaypointIndex;
        lastDoorUseTime = Time.time;
    }

    public void ExitRoom()
    {
        insideRoom = false;
        roomStayTimer = 0f;
        currentWaypointIndex = savedWaypointIndex; // 복귀 후 기존 목표 이어감
        lastDoorUseTime = Time.time;
    }

    private BaseDoor FindNearbyDoor()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var col in colliders)
        {
            BaseDoor door = col.GetComponent<BaseDoor>();
            if (door != null)
                return door;
        }
        return null;
    }

    private void TeleportToRoom(BaseDoor door)
    {
        transform.position = (door.GetTargetDoor() != null)
            ? door.GetTargetDoor().position
            : (Vector3)door.GetTargetPos();
    }

    private void TeleportToHallway(BaseDoor door)
    {
        transform.position = (door.GetTargetDoor() != null)
            ? door.GetTargetDoor().position
            : (Vector3)door.GetTargetPos();
    }

    public bool IsInsideRoom => insideRoom;
}