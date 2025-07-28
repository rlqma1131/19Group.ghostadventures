using DG.Tweening;
using UnityEngine;

public class LockedDoor : BaseDoor
{
    [Header("Locked Door Settings")]
    [SerializeField] private string puzzleId = ""; // 퍼즐 식별자 (예: "LivingRoom_Kitchen")
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject openIcon;

    [Header("Pair Door System")]
    [SerializeField] private LockedDoor pairedDoor; // 페어 문 (양방향 연결)

    private AudioSource audioSource;

    protected override void Start()
    {
        isLocked = true; // 기본적으로 잠김
        audioSource = GetComponent<AudioSource>();

        // 부모 클래스의 Start 호출
        base.Start();

        // 페어 문 자동 연결 (둘 다 서로를 참조하도록)
        if (pairedDoor != null && pairedDoor.pairedDoor != this)
        {
            pairedDoor.pairedDoor = this;
        }
    }

    protected override void TryInteract()
    {
        if (isLocked)
        {   
            ShowLockIcon();
            Debug.Log("문이 잠겨있습니다!");
            PlaySound(lockedSound);
        }
        else
        {
            TeleportPlayer();
        }
    }

    void ShowLockIcon()
    {
        if(lockIcon != null)
        {
            lockIcon.SetActive(true);

            SpriteRenderer sr = lockIcon.GetComponent<SpriteRenderer>();
            sr.color = new Color(1, 1, 1, 0); // 알파 0부터 시작

            Sequence seq = DOTween.Sequence();
            seq.Append(sr.DOFade(1f, 0.2f));
            seq.AppendInterval(1.5f);
            seq.Append(sr.DOFade(0f, 0.5f));
            seq.OnComplete(() =>
            {
                if(lockIcon != null)
                    lockIcon.SetActive(false);
            });
        }
    }

    // 퍼즐 해결 시 호출하는 메서드
    public void SolvePuzzle()
    {
        if(!isLocked) return;
        
        UnlockPair();
    }

    // 페어 문 함께 열기
    public void UnlockPair()
    {
        // 자신 열기
        if (isLocked)
        {
            isLocked = false;
            PlaySound(unlockSound);
            Debug.Log($"{gameObject.name} 문이 열렸습니다!");
            OnDoorUnlocked();
        }

        // 페어 문도 열기 (무한 루프 방지)
        if (pairedDoor != null && pairedDoor.isLocked)
        {
            pairedDoor.isLocked = false;
            pairedDoor.PlaySound(pairedDoor.unlockSound);
            Debug.Log($"{pairedDoor.gameObject.name} 페어 문이 열렸습니다!");
            pairedDoor.OnDoorUnlocked();
        }
    }

    // 페어 문 함께 잠그기
    public void LockPair()
    {
        // 자신 잠그기
        if (!isLocked)
        {
            isLocked = true;
            Debug.Log($"{gameObject.name} 문이 잠겼습니다!");
            UpdateDoorVisual();
        }

        // 페어 문도 잠그기
        if (pairedDoor != null && !pairedDoor.isLocked)
        {
            pairedDoor.isLocked = true;
            Debug.Log($"{pairedDoor.gameObject.name} 페어 문이 잠겼습니다!");
            pairedDoor.UpdateDoorVisual();
        }
    }

    private void OnDoorUnlocked()
    {
        UpdateDoorVisual();
    }

    private void PlaySound(AudioClip clip)
    {
      

        if (audioSource != null && clip != null)
        {
         
            audioSource.PlayOneShot(clip);
        }
        
    }

    // 테스트용 메서드들
    [ContextMenu("Test - Solve Puzzle")]
    public void TestSolvePuzzle()
    {
        SolvePuzzle();
    }

    [ContextMenu("Test - Lock Pair")]
    public void TestLockPair()
    {
        LockPair();
    }

    // 퍼즐 매니저에서 사용할 수 있는 프로퍼티
    public string PuzzleId => puzzleId;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(!isLocked)
        {
            if(openIcon != null)
                openIcon.SetActive(true);
        }
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        if(!isLocked)
        {
            if(openIcon != null)
                openIcon.SetActive(false);
        }
    }

}