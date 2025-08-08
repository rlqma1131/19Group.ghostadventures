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
    
    private float teleportDoorIgnoreTime = 1f;   // 텔레포트 후 문 무시 시간
    private float lastTeleportTime = -10f;       // 초기값 멀리 설정
    
    private Vector2? forcedTarget = null;
    public LayerMask obstacleMask;
    public LayerMask doorLayerMask; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        player = GameManager.Instance.Player.transform;
        // PickRandomDirection();
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
    
    public void SetForcedTarget(Vector2? target)
    {
        forcedTarget = target;
    }

    public void PatrolMove()
    {
        if (enemy.isTeleporting) return;
        Vector2 delta = moveDir * patrolSpeed * Time.fixedDeltaTime;
        Vector2 pos   = rb.position;
        Vector2 newPos = pos;

        // 1) 수직(Y) 이동 검사
        if (Mathf.Abs(delta.y) > 0f)
        {
            // 위 또는 아래 방향에 충돌체가 있는지 Raycast
            Vector2 dirY = Vector2.up * Mathf.Sign(delta.y);
            float distY  = Mathf.Abs(delta.y);
            if (Physics2D.Raycast(pos, dirY, distY, obstacleMask) == false)
                newPos.y += delta.y;
            else
                moveDir.y = 0;   // 막혔으면 Y 성분 제거
        }

        // 2) 수평(X) 이동 검사
        if (Mathf.Abs(delta.x) > 0f)
        {
            Vector2 dirX = Vector2.right * Mathf.Sign(delta.x);
            float distX  = Mathf.Abs(delta.x);
            if (Physics2D.Raycast(pos, dirX, distX, obstacleMask) == false)
                newPos.x += delta.x;
            else
                moveDir.x = 0;   // 막혔으면 X 성분 제거
        }

        rb.MovePosition(newPos);
        UpdateFlip();
    }

    public void ChasePlayer()
    {
        if (enemy.isTeleporting) return;
        if (player != null)
        {
            Vector2 targetPos = forcedTarget ?? player.position;
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
            moveDir = dir;
            UpdateFlip();
        }
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
        if (collision.collider.GetComponentInChildren<EnemyVolumeTrigger>() != null)
            return;

        Vector2 normal = collision.contacts[0].normal;
        moveDir = Vector2.Reflect(moveDir, normal).normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var interaction = GetComponentInChildren<EnemyQTETrigger>();
        if (interaction == null || !interaction.PlayerInInteractionRange)
            return;
        
        if (Time.time - lastTeleportTime < teleportDoorIgnoreTime) return;
        
        // 2) 스폰(혹은 상태 전환) 직후에 문 트리거 안 받도록
        if (Time.time - startTime < doorBlockDuration)
        {
            PickRandomDirection();
            return;
        }
        
        // 3) 오직 문 레이어에만 반응
        if ((doorLayerMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        // 4) 순찰 상태가 아닐 땐 무시
        if (!enemy.CurrentStateIsPatrol())
            return;

        // 5) BaseDoor 컴포넌트가 없으면 무시
        BaseDoor door = other.GetComponent<BaseDoor>();
        if (door == null)
            return;

        // 6) 랜덤 확률로 순간이동 or 방향 전환
        if (Random.value < 0.4f)
        {
            // 6-a) 도착 지점 장애물 체크 (OverlapCircle)
            Vector3 dest = door.GetTargetDoor()?.position
                           ?? (Vector3)door.GetTargetPos();
            bool blocked = Physics2D.OverlapCircle(dest, 0.2f, obstacleMask);
            if (blocked)
            {
                PickRandomDirection();
                return;
            }

            // 6-b) 안전 확인 됐으면 순간이동
            TeleportThroughDoor(door);
        }
        else
        {
            PickRandomDirection();
        }
    }
    
    private IEnumerator RestoreCollision(int pLayer, int eLayer) {
        yield return new WaitForSecondsRealtime(teleportDoorIgnoreTime);
        Physics2D.IgnoreLayerCollision(pLayer, eLayer, false);
    }

    private void TeleportThroughDoor(BaseDoor door) {
        // 1) 충돌 무시 시작
        int pLayer = GameManager.Instance.Player.layer;
        int eLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(pLayer, eLayer, true);

        // 2) 순간이동
        Transform td = door.GetTargetDoor();
        Vector3 dest = td != null ? td.position : (Vector3)door.GetTargetPos();
        transform.position = dest;

        // 3) 타이밍 재설정
        lastTeleportTime = Time.time;

        // 4) 1초 뒤 충돌 복원
        StartCoroutine(RestoreCollision(pLayer, eLayer));

        // 5) 상태 전환
        enemy.ChangeState(enemy.IdleState);
        StartCoroutine(ResumePatrolAfterDelay());
    }

    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        enemy.ChangeState(enemy.PatrolState);
    }
    
    public void MarkTeleported()
    {
        lastTeleportTime = Time.time;
    }
}
