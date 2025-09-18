using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using static _01.Scripts.Utilities.Timer;

public class PlayerSoulEnergy : MonoBehaviour
{
    [Header("Soul Energy Settings")]
    [SerializeField] int maxEnergy;
    [SerializeField, ReadOnly(true)] int currentEnergy;
    [SerializeField] float baseRestoreInterval; // n초마다 자연 회복
    [SerializeField] public int baseRestoreAmount;
    
    [HideInInspector] [SerializeField] float currentRestoreInterval;
    [HideInInspector] [SerializeField] int currentRestoreAmount;

    [Header("Healing Effect Particle Prefab")]
    [SerializeField] GameObject healingEffectParticle;

    // Fields
    CountdownTimer restoreTimer;
    
    // Properties
    public int CurrentEnergy => currentEnergy;

    void OnEnable() {
        currentRestoreInterval = baseRestoreInterval;
        currentRestoreAmount = baseRestoreAmount;
    }
    
    void Start() {
        currentEnergy = maxEnergy;
        
        restoreTimer = new CountdownTimer(currentRestoreInterval);
        restoreTimer.OnTimerStop += () => {
            if (restoreTimer.IsFinished) Restore(currentRestoreAmount);
            restoreTimer.Start();
        };
        restoreTimer.Start();
    }

    void Update() => restoreTimer.Tick(Time.deltaTime);

    public bool HasEnoughEnergy(int amount) => currentEnergy >= amount;

    // 에너지 소비
    public void Consume(int amount) {
        if (currentEnergy <= 0) return;
        
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }
    
    // 에너지 회복
    public void Restore(int amount) {
        if (currentEnergy >= maxEnergy) return;
        
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }
    
    // 전체 회복
    public void RestoreAll() {
        currentEnergy = maxEnergy;
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }

    public void SetRestoreBoost(float newInterval, int newAmount) {
        if (newAmount <= currentRestoreAmount) return;
        
        currentRestoreInterval = newInterval;
        currentRestoreAmount = newAmount;
        
        restoreTimer.Stop();
        restoreTimer.Reset(currentRestoreInterval);
        restoreTimer.Start();
    }

    public void ResetRestoreBoost() {
        currentRestoreInterval = baseRestoreInterval;
        currentRestoreAmount = baseRestoreAmount;
        
        restoreTimer.Stop();
        restoreTimer.Reset(currentRestoreInterval);
        restoreTimer.Start();
    }

    public void EnableHealingEffect() => healingEffectParticle?.gameObject.SetActive(true);

    public void DisableHealingEffect() => healingEffectParticle?.gameObject.SetActive(false);
}
