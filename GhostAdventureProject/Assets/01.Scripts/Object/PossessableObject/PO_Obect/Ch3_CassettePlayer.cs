using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_CassettePlayer : BasePossessable
{
    [Header("소리 목록")]
    [SerializeField] private AudioClip buttonPush;
    [SerializeField] private AudioClip talkingSound; // 대화 소리
    [SerializeField] private AudioClip glitchSound; // 퍼즐 해결 후 소리

    [Header("오디오 퍼즐 셋팅")]
    [SerializeField] private float minCutoff = 300f;
    [SerializeField] private float maxCutoff = 5000f;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 2f;

    private bool isPlaying = false; // 재생 여부
    private bool isSolved = false; // 문제 해결 여부

    private float answerValue; // 정답 주파수 조정값
    private float answerSpeed; // 정답 재생 속도

    private AudioLowPassFilter lowPassFilter;
    private AudioSource audioSource;
    private float cutoffFrequency = 500f; // 초기값
    private float playbackSpeed = 1f;     // 초기 재생속도 (pitch)

    protected override void Start()
    {
        base.Start();

        audioSource = SoundManager.Instance.GetComponent<AudioSource>();

        lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();

        lowPassFilter.cutoffFrequency = cutoffFrequency;
        audioSource.pitch = playbackSpeed;
    }

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
            SoundManager.Instance.PlaySFX(buttonPush);

            if (isSolved)
            {
                // 퍼즐 해결 후
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.ChangeBGM(glitchSound);
            }
            else
            {
                // 퍼즐 해결 전
                if (!isPlaying)
                {
                    PlayTalkingSound();
                }
                else
                {
                    StopTalkingSound();
                }
            }
        }

        if (!isPlaying)
            return;

        // 재생 중 일때만
        if (Input.GetKeyDown(KeyCode.A))
        {
            cutoffFrequency += 200f;
            cutoffFrequency = Mathf.Clamp(cutoffFrequency, minCutoff, maxCutoff);
            lowPassFilter.cutoffFrequency = cutoffFrequency;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            cutoffFrequency -= 200f;
            cutoffFrequency = Mathf.Clamp(cutoffFrequency, minCutoff, maxCutoff);
            lowPassFilter.cutoffFrequency = cutoffFrequency;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playbackSpeed += 0.1f;
            playbackSpeed = Mathf.Clamp(playbackSpeed, minPitch, maxPitch);
            audioSource.pitch = playbackSpeed;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playbackSpeed -= 0.1f;
            playbackSpeed = Mathf.Clamp(playbackSpeed, minPitch, maxPitch);
            audioSource.pitch = playbackSpeed;
        }

        CheckSolved();
    }

    void PlayTalkingSound()
    {
        isPlaying = true;
        SoundManager.Instance.ChangeBGM(talkingSound);
    }

    void StopTalkingSound()
    {
        isPlaying = false;
        SoundManager.Instance.StopBGM();
    }

    void CheckSolved()
    {
        if (Mathf.Abs(cutoffFrequency - answerValue) < 50f && Mathf.Abs(playbackSpeed - answerSpeed) < 0.05f)
        {
            hasActivated = false;
            isPlaying = false;
            isSolved = true;

            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(talkingSound);

            // 재생 끝났다고 가정하고 다음 처리
            //SoundManager.Instance.ChangeBGM(glitchSound);
            //화면에 단서 출력
            //UIManager.Instance.PromptUI.ShowPrompt("카세트테이프 단서", 5f);
        }
    }
}

