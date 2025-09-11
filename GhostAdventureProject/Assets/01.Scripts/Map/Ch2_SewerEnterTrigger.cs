using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SewerEnterTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
    [SerializeField] private GameObject enterTeleport;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        if (UIManager.Instance != null && UIManager.Instance.PromptUI != null)
        {
            UIManager.Instance.PromptUI.ShowPrompt("여긴 어디지..?", 2f);
            hasTriggered = true;
        }
        
        enterTeleport.SetActive(false);
    }
}
