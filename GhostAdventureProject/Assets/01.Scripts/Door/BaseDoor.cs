using System.Collections;
using _01.Scripts.Player;
using UnityEngine;

public abstract class BaseDoor : BaseInteractable
{
    [Header("문 세팅")] 
    [SerializeField] protected Transform targetDoor; // 이동할 문 오브젝트 (드래그 앤 드롭)
    [SerializeField] protected Vector2 targetPos; // 좌표 직접 입력 방식 (백업용)
    [SerializeField] protected bool isLocked;

    [Header("문 비주얼 설정")] 
    [SerializeField] protected GameObject closedObject; // WoodDoor_Close (잠긴 상태)
    [SerializeField] protected GameObject OpenObject; // WoodDoor_Side (열린 상태)

    bool previousLockedState;

    protected bool playerNearby;
    protected SpriteRenderer spriteRenderer;

    public bool IsLocked => isLocked;

    new protected virtual void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameManager.Instance.Player;
        UpdateDoorVisual(force: true);
    }

    protected virtual void OnEnable() {
        UpdateDoorVisual(force: true);
        // StartCoroutine(_LateRefresh()); // 혹시나 문제가 또 있다면 한프레임 뒤로 미뤄보기
        // IEnumerator _LateRefresh(){ yield return null; UpdateDoorVisual(force:true); }
    }

    protected virtual void Update() {
        if (player.InteractSystem.CurrentClosest != gameObject) return;

        if (EnemyAI.IsAnyQTERunning) {
            ShowHighlight(false);
            return;
        }

        if (playerNearby && Input.GetKeyDown(KeyCode.E)) {
            TryInteract();
        }

        ShowHighlight(playerNearby);
    }

    protected abstract void TryInteract();

    protected override void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            playerNearby = true;
            player.InteractSystem.AddInteractable(gameObject);
        }
    }

    protected override void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            playerNearby = false;
            player.InteractSystem.RemoveInteractable(gameObject);
        }
    }

    protected void TeleportPlayer() {
        if (EnemyAI.IsAnyQTERunning) return;

        GameObject player = GameManager.Instance?.PlayerObj;
        if (player == null)
            return;

        // 1) 충돌 무시 시작
        int playerLayer = player.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // 2) 순간이동
        Vector3 teleportPosition = targetDoor != null
            ? targetDoor.position
            : (Vector3)targetPos;
        player.transform.position = teleportPosition;

        // 3) 1초 후 다시 충돌 허용
        StartCoroutine(RestoreCollision(playerLayer, enemyLayer, 1.5f));
    }

    private IEnumerator RestoreCollision(int pLayer, int eLayer, float delay) {
        yield return new WaitForSecondsRealtime(delay);
        Physics2D.IgnoreLayerCollision(pLayer, eLayer, false);
    }

    protected void UpdateDoorVisual(bool force = false) {
        if (!force && isLocked == previousLockedState)
            return; // 상태변경없으면 무시 (기본 최적화)

        previousLockedState = isLocked;

        if (closedObject != null) closedObject.SetActive(isLocked);
        if (OpenObject != null) OpenObject.SetActive(!isLocked);
    }


    public void SetLockedFromSave(bool value) {
        isLocked = value;
        UpdateDoorVisual(force: true);
    }

    protected void MarkDoorStateChanged() {
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetDoorLocked(uid.Id, isLocked);
    }

    // Enemy가 접근할 수 있는 public 메서드들
    public Transform GetTargetDoor() => targetDoor;
    public Vector2 GetTargetPos() => targetPos;
}