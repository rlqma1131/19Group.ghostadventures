using UnityEngine;

public class EnemySearch : MonoBehaviour
{
    [Header("수색 설정")]
    public float searchWaitTime = 2f;
    public float searchEndWaitTime = 2f;
    public float searchDistance = 1.5f;
    public int searchLaps = 2;

    private Vector3 searchCenter;
    private int currentSearchLap = 0;
    private bool searchingRight = true;
    private bool isSearchWaiting = false;
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

        if (movement.lockYPosition)
        {
            Vector3 center = searchCenter;
            center.y = movement.fixedYPosition;
            searchCenter = center;
        }

        currentSearchLap = 0;
        searchingRight = true;
        isSearchWaiting = false;
        SetNextSearchTarget();
    }

    public void UpdateSearching()
    {
        if (isSearchWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= 1f)
            {
                isSearchWaiting = false;
                waitTimer = 0f;
                SetNextSearchTarget();
            }
            return;
        }

        if (movement.HasReachedTarget())
        {
            isSearchWaiting = true;
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
            movement.MoveToTarget(movement.moveSpeed * 0.6f);
    }

    private void SetNextSearchTarget()
    {
        if (currentSearchLap >= searchLaps)
        {
            movement.SetTarget(searchCenter);
            enemyAI.ChangeState(EnemyAI.AIState.SearchComplete);
            return;
        }

        Vector3 next = searchingRight
            ? searchCenter + Vector3.right * searchDistance
            : searchCenter + Vector3.left * searchDistance;

        searchingRight = !searchingRight;
        if (!searchingRight) currentSearchLap++;
        movement.SetTarget(next);
    }

    public float GetSearchWaitTime() => searchWaitTime;
    public float GetSearchEndWaitTime() => searchEndWaitTime;
}