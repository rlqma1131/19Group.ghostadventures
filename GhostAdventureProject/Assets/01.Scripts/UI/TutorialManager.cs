using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public enum TutorialStep
{
    LivingRoomIntro_Start,
    ShowControlKey,
    BirthdayLetter,
    FirstClue,
    ScanGuide,
    HideGuide,
    FakeMemory,
    HealingLamp,
    WarehouseHint,
    InputName,
    TrueMemory
}
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private HashSet<TutorialStep> completedSteps = new HashSet<TutorialStep>(); // 완료된 튜토리얼
    private Action OnAction;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Show(TutorialStep step)
    {
        if (completedSteps.Contains(step)) return;

        completedSteps.Add(step);

        switch (step)
        {
            case TutorialStep.LivingRoomIntro_Start:
                UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 목표: 이 집 안에 흩어진 기억 조각을 찾아 수집하세요.");
                WaitTimeAfterShowTutorial(3f, TutorialStep.ShowControlKey);
                break;

            case TutorialStep.ShowControlKey: // 튜토리얼 키 보여주기
                UIManager.Instance.TutorialUI_OpenAll(); 
                WaitTimeAfterShowTutorial(3f, TutorialStep.ShowControlKey);
                UIManager.Instance.TutorialUI_CloseAll(); 
                break;

            case TutorialStep.BirthdayLetter:
                // ToastUI.Instance.Show("1~4: 인벤토리 단서 확인 / ESC: 닫기", 3f);
                break;

            // case TutorialStep.ScanGuide:
            //     ToastUI.Instance.Show("E: 기억 스캔", 3f);
            //     break;

            // case TutorialStep.HideGuide:
            //     ToastUI.Instance.Show("※ 특정 오브젝트 빙의는 쉽지 않을 수 있습니다.\n숨을 수 있어!", 3f);
            //     break;

            // ...다른 케이스도 추가
        }
    }

    public bool HasCompleted(TutorialStep step) => completedSteps.Contains(step);

    public void WaitTimeAfterShowTutorial(float time, TutorialStep tutorial)
    {
        DOTween.Sequence()
            .AppendInterval(time)
            .AppendCallback(() =>
            {
                if(OnAction != null) {OnAction?.Invoke();}
                Show(tutorial);
                OnAction = null;
            }); 
    }
}


// // 사용할 때
// if (!TutorialManager.Instance.HasCompleted(TutorialStep.LivingRoomIntro))
// {
//     TutorialManager.Instance.Show(TutorialStep.LivingRoomIntro);
// }

// TutorialManager.Instance.Show(TutorialStep.ScanGuide); // 내부에서 중복 방지됨

// if (!TutorialManager.Instance.HasCompleted(TutorialStep.ForcePossess))
//     StartCoroutine(ForcePossessTutorial());
// // 튜토리얼 흐름을 직접 막아야 할 땐, Show()가 아니라 직접 함수 호출하면 돼.
