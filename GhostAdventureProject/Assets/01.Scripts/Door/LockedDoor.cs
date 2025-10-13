using UnityEngine;
using UnityEngine.Events;

public class LockedDoor : BaseDoor
{
    [Header("Locked Door Settings")] 
    [SerializeField] string puzzleId = ""; // 퍼즐 식별자 (예: "LivingRoom_Kitchen")
    [SerializeField] AudioClip lockedSound;
    [SerializeField] AudioClip unlockSound;
    [SerializeField] bool initiallyLocked = true;

    [Header("Pair Door System")] 
    [SerializeField] LockedDoor pairedDoor; // 페어 문 (양방향 연결)

    [Header("Event Trigger after entered")] 
    [SerializeField] UnityEvent OnEnter;
    
    AudioSource audioSource;

    override protected void Start() {
        isLocked = initiallyLocked; // 기본적으로 잠김
        audioSource = GetComponent<AudioSource>();

        // 부모 클래스의 Start 호출
        base.Start();

        // 페어 문 자동 연결 (둘 다 서로를 참조하도록)
        if (pairedDoor != null && pairedDoor.pairedDoor != this) {
            pairedDoor.pairedDoor = this;
        }
    }

    override protected void TryInteract() {
        if (isLocked) {
            Debug.Log("문이 잠겨있습니다!");
            PlaySound(lockedSound);
        }
        else {
            OnEnter?.Invoke();
            TeleportPlayer();
        }
    }

    // 퍼즐 해결 시 호출하는 메서드
    public void UnlockDoors() {
        if (!isLocked) return;

        // 자신 열기
        isLocked = false;
        PlaySound(unlockSound);
        Debug.Log($"{gameObject.name} 문이 열렸습니다!");
        OnDoorUnlocked();
        MarkDoorStateChanged();

        // 페어 문도 열기 (무한 루프 방지)
        if (pairedDoor != null && pairedDoor.isLocked) {
            pairedDoor.isLocked = false;
            pairedDoor.PlaySound(pairedDoor.unlockSound);
            Debug.Log($"{pairedDoor.gameObject.name} 페어 문이 열렸습니다!");
            pairedDoor.OnDoorUnlocked();
            pairedDoor.MarkDoorStateChanged();
        }
    }

    // 페어 문 함께 잠그기
    public void LockDoors() {
        // 자신 잠그기
        if (!isLocked) {
            isLocked = true;
            Debug.Log($"{gameObject.name} 문이 잠겼습니다!");
            UpdateDoorVisual(true);
            MarkDoorStateChanged();
        }

        // 페어 문도 잠그기
        if (pairedDoor != null && !pairedDoor.isLocked) {
            pairedDoor.isLocked = true;
            Debug.Log($"{pairedDoor.gameObject.name} 페어 문이 잠겼습니다!");
            pairedDoor.UpdateDoorVisual(true);
            pairedDoor.MarkDoorStateChanged();
        }
    }

    void OnDoorUnlocked() => UpdateDoorVisual(true);

    void PlaySound(AudioClip clip) {
        if (audioSource && clip) SoundManager.Instance.PlaySFX(clip);
    }

    // 테스트용 메서드들
    [ContextMenu("Test - Solve Puzzle")]
    public void TestSolvePuzzle() => UnlockDoors();

    [ContextMenu("Test - Lock Pair")]
    public void TestLockPair() => LockDoors();

    // 퍼즐 매니저에서 사용할 수 있는 프로퍼티
    public string PuzzleId => puzzleId;

    void OnMouseEnter() => UIManager.Instance.SetCursor(isLocked ? UIManager.CursorType.LockDoor : UIManager.CursorType.OpenDoor);
    void OnMouseExit() => UIManager.Instance.SetCursor(UIManager.CursorType.Default);
}