using System.Collections;
using UnityEngine;

public class SoulEnergySystem : MonoBehaviour
{
    // 싱글톤
    public static SoulEnergySystem Instance { get; private set; }

    public int maxEnergy;
    public int currentEnergy;
    
    public float baseRestoreInterval; // n초마다 자연 회복
    public int baseRestoreAmount;
    
    [HideInInspector] public float currentRestoreInterval;
    [HideInInspector] public int currentRestoreAmount;
    
    private Coroutine passiveRestoreCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        currentEnergy = maxEnergy;
    }

    public bool HasEnoughEnergy(int amount) => currentEnergy >= amount;

    public void Consume(int amount) // 에너지 소모
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        Debug.Log($"영혼 에너지 {amount} 소모, 현재: {currentEnergy}");
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }

    public void Restore(int amount) // 에너지 회복
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        Debug.Log($"영혼 에너지 {amount} 회복, 현재: {currentEnergy}");
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }

    public void RestoreAll() // 전체 회복
    {
        currentEnergy = maxEnergy;
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }

    private void OnEnable()
    {
        currentRestoreInterval = baseRestoreInterval;
        currentRestoreAmount = baseRestoreAmount;
        passiveRestoreCoroutine = StartCoroutine(PassiveRestoreRoutine());
    }

    private IEnumerator PassiveRestoreRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentRestoreInterval);
            Restore(currentRestoreAmount);
        }
    }

    public void SetRestoreBoost(float newInterval, int newAmount)
    {
        currentRestoreInterval = newInterval;
        currentRestoreAmount = newAmount;
        
        if(passiveRestoreCoroutine != null)
            StopCoroutine(passiveRestoreCoroutine);
        passiveRestoreCoroutine = StartCoroutine(PassiveRestoreRoutine());
    }

    public void ResetRestoreBoost()
    {
        currentRestoreInterval = baseRestoreInterval;
        currentRestoreAmount = baseRestoreAmount;
        
        if(passiveRestoreCoroutine != null)
            StopCoroutine(passiveRestoreCoroutine);
        passiveRestoreCoroutine = StartCoroutine(PassiveRestoreRoutine());
    }
}
