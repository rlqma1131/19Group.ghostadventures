using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Rendering;

public class Ch3_LockerSelector : MonoBehaviour
{
    public int RemainingOpens = 2;
    [SerializeField] private GameObject b1fDoor;
    [SerializeField] private LockedDoor lockedb1fDoor;
    public bool IsSolved { get; private set; } = false;
    
    // [Header("정답 시 등장 오브젝트 연출")]
    // [SerializeField] private Transform rewardObject;   // 기억조각 오브젝트
    // [SerializeField] private Transform targetPoint;    // 플레이어 앞 위치
    // [SerializeField] private float dropDuration = 2f;
    //
    // [Header("카메라 이동 관련")]
    // [SerializeField] private Cinemachine.CinemachineVirtualCamera playerCam;
    // [SerializeField] private Cinemachine.CinemachineVirtualCamera rewardCam;
    //
    // [SerializeField] private float cameraReturnDelay = 2f;
    
    [SerializeField] private List<ClueData> requiredClues;
    private List<Ch3_Locker> openedLockers = new List<Ch3_Locker>();
    public bool IsPenaltyActive { get; private set; } = false;
    
    [Header("Attempts")]
    [SerializeField] private int opensPerAttempt = 2; // 시도당 2회(기본값)
    private int currentAttempt = 1;                   // 1~3

    [Header("Blackout (Global Volume weight)")]
    [SerializeField] private Volume globalVolume;          // URP Global Volume (씬 오브젝트 드래그)
    [SerializeField] private Vector2 blackoutHoldRange = new Vector2(5f, 7f); // 어둠 유지
    [SerializeField] private float fadeOutTime = 0.6f;     // 0->1
    [SerializeField] private float fadeInTime = 1.0f;      // 1->0

    [Header("Penalty Door (다른 문)")]
    [Tooltip("2번째 시도 실패 시 잠글 '다른 문' (기존 문 아님)")]
    [SerializeField] private LockedDoor penaltyDoor;

    private void Awake()
    {
        // 시작값 정리 (기존 흐름 유지)
        currentAttempt = 1;
        RemainingOpens = Mathf.Max(1, opensPerAttempt);
        if (globalVolume != null) globalVolume.weight = 0f;
    }

    public void OnCorrectBodySelected()
    {
        b1fDoor.SetActive(true);
        // 기존 흐름: b1fDoor On -> 다음 프레임에 lockedb1fDoor SolvePuzzle
        // (원래 있던 코드 유지)
        IsSolved = true;
        ConsumeClue(requiredClues);

        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
        {
            locker.SetActivateState(false);
            if (!locker.IsCorrectBody)
            {
                locker.TryClose();
            }
            else
            {
                Collider2D col = locker.GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = false;
            }
        }

        // ★ 추가: 3번째 시도에서 성공하면 2번째 실패 때 잠갔던 "다른 문"을 다시 연다
        if (currentAttempt >= 3 && penaltyDoor != null)
        {
            penaltyDoor.SolvePuzzle();
        }

        StartCoroutine(DelaySolvePuzzle());
    }

    private IEnumerator DelaySolvePuzzle()
    {
        yield return null; // 다음 프레임
        lockedb1fDoor.SolvePuzzle();
    }

    // ====== 실패(오답) 진입점: 기존 흐름 유지, 내부 로직만 강화 ======
    public void OnWrongBodySelected()
    {
        if (RemainingOpens <= 0)
        {
            // 기존엔 단순 5초 대기 후 리셋
            // -> 시도번호에 따라 암전/잠금/게임오버 분기
            StartCoroutine(ResetLockersAfterPenalty());
        }
    }

    // ====== 핵심: 시도별 페널티 적용 ======
    private IEnumerator ResetLockersAfterPenalty()
    {
        IsPenaltyActive = true;

        // 잠시 모든 로커 비활성 (기존 흐름 유지)
        var lockers = FindObjectsOfType<Ch3_Locker>();
        foreach (var locker in lockers)
            locker.SetActivateState(false);

        // 1,2번째 시도는 암전. 2번째는 잠금까지.
        if (currentAttempt <= 2)
        {
            // 2번째 시도 실패면 '다른 문' 잠금
            if (currentAttempt == 2 && penaltyDoor != null)
                penaltyDoor.LockPair();

            // 어두워짐 0->1
            yield return StartCoroutine(FadeVolumeWeight(1f, fadeOutTime));

            // 유지 (5~7초)
            float hold = Random.Range(blackoutHoldRange.x, blackoutHoldRange.y);
            yield return new WaitForSeconds(hold);

            // 열었던 로커 원복 + 다음 시도 준비
            foreach (var locker in openedLockers)
            {
                locker.TryClose();
                locker.SetActivateState(HasAllClues());
            }
            openedLockers.Clear();
            RemainingOpens = Mathf.Max(1, opensPerAttempt);

            // 밝아짐 1->0
            yield return StartCoroutine(FadeVolumeWeight(0f, fadeInTime));

            // 다음 시도로 진행
            currentAttempt = Mathf.Min(3, currentAttempt + 1);
            IsPenaltyActive = false;
            yield break;
        }
        else
        {
            // 3번째 시도 실패: 암전 없이 즉시 게임오버
            IsPenaltyActive = false;
            PlayerLifeManager.Instance.HandleGameOver();
            yield break;
        }
    }

    private IEnumerator FadeVolumeWeight(float target, float duration)
    {
        if (globalVolume == null || duration <= 0f)
        {
            if (globalVolume != null) globalVolume.weight = target;
            yield break;
        }

        float start = globalVolume.weight;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 타임스케일 영향 없이 연출
            globalVolume.weight = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        globalVolume.weight = target;
    }
    
    public bool HasAllClues()
    {
        foreach (var clue in requiredClues)
        {
            if (!UIManager.Instance.Inventory_PlayerUI.collectedClues.Contains(clue))
                return false;
        }
        return true;
    }
    
    private void ConsumeClue(List<ClueData> clues)
    {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
    }

    public void RegisterOpenedLocker(Ch3_Locker locker)
    {
        if (!openedLockers.Contains(locker))
            openedLockers.Add(locker);
    }
}
