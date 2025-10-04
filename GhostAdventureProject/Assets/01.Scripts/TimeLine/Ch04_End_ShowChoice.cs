using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Playables;
public class Ch04_End_ShowChoice : MonoBehaviour
{
    //선택지 보여주는 스크립트

    [SerializeField] private PlayableDirector director;
    public CanvasGroup targetUI;
    public SpriteRenderer targetSprite1;
    public SpriteRenderer targetSprite2;
    public Collider2D targetCollider1;
    public Collider2D targetCollider2;
    public float fadeDuration = 1.5f;

    void Start()
    {
        director.stopped += OnTimelineEnd;


        targetCollider1.enabled = false;
        targetCollider2.enabled = false;
        targetUI.alpha = 0f;
    }

    private void OnTimelineEnd(PlayableDirector director)
    {
        // 타임라인이 끝나면 점점 나타남
        targetUI.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine);
        targetSprite1.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(() => targetCollider1.enabled = true);
        targetSprite2.DOFade(1f, fadeDuration).SetEase(Ease.InOutSine).OnComplete(() => targetCollider2.enabled = true);
    }
}
