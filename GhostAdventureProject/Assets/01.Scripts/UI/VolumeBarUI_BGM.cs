// using UnityEngine;
// using UnityEngine.UI;

// public class VolumeBarUI_BGM : MonoBehaviour
// {
//     [SerializeField] private Image[] gaugeBlocks; // 미리 배치된 10개의 블록
//     [SerializeField] private Button plusButton;
//     [SerializeField] private Button minusButton;
//     [SerializeField] private float targetAudioSource;

//     private int currentVolume;
//     private const int maxVolume = 10;

//     void Start()
//     {
//         plusButton.onClick.AddListener(IncreaseVolume);
//         minusButton.onClick.AddListener(DecreaseVolume);
//         UpdateGauge();
//         currentVolume = (int)SoundManager.Instance.BGMVolume;
//     }

//     void IncreaseVolume()
//     {
//         if (currentVolume < maxVolume)
//         {
//             currentVolume++;
//             ApplyVolume();
//         }
//     }

//     void DecreaseVolume()
//     {
//         if (currentVolume > 0)
//         {
//             currentVolume--;
//             ApplyVolume();
//         }
//     }

//     void ApplyVolume()
//     {
//         float normalized = currentVolume / (float)maxVolume;
//         if (targetAudioSource != null)
//         targetAudioSource.volume = normalized;

//         UpdateGauge();
//     }

//     void UpdateGauge()
//     {
//         for (int i = 0; i < gaugeBlocks.Length; i++)
//         {
//             gaugeBlocks[i].fillAmount = i < currentVolume ? 1f : 0f;
//         }
//     }
// }
