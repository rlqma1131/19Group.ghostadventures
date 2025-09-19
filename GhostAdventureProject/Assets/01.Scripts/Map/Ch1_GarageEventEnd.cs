using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_GarageEventEnd : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("더 이상 볼일 없어... 이 집에서 나가자");
        }
    }
}
