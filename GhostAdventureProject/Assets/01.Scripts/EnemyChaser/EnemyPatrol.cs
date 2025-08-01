using UnityEngine;
using System.Collections.Generic;

public class EnemyPatrol : MonoBehaviour
{
    [Header("순찰 설정")]
    public float patrolRadius = 3f;
    public float patrolWaitTime = 1f;
    public float patrolSpeed = 2f;
    public float targetReachThreshold = 0.5f;

    [Header("문 탈출 설정")]
    public float doorEscapeTime = 7f;
    public float doorSearchRadius = 15f;
    public float doorCooldownTime = 30f;
    public int nearDoorCandidates = 3;

    private Vector3 patrolCenter;
    private Vector3 currentTarget;
    private float patrolTimer = 0f;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private GameObject targetDoor;
    private Dictionary<GameObject, float> usedDoors = new();

    private EnemyMovement movement;
    private EnemyAI enemyAI;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        patrolCenter = transform.position;
        SetNewTarget();
    }

    public void UpdatePatrolling()
    {
        if (enemyAI.CurrentState != EnemyAI.State.Patrolling) return;

        patrolTimer += Time.deltaTime;

        if (patrolTimer >= doorEscapeTime)
        {
            TryEscapeToDoor();
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isWaiting = false;
                SetNewTarget();
            }
            return;
        }

        if (movement.HasReachedTarget(targetReachThreshold))
        {
            movement.StopMoving();
            isWaiting = true;
            waitTimer = 0f;
        }
        else
        {
            movement.SetTarget(currentTarget);
            movement.MoveToTarget(patrolSpeed);
        }
    }

    private void SetNewTarget()
    {
        float randomX = patrolCenter.x + Random.Range(-patrolRadius, patrolRadius);
        currentTarget = new Vector3(randomX, transform.position.y, transform.position.z);
        movement.SetTarget(currentTarget);
    }

    private void TryEscapeToDoor()
    {
        GameObject nearestDoor = FindNearestDoor();
        if (nearestDoor == null)
        {
            patrolTimer = 0f;
            return;
        }

        targetDoor = nearestDoor;
        movement.SetTarget(targetDoor.transform.position);
        StartCoroutine(MoveToDoorAndTeleport());
    }

    private System.Collections.IEnumerator MoveToDoorAndTeleport()
    {
        while (!movement.HasReachedTarget(targetReachThreshold))
        {
            movement.MoveToTarget(patrolSpeed * 1.5f);
            yield return null;
        }

        BaseDoor doorScript = targetDoor.GetComponent<BaseDoor>();
        if (doorScript != null)
        {
            usedDoors[targetDoor] = Time.time;

            Vector3 teleportPosition = (doorScript.GetTargetDoor() != null)
                ? doorScript.GetTargetDoor().position
                : (Vector3)doorScript.GetTargetPos();

            transform.position = teleportPosition;
            patrolCenter = teleportPosition;
        }

        patrolTimer = 0f;
        SetNewTarget();
    }

    private GameObject FindNearestDoor()
    {
        GameObject[] allDoors = GameObject.FindGameObjectsWithTag("Door");
        List<GameObject> availableDoors = new();

        foreach (GameObject door in allDoors)
        {
            if (usedDoors.ContainsKey(door) && Time.time - usedDoors[door] < doorCooldownTime)
                continue;

            if (Vector3.Distance(transform.position, door.transform.position) <= doorSearchRadius)
                availableDoors.Add(door);
        }

        if (availableDoors.Count == 0) return null;

        availableDoors.Sort((a, b) =>
            Vector3.Distance(transform.position, a.transform.position)
            .CompareTo(Vector3.Distance(transform.position, b.transform.position)));

        int candidateCount = Mathf.Min(nearDoorCandidates, availableDoors.Count);
        return availableDoors[Random.Range(0, candidateCount)];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentTarget, 0.2f);
    }
}