using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRestoreZone : MonoBehaviour
{
    [SerializeField] private int bonusRestoreAmount;
    [SerializeField] private float reduceInterval;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SoulEnergySystem.Instance.SetRestoreBoost(reduceInterval, SoulEnergySystem.Instance.baseRestoreAmount + bonusRestoreAmount);
            SoulEnergySystem.Instance.EnableHealingEffect();
            UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 빛이 나는 곳 근처에서 에너지가 회복됩니다.");
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
