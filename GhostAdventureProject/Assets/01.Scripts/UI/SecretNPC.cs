using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretNPC : MonoBehaviour
{
    public SecretNPC_Dialogue dialogueUI;
    public SpriteRenderer npcSprite;
    public Sprite trueFormSprite;
    public Color redEyeColor = Color.red;
    public Prompt prompt;

    // public EnemyChaseAI chaseAI;

    private int currentStep = 0;

    void Start()
    {
        StartDialogue();
        prompt = UIManager.Instance.PromptUI;
    }

    public void StartDialogue()
    {
        ShowCurrentStep();
    }

    void ShowCurrentStep()
    {
        switch (currentStep)
        {
            case 0:
                prompt.ShowPrompt_Click(
                "기억, 자아, 진실…세 가지를 묻겠다.",
                "첫 번째, 기억이란 무엇인가?");
                dialogueUI.ShowChoices(
                    new string[] { "과거의 기록", "현재의 해석", "환영" },
                    OnChoiceSelected
                );
                break;

            case 1:
                prompt.ShowPrompt_Click("두 번째: ‘나는 누구인가?’");
                dialogueUI.ShowChoices(
                    new string[] { "이 몸 (육체)", "축적된 기억", "내가 수행하는 역할" },
                    OnChoiceSelected
                );
                break;

            case 2:
                prompt.ShowPrompt_Click("마지막이다. ‘진실이란 무엇인가? 꺠̴̢̰͌̊진̸̰͗͊̇ ̴̰͌͆̿͋");
                dialogueUI.ShowChoices(  
                    new string[] { "발견하는 것", "만들어 내는 것", "되돌아보는 것" },
                    OnChoiceSelected
                );
                break;

            case 3:
                RevealAndAttack();
                break;
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        currentStep++;
        ShowCurrentStep();
    }

    void RevealAndAttack()
    {
        prompt.ShowPrompt_Click("답은 중요하지 않아", "ㄱ̷̮̰̙̻̏̇̆ㅏ̸̫͎͚͝͠ㅎ̶̛̛̰̗͕͉̻̇̐̈̋ㅈ̴̢̢͎̠͍̭͈̈̆̓̈́̀̇̏̕라는 게 중요하지");

        // 외형 교체 + 눈 색상 + 글리치 효과 등
        npcSprite.sprite = trueFormSprite;
        npcSprite.color = redEyeColor;

        // 공격 시작
        // chaseAI.BeginChase();
    }

    //구간	설명
// StartDialogue()	처음 대화 시작 (NPC 접근 시 호출)
// ShowCurrentStep()	현재 단계에 맞는 대사/선택지 표시
// ShowChoices()	버튼으로 선택지 표시, 클릭 시 콜백
// OnChoiceSelected()	선택하면 다음 단계로 이동
// RevealAndAttack()	위장 해제 + AI 추격 시작
}
