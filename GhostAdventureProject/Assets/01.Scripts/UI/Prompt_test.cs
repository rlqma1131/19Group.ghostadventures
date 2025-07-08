using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prompt_test : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        string[] lines = 
        {
            "안녕하세요. 지금은",
            "프롬프트 테스트 중입니다",
            "잘 작동하나요?"            
        };
        // 클릭으로 다음 line으로 넘어갑니다. ShowPrompt(string[])
        UIManager.Instance.PromptUI.ShowPrompt(lines);

        // 지정한 시간(2f) 뒤에 자동으로 사라집니다. ShowPrompt(string, float)
        UIManager.Instance.PromptUI.ShowPrompt("프롬프트 테스트입니다. 잘 작동 하나요?", 2f);
    }
}


