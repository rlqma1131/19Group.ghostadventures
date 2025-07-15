using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeleportZoneData
{
    public string zoneName;
    public Vector3 targetPosition;
}

public class EnemyTeleportSystem : MonoBehaviour
{
    [Header("텔레포트 설정")]
    public bool canUseTeleport = true;
    public float teleportChaseRange = 30f;
    public float teleportCooldownTime = 3f;  // 3초로 증가
    public float postTeleportChaseTime = 2f;
    public bool enablePostTeleportChase = false;  // 텔레포트 후 추격 관성 활성화 여부
    public List<TeleportZoneData> teleportZones = new();

    private bool justTeleported = false;
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (justTeleported) return;

        if (canUseTeleport && other.CompareTag("TelePortZone"))
        {
            Debug.Log($"Enemy가 {other.name}에 진입");
            HandleTeleportZone(other);
        }
    }

    // OnTriggerStay2D도 막아야 합니다
    private void OnTriggerStay2D(Collider2D other)
    {
        // 텔레포트존 안에 있는 동안은 아무것도 하지 않음
        if (other.CompareTag("TelePortZone"))
        {
            return;
        }
    }

    private void HandleTeleportZone(Collider2D teleportZone)
    {
        string zoneName = teleportZone.name;
        Debug.Log($"텔레포트 처리 시작: {zoneName}");

        foreach (var teleportData in teleportZones)
        {
            if (zoneName.Contains(teleportData.zoneName))
            {
                Vector3 teleportPosition = teleportData.targetPosition;

                // Y축 고정을 먼저 적용
                if (enemyAI.lockYPosition)
                {
                    teleportPosition.y = enemyAI.fixedYPosition;
                }

                // 텔레포트존 밖으로 위치 조정 (2유닛 정도 떨어뜨리기)
                if (zoneName.Contains("TelePortZone1"))
                {
                    teleportPosition.x -= 2f; // 왼쪽으로 2유닛
                }
                else if (zoneName.Contains("TelePortZone2"))
                {
                    teleportPosition.x += 2f; // 오른쪽으로 2유닛
                }

                transform.position = teleportPosition;
                Debug.Log($"Enemy가 {zoneName}에서 조정된 좌표 {teleportPosition}로 텔레포트!");
                Debug.Log($"Y축 고정 상태: {enemyAI.lockYPosition}, 고정 Y값: {enemyAI.fixedYPosition}");

                StartCoroutine(TeleportCooldown());
                return;
            }
        }

        HandleTeleportZoneOldWay(teleportZone);
    }

    private void HandleTeleportZoneOldWay(Collider2D teleportZone)
    {
        string zoneName = teleportZone.name;
        string targetZoneName = "";

        if (zoneName.Contains("TelePortZone1"))
            targetZoneName = "TelePortZone2";
        else if (zoneName.Contains("TelePortZone2"))
            targetZoneName = "TelePortZone1";

        if (!string.IsNullOrEmpty(targetZoneName))
        {
            GameObject targetZone = GameObject.Find(targetZoneName);
            if (targetZone != null)
            {
                Vector3 teleportPosition = targetZone.transform.position;

                // 텔레포트존 밖으로 위치 조정
                if (targetZoneName.Contains("TelePortZone1"))
                {
                    teleportPosition.x -= 2f; // 왼쪽으로 2유닛
                }
                else if (targetZoneName.Contains("TelePortZone2"))
                {
                    teleportPosition.x += 2f; // 오른쪽으로 2유닛
                }

                if (enemyAI.lockYPosition)
                {
                    teleportPosition.y = enemyAI.fixedYPosition;
                }

                transform.position = teleportPosition;
                Debug.Log($"Enemy가 {zoneName}에서 {targetZoneName} 근처 {teleportPosition}로 텔레포트!");
                StartCoroutine(TeleportCooldown());
            }
        }
    }

    private IEnumerator TeleportCooldown()
    {
        justTeleported = true;
        enemyAI.StopMoving();

        yield return new WaitForSeconds(teleportCooldownTime);

        justTeleported = false;
        enemyAI.isMoving = true;
    }

    public bool TryTeleportChase(Transform player)
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > teleportChaseRange)
        {
            Debug.Log($"플레이어가 너무 멀어서 텔레포트 추격 포기 (거리: {distanceToPlayer:F2})");
            return false;
        }

        GameObject[] teleportZones = GameObject.FindGameObjectsWithTag("TelePortZone");

        if (teleportZones.Length < 2)
        {
            Debug.Log("텔레포트존이 부족합니다");
            return false;
        }

        GameObject closestZone = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject zone in teleportZones)
        {
            float distance = Vector3.Distance(transform.position, zone.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestZone = zone;
            }
        }

        if (closestZone != null && closestDistance <= 10f)
        {
            Debug.Log($"텔레포트 추격: {closestZone.name}으로 즉시 텔레포트!");
            ExecuteTeleport(closestZone);
            return true;
        }

        Debug.Log($"가장 가까운 텔레포트존까지 거리가 너무 멀음: {closestDistance:F2}");
        return false;
    }

    private void ExecuteTeleport(GameObject fromZone)
    {
        GameObject[] allZones = GameObject.FindGameObjectsWithTag("TelePortZone");

        GameObject targetZone = null;
        foreach (GameObject zone in allZones)
        {
            if (zone != fromZone)
            {
                targetZone = zone;
                break;
            }
        }

        if (targetZone != null)
        {
            Vector3 teleportPosition = targetZone.transform.position;

            if (enemyAI.lockYPosition)
            {
                teleportPosition.y = enemyAI.fixedYPosition;
            }

            transform.position = teleportPosition;
            Debug.Log($"Enemy가 텔레포트 추격으로 {targetZone.name}에 도착!");

            // 텔레포트 후 추격 관성 적용 (옵션)
            if (enablePostTeleportChase)
            {
                StartCoroutine(PostTeleportChase());
            }
            else
            {
                Debug.Log("텔레포트 후 추격 관성 비활성화됨");
            }
        }
    }

    private IEnumerator PostTeleportChase()
    {
        Debug.Log("텔레포트 후 추격 관성 시작!");

        float timer = 0f;
        Vector3 lastPosition = transform.position;
        float stuckTimer = 0f;

        while (timer < postTeleportChaseTime)
        {
            Transform player = enemyAI.Player;
            if (player != null)
            {
                var movement = GetComponent<EnemyMovement>();

                // 플레이어가 감지 범위 내에 있는지 확인
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= enemyAI.detectionRange)
                {
                    movement.SetTarget(player.position);
                    movement.MoveToTarget(movement.chaseSpeed);

                    // 막혀서 움직이지 못하는지 체크
                    if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
                    {
                        stuckTimer += Time.deltaTime;
                        if (stuckTimer >= 1f) // 1초 동안 못 움직이면 추격 중단
                        {
                            Debug.Log("텔레포트 후 추격 중 막혀서 조기 종료!");
                            break;
                        }
                    }
                    else
                    {
                        stuckTimer = 0f; // 움직이고 있으면 리셋
                        lastPosition = transform.position;
                    }
                }
                else
                {
                    Debug.Log("플레이어가 감지 범위를 벗어나서 추격 관성 종료!");
                    break;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("텔레포트 후 추격 관성 종료!");
    }
}