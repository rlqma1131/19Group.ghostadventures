using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoulGauge : MonoBehaviour
{
    [SerializeField] Image[] soulGauge; // 영혼게이지
    public int currentSoulGauge {get; private set;}

    public void SetSoulGauge(int amount) // 영혼게이지 1개당 amount: 1
    {   
        currentSoulGauge = amount;
        currentSoulGauge = Mathf.Clamp(currentSoulGauge, 0, soulGauge.Length); 
        for (int i = 0; i < soulGauge.Length; i++)
        {
            soulGauge[i].fillAmount = i < currentSoulGauge ? 1f : 0f; // UI에 반영.
        }
    }
}
