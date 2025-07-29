using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_CassettePlayer : BasePossessable
{
    [Header("소리 목록")]
    [SerializeField] private AudioClip talkingSound; // 대화 소리
    [SerializeField] private AudioClip glitchSound; // 퍼즐 해결 후 소리

    private bool isPlaying = false; // 재생 여부
    private bool isSolved = false; // 문제 해결 여부

    private float answerValue; // 정답 주파수 조정값
    private float answerSpeed; // 정답 재생 속도

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            //SoundManager.Instance.ChangeBGM(챕터3BGM);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isSolved)
            {
                // 퍼즐 해결 후
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.ChangeBGM(glitchSound);
            }
            else
            {
                // 퍼즐 해결 전
                PlayTalkingSound();
            }
        }

        if (!isPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            //cutoffFrequency 증가
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            //cutoffFrequency 감소
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            // 재생 속도 증가
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // 재생 속도 감소
        }

        CheckSolved();
    }

    void PlayTalkingSound() // 토글
    {
        isPlaying = true;
        SoundManager.Instance.ChangeBGM(talkingSound);

        // 혹은 SoundManager.Instance.StopBGM();
        // isPlaying = false;
    }

    void CheckSolved()
    {
        if(cutoffFrequency == answerValue && 재생 속도 == answerSpeed)
        { 
            hasActivated = false; // 빙의 불가

            isPlaying = false; // 재생 중지
            isSolved = true;
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(talkingSound);
            //화면에 정답 타이핑 효과

            // 재생 끝나면
            SoundManager.Instance.ChangeBGM(glitchSound);
            UIManager.Instance.PromptUI.ShowPrompt("카세트테이프 단서", 5f);
        }
    }
}

