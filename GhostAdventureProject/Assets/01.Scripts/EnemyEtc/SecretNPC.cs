using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

// 위장NPC의 대사/선택지내용을 관리하는 스크립트입니다.

public class SecretNPC : MonoBehaviour
{
    public SecretNPC_Dialogue dialogueUI;
    public SpriteRenderer npcSprite;
    public Sprite trueFormSprite;
    private Prompt prompt;
    private Animator anim;
    private bool clear = false;

    [SerializeField] private int currentStep = 0;
    [SerializeField] private SoundEventConfig soundConfig;

    void Start()
    {
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
                    dialogueUI, new string[] { "과거의 기록", "현재의 해석", "환영" }, 
                    OnChoiceSelected,
                    "기억, 자아, 진실…세 가지를 묻겠다.", "첫 번째, 기억이란 무엇인가?"); 
                break;

            case 1:
                prompt.ShowPrompt_Click(
                    dialogueUI, new string[] { "이 몸 (육체)", "축적된 기억", "내가 수행하는 역할" },
                    OnChoiceSelected,
                    "두 번째: ‘나는 누구인가?’");
                break;

            case 2:
                prompt.ShowPrompt_Click(
                    dialogueUI, new string[] { "발견하는 것", "만들어 내는 것", "되돌아보는 것" },
                    OnChoiceSelected,
                    "마지막이다. ‘진실이란 무엇인가? 꺠̴̢̰͌̊진̸̰͗͊̇ ̴̰͌͆̿͋");
                break;

            case 3:
                RevealAndAttack();
                break;
            case 4:
                prompt.attackmode = true;
                TransAttackMode();
                break;
        }
    }

    void OnChoiceSelected()
    {
        currentStep++;
        ShowCurrentStep();
    }


    void RevealAndAttack()
    {
        prompt.attackmode = true;
        prompt.ShowPrompt_Click(
            dialogueUI, null, null, 
            "답은 중요하지 않아.\nㄱ̷̮̰̙̻̏̇̆ㅏ̸̫͎͚͝͠ㅎ̶̛̛̰̗͕͉̻̇̐̈̋ㅈ̴̢̢͎̠͍̭͈̈̆̓̈́̀̇̏̕ 라는 게 중요하지");
        
        prompt.attack += TransAttackMode;
            
    }

    void TransAttackMode()
    {
        EnemyAI enemy = FindObjectOfType<EnemyAI>();
            enemy.StartInvestigate(transform);
            
        this.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !clear) // 또는 같은 방 입장시
        {
        //     UIManager.Instance.PromptUI.ShowPrompt_2("사신..! 도망쳐야 해!", "잠깐.. 왜 아무 반응이 없지..? 더 가까이 가볼까.."); // 튜토리얼로 변경해도 될 듯
            StartDialogue();
        }
    }
}
