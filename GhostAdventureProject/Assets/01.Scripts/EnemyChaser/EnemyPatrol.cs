using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("순찰 설정")]
    public float patrolDistance = 3f;
    public float patrolWaitTime = 1f;

    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 startPos;
    private Vector3 currentPatrolCenter;  // 현재 순찰 중심점
    private bool isPatrolWaiting = false;
    private float waitTimer = 0f;

    private EnemyMovement movement;
    private EnemyAI enemyAI;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        startPos = transform.position;
        currentPatrolCenter = startPos;
        SetupPatrolPoints();
    }

    public void SetNewPatrolCenter(Vector3 newCenter, float radius)
    {
        currentPatrolCenter = newCenter;
        patrolDistance = radius;
        SetupPatrolPoints();
        currentPatrolIndex = 0;
        Debug.Log($"새로운 순찰 중심점 설정: {newCenter}, 반경: {radius}");
    }

    public void UpdatePatrolling()
    {
        if (isPatrolWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isPatrolWaiting = false;
                SetNextPatrolTarget();
                waitTimer = 0f;
            }
            return;
        }

        if (movement.HasReachedTarget())
        {
            movement.StopMoving();
            isPatrolWaiting = true;
            waitTimer = 0f;
        }
        else
        {
            movement.MoveToTarget(movement.patrolSpeed);
        }
    }

    public void SetNextPatrolTarget()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        movement.SetTarget(patrolPoints[currentPatrolIndex]);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void SetupPatrolPoints()
    {
        Vector3 left = startPos + Vector3.left * patrolDistance;
        Vector3 right = startPos + Vector3.right * patrolDistance;

        if (movement.lockYPosition)
        {
            left.y = movement.fixedYPosition;
            right.y = movement.fixedYPosition;
        }

        List<Vector3> valid = new();
        if (!IsBlocked(left)) valid.Add(left);
        if (!IsBlocked(right)) valid.Add(right);

        patrolPoints = valid.Count switch
        {
            2 => new[] { valid[0], valid[1] },
            1 => new[] { startPos, valid[0] },
            _ => new[] { startPos + Vector3.left * 0.5f, startPos + Vector3.right * 0.5f }
        };

        if (movement.lockYPosition)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 point = patrolPoints[i];
                point.y = movement.fixedYPosition;
                patrolPoints[i] = point;
            }
        }
    }

    private bool IsBlocked(Vector3 pos)
    {
        Collider2D col = Physics2D.OverlapCircle(pos, 0.2f);
        return col != null && col.CompareTag("Ground");
    }

    public Vector3 GetStartPosition() => startPos;
}