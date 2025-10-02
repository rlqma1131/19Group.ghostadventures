using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _01.Scripts.Player;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// * Ch3 위장NPC *
// 위장NPC한테 다가가면 대사가 나옵니다. 클릭하면 다음 대사를 볼 수 있습니다.
// 대사 후 선택지 버튼 3개가 나옵니다.
// 선택지에 따라 결과가 달라지진 않습니다.
// 모든 선택지가 끝나면 위장NPC는 Enemy로 변합니다.

public class SecretNPC : MonoBehaviour
{
    [Header("SecretNPC UI")]
    [SerializeField] private Button[] choiceButtons;        // 선택지 버튼
    [SerializeField] private TextMeshProUGUI dialogueText;  // NPC 대사 텍스트
    [SerializeField] private GameObject dialoguePanel;      // 대사 패널        

    // Dialogue 진행 상태
    private readonly Queue<string> promptQueue = new Queue<string>();
    private string[] pendingChoices;
    private Action choiceCallback;
    private bool isActive = false;

    // NPC 상태
    private int currentStep = 0;
    private bool clear = false;
    private bool attackMode = false;

    private Prompt prompt;
    private Player player;
    private Animator anim;
    public Action OnAttack;


    private void Start()
    {
        anim = GetComponent<Animator>();
        prompt = UIManager.Instance.PromptUI;
        player = GameManager.Instance.Player;
        dialoguePanel.SetActive(false);
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !clear)
        {
            clear = true;
            player.PossessionSystem.CanMove = false;
            prompt.ShowPrompt_2(
                "사신..! 도망쳐야 해!", 
                "잠깐.. 왜 아무 반응이 없지..? 더 가까이 가볼까.."
            );
            await Task.Delay(4000);
            player.transform.DOMoveX(player.transform.position.x + 2.5f, 2f) // 2초 동안 X축으로 2.5만큼 이동
              .OnComplete(() => StartDialogue());
        }
    }

    // 대사 시작
    public void StartDialogue()
    {
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        switch (currentStep)
        {
            case 0:
                ShowDialoge(
                    new[] { "과거의 기록", "현재의 해석", "환영" },
                    OnChoiceSelected,
                    "기억, 자아, 진실…세 가지를 묻겠다.",
                    "첫 번째, 기억이란 무엇인가?"
                );
                break;

            case 1:
                ShowDialoge(
                    new[] { "이 몸 (육체)", "축적된 기억", "내가 수행하는 역할" },
                    OnChoiceSelected,
                    "두 번째: ‘나는 누구인가?’"
                );
                break;

            case 2:
                ShowDialoge(
                    new[] { "발견하는 것", "만들어 내는 것", "되돌아보는 것" },
                    OnChoiceSelected,
                    "마지막이다. ‘진실이란 무엇인가? 꺠̴̢̰͌̊진̸̰͗͊̇..."
                );
                break;

            case 3:
                RevealAndAttack();
                break;
        }
    }

    // 선택지를 골랐을 때
    private void OnChoiceSelected()
    {
        currentStep++;
        ShowCurrentStep();
    }

    // 선택지 보여주기
    public void ShowChoices(string[] choices, Action<int> onChoiceSelected)
    {
        // 선택지 개수만큼 버튼 세팅
        for (int i = 0; i < choices.Length; i++)
        {
            int index = i;
            choiceButtons[i].gameObject.SetActive(true);               // 버튼 보이기
            choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i]; // 글자 변경

            // 버튼 클릭 시 → 선택된 index 전달
            choiceButtons[i].onClick.RemoveAllListeners();
            choiceButtons[i].onClick.AddListener(() =>
            {
                HideButtons();
                onChoiceSelected?.Invoke(index);
            });
        }
    }

    // 위장NPC 대사 보여주기
    private void ShowDialoge(string[] choices, Action onChoiceSelected, params string[] lines)
    {
        // 큐 초기화
        promptQueue.Clear();
        foreach (var line in lines) promptQueue.Enqueue(line);

        // UI 활성화
        dialoguePanel.SetActive(true);
        enabled = true;

        pendingChoices = choices;
        choiceCallback = onChoiceSelected;

        ShowNextLine();
    }

    // 다음 대사 보여주기
    private void ShowNextLine()
    {
        if (promptQueue.Count > 0)
        {
            isActive = true;
            string nextLine = promptQueue.Dequeue();
            dialogueText.text = nextLine;
            dialogueText.color = Color.blue;

            if (!dialogueText.gameObject.activeSelf)
                dialogueText.gameObject.SetActive(true);

            // 마지막 줄 + 선택지
            if (promptQueue.Count == 0 && pendingChoices != null)
            {
                ShowChoices(pendingChoices, index =>
                {
                    HidePrompt();
                    choiceCallback?.Invoke();
                });
            }
        }
        else
        {
            HidePrompt();
        }
    }

    // 선택지 버튼 보이기
    public void ShowButtons()
    {
        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(true);
        }
    }

    // 선택지 버튼 숨기기
    public void HideButtons()
    {
        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);  // 현재 선택된 UI 포커스 해제
        }
    }

    // 대사 숨기기
    private void HidePrompt()
    {
        promptQueue.Clear();
        isActive = false;
        dialoguePanel.SetActive(false);
        enabled = false;
    }

    // Enemy로 변함
    private void RevealAndAttack()
    {
        attackMode = true;

        ShowDialoge(
            null, null,
            "답은 중요하지 않아.\nㄱ̷̮̰̙̻̏̇̆ㅏ̸̫͎͚͝͠ㅎ̶̛̛̰̗͕͉̻̇̐̈̋ㅈ̴̈̆̓..."
        );

        OnAttack += TransAttackModeAnimation;
    }
    private void TransAttackModeAnimation()
    {
        anim.Play("SecretNPC_Reveal");
    }

    // SecretNPC_Reveal 애니메이션 종료 후 실행.
    private void StartAttackMode()
    {
        EnemyAI enemy = FindObjectOfType<EnemyAI>();
        
        enemy.StartInvestigate(transform);
        gameObject.SetActive(false);
        player.PossessionSystem.CanMove = true;
    }

    // 클릭시 다음대사로 넘어감
    private void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0) && promptQueue.Count > 0)
        {
            ShowNextLine();
        }
        else if (Input.GetMouseButtonDown(0) && promptQueue.Count == 0 && attackMode)
        {
            HidePrompt();
            OnAttack?.Invoke();
        }
    }
}