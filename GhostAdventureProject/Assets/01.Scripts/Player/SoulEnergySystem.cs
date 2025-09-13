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

    [SerializeField] private GameObject healingEffectParticle;
    
    [HideInInspector] public float currentRestoreInterval;
    [HideInInspector] public int currentRestoreAmount;
    
    private Coroutine passiveRestoreCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start() {
        currentEnergy = maxEnergy;
    }

    public bool HasEnoughEnergy(int amount) => currentEnergy >= amount;

    public void Consume(int amount) // 에너지 소모
    {
        currentEnergy = Mathf.Max(currentEnergy - amount, 0);
        UIManager.Instance.SoulGaugeUI.SetSoulGauge(currentEnergy);
    }

    public void Restore(int amount) // 에너지 회복
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
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
            
            if (this == null || !gameObject.activeInHierarchy)
                yield break;
            
            Restore(currentRestoreAmount);
        }
    }

    public void SetRestoreBoost(float newInterval, int newAmount)
    {
        if (newAmount <= currentRestoreAmount)
            return;
        
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

    public void EnableHealingEffect()
    {
        if(healingEffectParticle != null)
            healingEffectParticle.gameObject.SetActive(true);
    }

    public void DisableHealingEffect()
    {
        if(healingEffectParticle != null)
            healingEffectParticle.gameObject.SetActive(false);
    }
}
