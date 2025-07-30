using UnityEngine;
using UnityEngine.UI;

public class VolumeBarUI_Master : MonoBehaviour
{
    [SerializeField] private Image[] gaugeBlocks; // 미리 배치된 10개의 블록
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private AudioSource targetAudioSource;

    private int currentVolume = 7;
    private const int maxVolume = 10;

    void Start()
    {
        plusButton.onClick.AddListener(IncreaseVolume);
        minusButton.onClick.AddListener(DecreaseVolume);
        UpdateGauge();
    }

    void IncreaseVolume()
    {
        if (currentVolume < maxVolume)
        {
            currentVolume++;
            ApplyVolume();
        }
    }

    void DecreaseVolume()
    {
        if (currentVolume > 0)
        {
            currentVolume--;
            ApplyVolume();
        }
    }

    void ApplyVolume()
    {
        float normalized = currentVolume / (float)maxVolume;
        // if (targetAudioSource != null)
        // targetAudioSource.volume = normalized;

        // 또는 전체 사운드 조절
        AudioListener.volume = normalized;

        UpdateGauge();
    }

    void UpdateGauge()
    {
        for (int i = 0; i < gaugeBlocks.Length; i++)
        {
            gaugeBlocks[i].fillAmount = i < currentVolume ? 1f : 0f;
        }
    }
}
