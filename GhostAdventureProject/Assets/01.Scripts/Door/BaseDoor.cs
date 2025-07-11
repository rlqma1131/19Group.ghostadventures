using UnityEngine;

public abstract class BaseDoor : MonoBehaviour
{
    [Header("문 세팅")]
    protected bool isLocked = false;
    [SerializeField] protected Transform targetDoor; // 이동할 문 오브젝트 (드래그 앤 드롭)
    [SerializeField] protected Vector2 targetPos; // 좌표 직접 입력 방식 (백업용)

    [Header("문 비주얼 설정")]
    [SerializeField] protected GameObject closedObject;    // WoodDoor_Close (잠긴 상태)
    [SerializeField] protected GameObject OpenObject;      // WoodDoor_Side (열린 상태)

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

        // TODO: 테스트 완료 후 삭제 필요 - V키 테스트 코드
        if (playerNearby && Input.GetKeyDown(KeyCode.V))
        {
            TestToggleDoor();
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
        if (isLocked)
        {
            // 잠긴 상태: Close 스프라이트 보이기, Side 스프라이트 숨기기
            if (closedObject != null)
                closedObject.SetActive(true);
            if (OpenObject != null)
                OpenObject.SetActive(false);
        }
        else
        {
            // 열린 상태: Close 스프라이트 숨기기, Side 스프라이트 보이기
            if (closedObject != null)
                closedObject.SetActive(false);
            if (OpenObject != null)
                OpenObject.SetActive(true);
        }
    }

    // TODO: 테스트 완료 후 삭제 필요 - 테스트용 메서드
    protected virtual void TestToggleDoor()
    {
        isLocked = !isLocked;
        UpdateDoorVisual();

        string status = isLocked ? "잠김" : "열림";
        Debug.Log($"[테스트] 문 상태 변경: {status}");
    }

    // Enemy가 접근할 수 있는 public 메서드들
    public Transform GetTargetDoor() => targetDoor;
    public Vector2 GetTargetPos() => targetPos;
}