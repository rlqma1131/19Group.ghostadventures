using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float patrolSpeed = 4f;
    public float chaseSpeed = 4.2f;
    private Vector2 moveDir;
    private Transform player;
    private EnemyAI enemy;
    
    public float directionChangeInterval = 5f;
    private float directionChangeTimer = 0f;
    private float startTime;
    public float doorBlockDuration = 20f;
    
    private Vector2? forcedTarget = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player.transform;
        PickRandomDirection();
        startTime = Time.time;
    }

    public void PickRandomDirection()
    {
        float currentAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float newAngle = Random.Range(currentAngle - 60f, currentAngle + 60f);

        Vector2 dir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

        if (Mathf.Abs(dir.x) < 0.3f) dir.x = Mathf.Sign(dir.x) * 0.3f;
        if (Mathf.Abs(dir.y) < 0.3f) dir.y = Mathf.Sign(dir.y) * 0.3f;

        moveDir = dir.normalized;
        directionChangeTimer = directionChangeInterval;
    }

    private void Update()
    {
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            PickRandomDirection();
        }
    }

    public void PatrolMove()
    {
        rb.MovePosition(rb.position + moveDir * patrolSpeed * Time.fixedDeltaTime);
        UpdateFlip();
    }

    public void ChasePlayer()
    {
        if (player != null)
        {
            Vector2 targetPos = forcedTarget ?? player.position;
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
            moveDir = dir;
            UpdateFlip();
        }
    }
    
    public void SetTargetPosition(Vector2 target)
    {
        forcedTarget = target;
    }

    public void ClearTargetPosition()
    {
        forcedTarget = null;
    }

    private void UpdateFlip()
    {
        if (moveDir.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveDir.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<EnemyVolumeTrigger>() != null)
            return;

        Vector2 normal = collision.contacts[0].normal;
        moveDir = Vector2.Reflect(moveDir, normal).normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemy.CurrentStateIsPatrol())
        {
            BaseDoor door = other.GetComponent<BaseDoor>();
            if (door != null)
            {
                if (Time.time - startTime < doorBlockDuration)
                {
                    PickRandomDirection();
                    return;
                }

                if (Random.value < 0.4f)
                {
                    Transform targetDoor = door.GetTargetDoor();
                    Vector3 targetPos = targetDoor != null
                        ? targetDoor.position
                        : (Vector3)door.GetTargetPos();

                    transform.position = targetPos;
                    enemy.ChangeState(enemy.IdleState);
                    enemy.StartCoroutine(ResumePatrolAfterDelay());
                    return;
                }
                PickRandomDirection();
            }
        }
    }

    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        enemy.ChangeState(enemy.PatrolState);
    }
}
