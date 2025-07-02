using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Ch1_MainFlashlight : BasePossessable
{
    [SerializeField] private Camera controlCamera;
    [SerializeField] private List<Ch1_FlashlightBeam> flashlightBeams;
    [SerializeField] private Color targetColor = new Color(1, 1, 1);
    [SerializeField] private SpriteRenderer wallLetterRenderer;
    [SerializeField] private Animator clearDoorAnimator;
    
    private bool isControlMode = false;
    private bool puzzleCompleted = false;

    // [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private float timeLimit = 15f;
    private float timeRemaining;
    private bool timerActive = false;
    private bool timerExpired = false;
    private bool timerStarted = false;

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
                    controlCamera.gameObject.SetActive(false);
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
                controlCamera.gameObject.SetActive(false);
                
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
                controlCamera.gameObject.SetActive(true);
            }
        }
        
        if(!isControlMode) return;

        for (int i = 0; i < flashlightBeams.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                flashlightBeams[i].ToggleBeam();
                CheckColorPuzzle();
            }
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
            timerStarted = true;

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
        Color combined = Color.black;

        foreach (var beam in flashlightBeams)
        {
            if(beam.isOn)
            {
                Debug.Log($"켜진 빔 색상: {beam.BeamColor}");
                combined += beam.BeamColor;
            }
        }

        Debug.Log($"조합된 색상: {combined}");
        
        if (ApproximatelyEqual(combined, targetColor))
        {
            if (!puzzleCompleted)
            {
                puzzleCompleted = true;

                RevealLetter();
                // clearDoorAnimator?.SetTrigger("Open");

                // 타이머 중지
                timerActive = false;
                if (timerPanel != null)
                    timerPanel.SetActive(false); // UI 숨김

                Debug.Log("퍼즐 성공!");
            }
        }
        else
        {
            if (!puzzleCompleted)
                HideLetter();
        }
    }

    private void RevealLetter()
    {
        if (wallLetterRenderer != null)
        {
            var c = wallLetterRenderer.color;
            c.a = 1f;
            wallLetterRenderer.color = c;
        }
    }

    private void HideLetter()
    {
        if (wallLetterRenderer != null)
        {
            var c = wallLetterRenderer.color;
            c.a = 0f;
            wallLetterRenderer.color = c;
        }
    }

    private bool ApproximatelyEqual(Color a, Color b, float tolerance = 0.2f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
