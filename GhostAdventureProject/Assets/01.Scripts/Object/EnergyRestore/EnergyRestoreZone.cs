using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRestoreZone : MonoBehaviour
{
    [SerializeField] private int bonusRestoreAmount;
    [SerializeField] private float reduceInterval;


    public bool IsActive = true;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsActive)
        {
            SoulEnergySystem.Instance.SetRestoreBoost(reduceInterval, SoulEnergySystem.Instance.baseRestoreAmount + bonusRestoreAmount);
            SoulEnergySystem.Instance.EnableHealingEffect();
            TutorialManager.Instance.Show(TutorialStep.EnergyRestoreZone);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (SoulEnergySystem.Instance != null && SoulEnergySystem.Instance.gameObject.activeInHierarchy)
            {
                SoulEnergySystem.Instance.ResetRestoreBoost();
                SoulEnergySystem.Instance.DisableHealingEffect();
            }
        }
    }

}
