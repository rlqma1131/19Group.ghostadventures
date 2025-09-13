using UnityEngine;

public class AutoTeleport : MonoBehaviour
{
    [Header("Auto Teleport Settings")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector2 targetPos;
    [SerializeField] private float ignoreDuration = 1f; // 같은 포탈 재진입 무시 시간

    private GameObject lastTeleportedObject;
    private float lastTeleportTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == lastTeleportedObject && Time.time - lastTeleportTime < ignoreDuration)
        {
            Debug.Log("[AutoTeleport] 재진입 방지 쿨타임");
            return;
        }

        if (IsTeleportable(other.gameObject))
        {
            TeleportObject(other.gameObject);

            // 대상 포탈에도 최근 정보 전달
            AutoTeleport targetPortal = targetTransform?.GetComponent<AutoTeleport>();
            if (targetPortal != null)
            {
                targetPortal.MarkRecentlyTeleported(other.gameObject);
            }
        }
    }

    private bool IsTeleportable(GameObject obj)
    {
        // GameManager에 등록된 Player
        if (GameManager.Instance != null && obj == GameManager.Instance.PlayerObj)
            return true;

        // Enemy AI
        if (obj.GetComponent<EnemyAI>() != null)
            return true;

        return false;
    }

    private void TeleportObject(GameObject obj)
    {
        Vector3 destination = targetTransform != null
            ? targetTransform.position
            : new Vector3(targetPos.x, targetPos.y, obj.transform.position.z);

        obj.transform.position = destination;

        Debug.Log($"[AutoTeleport] {obj.name} 텔레포트 완료 → {destination}");
    }

    public void MarkRecentlyTeleported(GameObject obj)
    {
        lastTeleportedObject = obj;
        lastTeleportTime = Time.time;
    }
}