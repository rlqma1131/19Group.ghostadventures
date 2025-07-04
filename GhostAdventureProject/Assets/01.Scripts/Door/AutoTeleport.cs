using UnityEngine;

public class AutoTeleport : MonoBehaviour
{
    [Header("Auto Teleport Settings")]
    [SerializeField] private Transform targetTransform; // 목표 Transform (드래그 앤 드롭)
    [SerializeField] private Vector2 targetPos; // 목표 위치 좌표 (백업용)



    private void OnTriggerEnter2D(Collider2D other)
    {
        // GameManager를 통해 플레이어 확인
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            if (other.gameObject == GameManager.Instance.Player)
            {
                TeleportPlayer(other.gameObject);
            }
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        if (player == null) return;

        Vector3 teleportPosition;

        // 우선 targetTransform 사용
        if (targetTransform != null)
        {
            teleportPosition = targetTransform.position;
        }
        // targetTransform이 없으면 좌표 직접 사용
        else
        {
            teleportPosition = new Vector3(targetPos.x, targetPos.y, player.transform.position.z);
        }

        // 플레이어 위치 변경
        player.transform.position = teleportPosition;
    }
}