using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Ch4_Book : BasePossessable
{
    [Header("Drop & Note")]
    [SerializeField] Transform dropPoint;            // 책이 떨어질 위치
    [SerializeField] GameObject note;          // 쪽지 프리팹(비활성 템플릿)

    bool hasDropped;

    protected override void Start() {
        base.Start();
        if(note) note.SetActive(false);
    }

    public override void TriggerEvent() {
        if (!IsPossessed || hasDropped) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 1) 책 떨어뜨리는 연출
            Vector3 endPos = dropPoint
                ? dropPoint.position
                : transform.position + Vector3.down * 0.4f;

            // 살짝 회전+튕김 연출 (DOTween 사용 가정)
            transform.DOMove(endPos, 0.15f).SetEase(Ease.OutQuad);
            transform.DORotate(new Vector3(0f,0f,Random.Range(-15f,15f)), 0.15f);

            // 2) NOTE 등장 (이미 하던 거 유지)
            if (note) note.SetActive(true);

            // 3) 플레이어 강제 빙의 해제
            if (IsPossessed)
            {
                Unpossess(); // BasePossessable 쪽 함수 그대로 사용
            }

            // 4) 이 책은 더 이상 조작 불가
            hasActivated = false;
            hasDropped = true;

            // 5) 상호작용 목록에서 제거
            if (player) player.InteractSystem.RemoveInteractable(gameObject);

            // 6) 프롬프트 안내
            UIManager.Instance?.PromptUI2.ShowPrompt_UnPlayMode("쪽지가 나타났다", 1.3f);
        }
    }
}
