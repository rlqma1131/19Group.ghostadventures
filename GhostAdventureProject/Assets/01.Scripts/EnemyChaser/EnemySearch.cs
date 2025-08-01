using UnityEngine;

public class EnemySearch : MonoBehaviour
{
    [Header("수색 설정")]
    public float searchWaitTime = 2f;
    public float searchEndWaitTime = 2f;
    public float searchDistance = 1.5f;
    public int searchLaps = 2;

    private Vector3 searchCenter;
    private int currentLap = 0;
    private bool movingRight = true;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private EnemyMovement movement;
    private EnemyAI enemyAI;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        enemyAI = GetComponent<EnemyAI>();
    }
    
    public void SetupSearchPattern()
    {
        searchCenter = transform.position;
        currentLap = 0;
        movingRight = true;
        isWaiting = false;
        SetNextTarget();
    }
    
    public void UpdateSearching()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= searchWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                SetNextTarget();
            }
            return;
        }

        if (movement.HasReachedTarget())
        {
            isWaiting = true;
            waitTimer = 0f;
        }
        else
        {
            movement.MoveToTarget(movement.moveSpeed * 0.6f);
        }
    }
    
    public void UpdateSearchComplete()
    {
        if (Vector3.Distance(transform.position, searchCenter) > 0.3f)
        {
            movement.MoveToTarget(movement.moveSpeed * 0.6f);
        }
        else
        {
            enemyAI.ChangeState(EnemyAI.State.Patrolling);
        }
    }
    
    private void SetNextTarget()
    {
        if (currentLap >= searchLaps)
        {
            movement.SetTarget(searchCenter);
            return;
        }

        Vector3 nextPos = searchCenter + (movingRight ? Vector3.right : Vector3.left) * searchDistance;
        movingRight = !movingRight;

        if (!movingRight) currentLap++; // 왕복 1회로 계산
        movement.SetTarget(nextPos);
    }

    public float GetSearchWaitTime() => searchWaitTime;
    public float GetSearchEndWaitTime() => searchEndWaitTime;
}