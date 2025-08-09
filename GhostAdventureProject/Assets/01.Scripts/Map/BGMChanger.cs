using UnityEngine;

public class BGMChanger : MonoBehaviour
{
    [Header("이 맵에서 재생할 BGM")]
    [SerializeField] private AudioClip mapBGM;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float targetVolume = 0.2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (SoundManager.Instance == null)
            return;

        // 이미 같은 BGM이면 무시
        if (SoundManager.Instance.CurrentBGM == mapBGM)
            return;

        SoundManager.Instance.ChangeBGM(mapBGM, fadeDuration, targetVolume);
    }
}
