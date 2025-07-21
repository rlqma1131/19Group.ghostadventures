using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPrompt : MonoBehaviour
{
    [TextArea(2, 5)]
    public string promptMessage = "기본 프롬프트 메시지입니다.";

    public float displayTime = 2f;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (UIManager.Instance != null && UIManager.Instance.PromptUI != null)
        {
            UIManager.Instance.PromptUI.ShowPrompt(promptMessage, displayTime);
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning("Prompt 오브젝트를 찾을 수 없습니다.");
        }
    }
}
