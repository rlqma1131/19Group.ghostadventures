using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ch1_MainFlashlight : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private List<Ch1_FlashlightBeam> flashlightBeams;
    [SerializeField] private Animator clearDoorAnimator;
    [SerializeField] private List<GameObject> mirrorBeamVisuals; // 맵에서 보여질 빛 시각화용
    
    private bool isControlMode = false;
    private bool puzzleCompleted = false;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private float timeLimit = 15f;
    private float timeRemaining;
    private bool timerActive = false;
    private bool timerExpired = false;
    // private bool timerStarted = false;

    protected override void Update()
    {
        //  타이머 실행
        if (timerActive && !puzzleCompleted && !timerExpired)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                timerExpired = true;
                timerActive = false;

                Debug.Log("시간 초과!");

                if (isControlMode)
                {
                    isControlMode = false;
                    zoomCamera.Priority = 20;   // 카메라 우선순위 높이기
                }

                Unpossess();

                // 타이머 UI 끄기
                if (timerPanel != null)
                    timerPanel.SetActive(false);

                // 타임오버 이벤트 발생 지점
            }

            UpdateTimerText();
        }
        
        if(!isPossessed)
            return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isControlMode)
            {
                // 조작 중이면 조작 종료
                isControlMode = false;
                zoomCamera.Priority = 5; // 카메라 우선순위 낮추기

                if (puzzleCompleted)
                {
                    hasActivated = false;
                    Unpossess();
                }
                else
                {
                    Unpossess(); // 퍼즐 미완료 상태여도 해제 가능
                }
            }
            else if (!puzzleCompleted)
            {
                // 조작 시작
                isControlMode = true;
                zoomCamera.Priority = 20; // 카메라 우선순위 높이기
            }
        }
        
        if(!isControlMode) return;

        for (int i = 0; i < flashlightBeams.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                flashlightBeams[i].ToggleBeam();
                SyncMirrorVisuals();
                CheckColorPuzzle();
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
    
    public override void OnQTESuccess()
    {
        base.OnQTESuccess();

        // 퍼즐이 성공했거나 실패하지 않았으면 기존 흐름 유지
        if (puzzleCompleted)
            return;

        // 실패 이후 재빙의 가능하게 만들기
        if (!timerActive)
        {
            // 타이머 상태 초기화
            timeRemaining = timeLimit;
            timerActive = true;
            timerExpired = false;
            // timerStarted = true;

            if (timerPanel != null)
                timerPanel.SetActive(true);

            // if (timerSlider != null)
            // {
            //     timerSlider.maxValue = timeLimit;
            //     timerSlider.value = timeRemaining;
            // }

            UpdateTimerText();

            Debug.Log("타이머 재시작 (재도전)");
        }
    }
    
    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = $"남은 시간: {seconds}s";
        }
    }

    private void CheckColorPuzzle()
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

                timerActive = false;
                if (timerPanel != null)
                    timerPanel.SetActive(false);

                Debug.Log("퍼즐 성공!");
                
                // 문 열기
                if (clearDoorAnimator != null)
                    clearDoorAnimator.SetTrigger("DoorOpen");
                Debug.Log("문 열림 완료");
            }
        }
        // else
        // {
        //     if (!puzzleCompleted)
        //         HideLetter();
        // }
    }

    // private void RevealLetter()
    // {
    //     if (wallLetterRenderer != null)
    //     {
    //         var c = wallLetterRenderer.color;
    //         c.a = 1f;
    //         wallLetterRenderer.color = c;
    //     }
    // }
    //
    // private void HideLetter()
    // {
    //     if (wallLetterRenderer != null)
    //     {
    //         var c = wallLetterRenderer.color;
    //         c.a = 0f;
    //         wallLetterRenderer.color = c;
    //     }
    // }
}
