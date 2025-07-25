using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public class Ch02_NPCEvent : MonoBehaviour
{


    [SerializeField] private PlayableDirector director; // 타임라인 디렉터
    private bool isTimelinePlaying = false; // 타임라인 재생 여부



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isTimelinePlaying)
        {
            
            if (director != null)
            {
                GameObject player = collision.gameObject;
                Vector3 scale = player.transform.localScale;
                scale.x = -Mathf.Abs(scale.x); // 항상 왼쪽 보게
                player.transform.localScale = scale;
                PossessionSystem.Instance.CanMove = false; // 플레이어 이동 불가능하게 설정
                UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
                director.Play();
                isTimelinePlaying = true; // 타임라인 재생 상태로 설정
            }
        }
    }
    private void OnEnable()
    {
        if (director != null)
        {
            director.stopped += OnTimelineStopped; // 타임라인이 중지될 때 이벤트 등록
        }
    }


    private void OnTimelineStopped(PlayableDirector playable)
    {
        PossessionSystem.Instance.CanMove = true; // 플레이어 이동 가능하게 설정
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 다시 열기
        UIManager.Instance.PromptUI.ShowPrompt("이 쪽지는 나를 말하는 건가?", 1.5f); // 프롬프트 UI 닫기
    }

    private void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }
}
