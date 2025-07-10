using UnityEngine;

public abstract class BaseDoor : MonoBehaviour
{
    [Header("문 세팅")]
   protected bool isLocked = false;
    [SerializeField] protected Transform targetDoor; // 이동할 문 오브젝트 (드래그 앤 드롭)
    [SerializeField] protected Vector2 targetPos; // 좌표 직접 입력 방식 (백업용)

    [Header("문 비주얼 설정")]
    [SerializeField] protected GameObject closedObject;
    [SerializeField] protected GameObject OpenObject;

    protected bool playerNearby = false;

    public bool IsLocked => isLocked;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorVisual();
    }

    protected virtual void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    protected abstract void TryInteract();

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("문 근처에 도착했습니다. E키를 눌러 상호작용하세요.");
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    protected void TeleportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 teleportPosition;

            // 우선 targetDoor 오브젝트 참조 방식 사용
            if (targetDoor != null)
            {
                teleportPosition = targetDoor.position;
                Debug.Log($"플레이어가 {targetDoor.name}로 텔레포트했습니다!");
            }
            // targetDoor가 없으면 좌표 직접 입력 방식 사용 (백업)
            else
            {
                teleportPosition = targetPos;
                Debug.Log($"플레이어가 {targetPos}로 텔레포트했습니다!");
            }

            player.transform.position = teleportPosition;
        }
    }

    protected void UpdateDoorVisual()
    {
        if (spriteRenderer == null) return;

        if (isLocked && closedObject != null)
        {
            closedObject.SetActive(true);
        }
        else if (!isLocked && OpenObject != null)
        {
            closedObject.SetActive(false);
        }
    }

}
