using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static _01.Scripts.Utilities.Timer;

public class Ch1_MainFlashlight : BasePossessable
{
    [Header("Main References")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private List<Ch1_FlashlightBeam> flashlightBeams;
    [SerializeField] private List<GameObject> mirrorBeamVisuals; // 맵에서 보여질 빛 시각화용
    [SerializeField] private LockedDoor Door; // 퍼즐 성공 시 열릴 문

    [Header("Timer References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private SoundEventConfig soundConfig;
    [SerializeField] private float timeLimit = 60f;
   
    private bool isControlMode = false;
    private bool puzzleCompleted = false;
    private bool inputLocked = false;

    CountdownTimer countdownTimer;

    override protected void Start() {
        base.Start();

        countdownTimer = new CountdownTimer(timeLimit);
        countdownTimer.OnTimerStart += () => {
            if (timerPanel) timerPanel.SetActive(true);
            UpdateTimerText();
        };
        countdownTimer.OnTimerStop += () => {
            if (countdownTimer.IsFinished) {
                if (timerPanel) timerPanel.SetActive(false);
                
                UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 제한 시간이 종료되었습니다.");

                if (isControlMode) {
                    EnemyAI.PauseAllEnemies();
                    isControlMode = false;
                    UIManager.Instance.PlayModeUI_CloseAll();
                    zoomCamera.Priority = 20;
                }

                zoomCamera.Priority = 5;
                Unpossess();
                UIManager.Instance.PlayModeUI_OpenAll();
                EnemyAI.ResumeAllEnemies();
                
                // 타임오버 이벤트 발생 지점
                // 플레이어 조작 멈춤 & Lives 1 로 만듦
                player.PossessionSystem.CanMove = false;

                // 적 호출 - SoundTriggerObject 사용
                SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
            }
            else {
                if (timerPanel) timerPanel.SetActive(false);

                EnemyAI.ResumeAllEnemies();
                UIManager.Instance.PlayModeUI_OpenAll();
                zoomCamera.Priority = 5;

                hasActivated = false;
                MarkActivatedChanged();

                Unpossess();

                Door.UnlockDoors();
                UIManager.Instance.NoticePopupUI.FadeInAndOut("퍼즐을 해결했습니다. 출구가 열렸습니다.");
                UIManager.Instance.PromptUI.ShowPrompt("N", 3f);
            }
        };
    }

    protected override void Update()
    {
        // 타이머 실행
        if (!puzzleCompleted) {
            countdownTimer.Tick(Time.deltaTime);
            UpdateTimerText();
        }

        if (!isPossessed || inputLocked) return;

        if (Input.GetKeyDown(KeyCode.E)) {
            if (isControlMode) {
                EnemyAI.ResumeAllEnemies();
                isControlMode = false;
                UIManager.Instance.PlayModeUI_OpenAll();
                zoomCamera.Priority = 5;
                Unpossess();
            }
        }

        if (!isControlMode) return;

        for (int i = 0; i < flashlightBeams.Count; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                flashlightBeams[i].ToggleBeam();
                SyncMirrorVisuals();
                StartCoroutine(CheckColorPuzzle()); // ← 코루틴으로 호출
            }
        }
    }

    private void SyncMirrorVisuals()
    {
        for (int i = 0; i < flashlightBeams.Count; i++)
        {
            bool isOn = flashlightBeams[i].isOn;
            mirrorBeamVisuals[i].SetActive(isOn);
        }
    }

    // public override void OnQTESuccess()
    // {
    //     base.OnQTESuccess();

    //     // 퍼즐이 성공했거나 실패하지 않았으면 기존 흐름 유지
    //     if (puzzleCompleted)
    //         return;

    //     // 실패 이후 재빙의 가능하게 만들기
    //     if (!timerActive)
    //     {
    //         // 타이머 상태 초기화
    //         timeRemaining = timeLimit;
    //         timerActive = true;
    //         timerExpired = false;

    //         if (timerPanel != null)
    //             timerPanel.SetActive(true);

    //         UpdateTimerText();
    //     }
    // }

    private void UpdateTimerText() {
        if (!timerText) return;
        
        int seconds = Mathf.CeilToInt(countdownTimer.Progress * timeLimit);
        timerText.text = seconds.ToString();

        // 색상 변화 (15초 → 흰색, 5초 → 빨간색)
        float t = Mathf.InverseLerp(5f, 15f, countdownTimer.Progress * timeLimit); // 15일 때 1, 5일 때 0
        timerText.color = Color.Lerp(Color.red, Color.white, t);
    }

    private IEnumerator CheckColorPuzzle()
    {
        // 정답: 2, 4, 5번 (index 기준으로 1, 3, 4)
        bool light2 = flashlightBeams[1].isOn;
        bool light4 = flashlightBeams[3].isOn;
        bool light5 = flashlightBeams[4].isOn;

        // 다른 빛이 켜지면 안 됨
        bool othersOff = true;
        for (int i = 0; i < flashlightBeams.Count; i++)
        {
            if ((i != 1 && i != 3 && i != 4) && flashlightBeams[i].isOn)
            {
                othersOff = false;
                break;
            }
        }

        if (light2 && light4 && light5 && othersOff)
        {
            if (!puzzleCompleted)
            {
                puzzleCompleted = true;
                inputLocked = true;
                ChapterEndingManager.Instance.CollectCh1Clue("N");

                // 퍼즐 성공 후 3초 대기
                yield return new WaitForSecondsRealtime(3f);

                countdownTimer.Stop();
            }
        }
    }

    // 빙의 하고 바로 줌
    public override void OnPossessionEnterComplete()
    {
        base.OnPossessionEnterComplete();
        EnemyAI.PauseAllEnemies();
        isControlMode = true;
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomCamera.Priority = 20;
        
        // 퍼즐이 성공했거나 실패하지 않았으면 기존 흐름 유지
        // 현재 퍼즐 풀이를 진행 중인 경우 타이머 시작 X
        if (puzzleCompleted || countdownTimer.IsRunning) return;
        
        UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 제한 시간: 45초");
        
        // 타이머 상태 초기화
        countdownTimer.Start();
    }
}
