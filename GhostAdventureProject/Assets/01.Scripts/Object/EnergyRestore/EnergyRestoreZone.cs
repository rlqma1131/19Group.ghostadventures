using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;

public class EnergyRestoreZone : MonoBehaviour
{
    [SerializeField] private int bonusRestoreAmount;
    [SerializeField] private float reduceInterval;

    public bool IsActive = true;

    Player player;
    
    void Start() => player = GameManager.Instance.Player;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && IsActive) {
            player.SoulEnergySystem.SetRestoreBoost(reduceInterval, player.SoulEnergySystem.baseRestoreAmount + bonusRestoreAmount);
            player.SoulEnergySystem.EnableHealingEffect();
            TutorialManager.Instance.Show(TutorialStep.EnergyRestoreZone);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (player.SoulEnergySystem != null && player.SoulEnergySystem.gameObject.activeInHierarchy)
            {
                player.SoulEnergySystem.ResetRestoreBoost();
                player.SoulEnergySystem.DisableHealingEffect();
            }
        }
    }
}
