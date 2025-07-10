using UnityEngine;

public class AutoTeleport : MonoBehaviour
{
    [Header("Auto Teleport Settings")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector2 targetPos;
    [SerializeField] private float ignoreDuration = 1f; // 같은 포탈 재진입 무시 시간

    private GameObject lastTeleportedPlayer;
    private float lastTeleportTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.Player == null) return;
        if (other.gameObject != GameManager.Instance.Player) return;

        // 최근 들어온 플레이어가 나 자신이라면 무시
        if (other.gameObject == lastTeleportedPlayer && Time.time - lastTeleportTime < ignoreDuration)
        {
            Debug.Log("[AutoTeleport] 재진입 방지 쿨타임 작동 중");
            return;
        }

        TeleportPlayer(other.gameObject);

        // 대상 포탈에도 정보 전달
        AutoTeleport targetPortal = targetTransform?.GetComponent<AutoTeleport>();
        if (targetPortal != null)
        {
            targetPortal.MarkRecentlyTeleported(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        Vector3 destination = targetTransform != null
            ? targetTransform.position
            : new Vector3(targetPos.x, targetPos.y, player.transform.position.z);

        player.transform.position = destination;

        Debug.Log($"[AutoTeleport] {player.name} 텔레포트 완료 → {destination}");
    }

    public void MarkRecentlyTeleported(GameObject player)
    {
        lastTeleportedPlayer = player;
        lastTeleportTime = Time.time;
    }
}
