using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterEnemyAI : EnemyAI
{
    [Header("Teleport Settings")]
    public float teleportInterval = 30f; // 텔레포트 주기
    public float teleportDistanceBehindPlayer = 5f;
    public Vector2 allowedMinBounds;
    public Vector2 allowedMaxBounds;

    private float teleportTimer;
    private bool initialDelayPassed = false;
    
    private Transform player;
    private Vector2 playerDirection;

    protected override void Start()
    {
        base.Start();
        teleportTimer = teleportInterval;
        player = GameManager.Instance.Player.transform;
        playerDirection = GameManager.Instance.Player.GetComponent<Rigidbody2D>().velocity;
    }

    protected override void Update()
    {
        base.Update();
        
        if (QTEHandler != null && QTEHandler.IsRunning()) 
            return;

        teleportTimer -= Time.deltaTime;

        if (teleportTimer <= 0f)
        {
            if (!initialDelayPassed)
            {
                initialDelayPassed = true;
                teleportTimer = teleportInterval;
                return;
            }

            TeleportBehindPlayer();
            teleportTimer = teleportInterval;
        }
    }

    private void TeleportBehindPlayer()
    {
        if (playerDirection == Vector2.zero)
        {
            float facing = player.localScale.x;
            playerDirection = new Vector2(facing, 0);
        }
        playerDirection.Normalize();

        Vector2 teleportPos = Vector2.zero;
        bool foundSafeSpot = false;

        for (int i = 0; i < 5; i++)
        {
            Vector2 candidatePos = (Vector2)player.position - playerDirection * teleportDistanceBehindPlayer;
            candidatePos += Random.insideUnitCircle * (1.5f * i);

            candidatePos.x = Mathf.Clamp(candidatePos.x, allowedMinBounds.x, allowedMaxBounds.x);
            candidatePos.y = Mathf.Clamp(candidatePos.y, allowedMinBounds.y, allowedMaxBounds.y);

            Collider2D hit = Physics2D.OverlapCircle(candidatePos, 0.5f, LayerMask.GetMask("Wall"));
            if (hit == null)
            {
                teleportPos = candidatePos;
                foundSafeSpot = true;
                break;
            }
        }

        if (!foundSafeSpot)
            return;

        // // 뒤쪽 위치 = 플레이어 위치 - 방향 * 거리
        // Vector2 teleportPos = (Vector2)player.position - playerDirection * teleportDistanceBehindPlayer;
        //
        // // 이동 가능 범위 제한 적용
        // teleportPos.x = Mathf.Clamp(teleportPos.x, allowedMinBounds.x, allowedMaxBounds.x);
        // teleportPos.y = Mathf.Clamp(teleportPos.y, allowedMinBounds.y, allowedMaxBounds.y);

        // 순간이동 실행
        transform.position = teleportPos;

        // 순간이동 애니메이션 트리거 (애니메이션 있을 경우)
        Animator.SetTrigger("Teleport");
    }
    
    public void OnTeleportAnimationEnd()
    {
        ChangeState(PatrolState);
    }
}
