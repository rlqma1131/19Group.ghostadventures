using UnityEngine;

public abstract class BaseDoor : BaseInteractable
{
    [Header("문 세팅")]
    protected bool isLocked = false;
    [SerializeField] protected Transform targetDoor; // 이동할 문 오브젝트 (드래그 앤 드롭)
    [SerializeField] protected Vector2 targetPos; // 좌표 직접 입력 방식 (백업용)

    [Header("문 비주얼 설정")]
    [SerializeField] protected GameObject closedObject;    // WoodDoor_Close (잠긴 상태)
    [SerializeField] protected GameObject OpenObject;      // WoodDoor_Side (열린 상태)

    protected bool playerNearby = false;
    protected SpriteRenderer spriteRenderer;
    private bool previousLockedState = false;
    public bool IsLocked => isLocked;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorVisual();
    }

    protected virtual void Update()
    {
        if (PlayerInteractSystem.Instance.CurrentClosest != gameObject)
            return;

        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        SetHighlight(playerNearby);
    }

    protected abstract void TryInteract();

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }

    protected void TeleportPlayer()
    {
        GameObject player = GameManager.Instance?.Player;
        if (player == null)
            return;

        Vector3 teleportPosition = targetDoor != null ? targetDoor.position : (Vector3)targetPos;

        player.transform.position = teleportPosition;
    }

    protected void UpdateDoorVisual()
    {
        if(isLocked == previousLockedState)
            return; // 상태변경없으면 무시
        
        previousLockedState = isLocked;
        
        if(closedObject != null)
            closedObject.SetActive(isLocked);
        if(OpenObject != null)
            OpenObject.SetActive(!isLocked);
    }

    // Enemy가 접근할 수 있는 public 메서드들
    public Transform GetTargetDoor() => targetDoor;
    public Vector2 GetTargetPos() => targetPos;
}