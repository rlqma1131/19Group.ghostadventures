using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class SecretNPC_Dialogue : MonoBehaviour
{
    public Button[] choiceButtons;

    private void Start() 
    {
        gameObject.SetActive(false);
    }

    public void ShowChoices(string[] choices, Action<int> onChoiceSelected)
    {
        gameObject.SetActive(true);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Length)
            {
                int index = i; // 지역 복사 (중요!)
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i];

                choiceButtons[i].onClick.RemoveAllListeners(); // 기존 리스너 제거
                choiceButtons[i].onClick.AddListener(() =>
                {
                    HideAllChoices(); // 버튼 숨기기
                    onChoiceSelected.Invoke(index); // 선택한 인덱스 전달
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void HideAllChoices()
    {
        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(false);
        }
    }
}
