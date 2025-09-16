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
            player.SoulEnergy.SetRestoreBoost(reduceInterval, player.SoulEnergy.baseRestoreAmount + bonusRestoreAmount);
            player.SoulEnergy.EnableHealingEffect();
            TutorialManager.Instance.Show(TutorialStep.EnergyRestoreZone);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (player.SoulEnergy != null && player.SoulEnergy.gameObject.activeInHierarchy)
            {
                player.SoulEnergy.ResetRestoreBoost();
                player.SoulEnergy.DisableHealingEffect();
            }
        }
    }
}
