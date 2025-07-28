using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
public enum TutorialStep
{
    LivingRoomIntro_Start,
    ShowControlKey_And_HighLightBithdayBox,
    Q_key_Interact,
    HideArea_Interact,
    HideArea_QTE,
    Mouse_Possesse,
    LaundryRoom,
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
    private UIManager uimanager;
    private NoticePopup notice; // 알림창 (가이드안내)
    private Prompt prompt; // 프롬프트 (대사출력)
    private bool canMove; // 플레이어 움직일 수 있는지

    // Ch1 Tutorial
    [SerializeField] private Ch1_CelebrityBox celebrityBox;

    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        uimanager = UIManager.Instance;
        notice = uimanager.NoticePopupUI;
        prompt = uimanager.PromptUI;
        canMove = PossessionSystem.Instance.CanMove;
    }
    
    public void Show(TutorialStep step)
    {
        if (completedSteps.Contains(step)) return;

        completedSteps.Add(step);

        switch (step)
        {
            case TutorialStep.LivingRoomIntro_Start:
                LivingRoom_StartTutorial();
                break;

            case TutorialStep.ShowControlKey_And_HighLightBithdayBox: // 튜토리얼 키 보여주기, 깜짝상자 하이라이트
                PossessionSystem.Instance.CanMove = true;
                uimanager.TutorialUI_OpenAll();
                celebrityBox.highlight.SetActive(true); 
                break;

            case TutorialStep.Q_key_Interact:
                notice.FadeInAndOut("※ 빙의 후에는 Q를 눌러 특정 행동을 할 수 있습니다.");
                break;

            case TutorialStep.HideArea_Interact:
                notice.FadeInAndOut("※특정 오브젝트 빙의는 쉽지않을 수 있습니다.");
                break;

            case TutorialStep.HideArea_QTE:
                prompt.ShowPrompt("숨을 수 있어");
                break;
            case TutorialStep.Mouse_Possesse:
                prompt.ShowPrompt("숨겨진 공간을 찾아볼까?");
                break;
            
            case TutorialStep.LaundryRoom:
                prompt.ShowPrompt("…여긴… 잠깐, 문이… 닫혔어?");
                notice.FadeInAndOut("※ 제한 시간 내에 퍼즐을 해결하지 못하면 나갈 수 없습니다.");
                break;
            // case TutorialStep.HideGuide:
            //     ToastUI.Instance.Show("※ 특정 오브젝트 빙의는 쉽지 않을 수 있습니다.\n숨을 수 있어!", 3f);
            //     break;

            // ...다른 케이스도 추가
        }
    }

    // 거실 진입시
    public async void LivingRoom_StartTutorial()
    {   
        PossessionSystem.Instance.CanMove = false;
        await Task.Delay(2000);
        prompt.ShowPrompt_2("나도 모르게 여기로 들어왔어..", "여기서 기억을 찾을 수 있을까?");
        await Task.Delay(3000);
        notice.FadeInAndOut("※ 목표: 이 집 안에 흩어진 기억 조각을 찾아 수집하세요.");
        await Task.Delay(3000);
        WaitTimeAfterShowTutorial(0f, TutorialStep.ShowControlKey_And_HighLightBithdayBox);
    }
    

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
    public bool HasCompleted(TutorialStep step) => completedSteps.Contains(step);
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
