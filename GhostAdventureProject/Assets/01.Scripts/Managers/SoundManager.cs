using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 이름 → SceneType 변환을 위한 열거형
public enum SceneType
{
    StartScene,
    Ch01_House,
    Ch02_PlayGround,
    Ch03_Hospital,
    CutScene
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("AudioSources")] 
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxLoopSource;
    [SerializeField] private AudioSource enemySource;
    private AudioClip currentClip;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxPool = new();
    private int currentSFXIndex = 0;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip bgm_StartScene;
    [SerializeField] private AudioClip bgm_Ch01_House;
    [SerializeField] private AudioClip bgm_Ch02_PlayGround;
    [SerializeField] private AudioClip bgm_Ch03_Hospital;

    [Header("SFX List")]
    public AudioClip cluePickUP;

    public float BGMVolume => bgmSource.volume;
    public float SFXVolume => sfxSource.volume;
    public AudioSource EnemySource => enemySource;
    //public bool IsBGMMuted => bgmSource.mute;
    //public bool IsSFXMuted => sfxSource.mute;

    private Coroutine bgmFadeCoroutine;

    // Enemy 음악이랑 교체될 때 필요
    private AudioClip lastBGMClip;
    private float lastBGMVolume = 0.2f;

    public AudioClip CurrentBGM => bgmSource ? bgmSource.clip : null;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitSFXPool();
    }

    // 씬 이름에서 SceneType 변환
    private SceneType GetSceneType(string sceneName)
    {
        switch (sceneName)
        {
            case "StartScene": return SceneType.StartScene;
            case "Ch01_House": return SceneType.Ch01_House;
            case "Ch02_PlayGround": return SceneType.Ch02_PlayGround;
            case "Ch03_Hospital": return SceneType.Ch03_Hospital;

            default: return SceneType.CutScene;
        }
    }

    // 씬 로드 시 자동 BGM 전환
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneType sceneType = GetSceneType(scene.name);

        switch (sceneType)
        {
            case SceneType.StartScene:
                ChangeBGM(bgm_StartScene);
                break;
            case SceneType.Ch01_House:
                ChangeBGM(bgm_Ch01_House);
                break;
            case SceneType.Ch02_PlayGround:
                ChangeBGM(bgm_Ch02_PlayGround);
                break;
            case SceneType.Ch03_Hospital:
                ChangeBGM(bgm_Ch03_Hospital);
                break;

            default:
                FadeOutAndStopBGM();
                break;
        }
        Debug.Log($"[SoundManager]씬 실행됨 : {scene.name}");
    }

    // BGM 전환 (자동 페이드)

    public void ChangeBGM(AudioClip newClip, float fadeDuration = 1f, float targetVolume = 0.2f)
    {
        // ★ 안전 가드
        if (bgmSource == null || newClip == null) return;

        lastBGMClip = newClip;
        lastBGMVolume = targetVolume;

        // ★ 같은 클립이 이미 재생 중이면 재시작 금지, 볼륨만 맞춰준다
        if (bgmSource.clip == newClip && bgmSource.isPlaying)
        {
            if (!Mathf.Approximately(bgmSource.volume, targetVolume))
            {
                if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
                bgmFadeCoroutine = StartCoroutine(FadeBGMVolumeTo(targetVolume, fadeDuration)); // ★
            }
            return; // ★ 재생 유지
        }

        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        bgmFadeCoroutine = StartCoroutine(FadeOutInBGM(newClip, fadeDuration, targetVolume));
        Debug.Log($"BGM 실행됨 : {newClip}");
    }

    private IEnumerator FadeOutInBGM(AudioClip newClip, float duration, float targetVolume)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        // 페이드 아웃
        while (timer < duration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = true;
        bgmSource.Play();

        // 페이드 인
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    private IEnumerator FadeBGMVolumeTo(float target, float duration)
    {
        float start = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        bgmSource.volume = target;
    }

    public void FadeOutAndStopBGM(float duration = 1f)
    {
        if (bgmFadeCoroutine != null)
            StopCoroutine(bgmFadeCoroutine);

        bgmFadeCoroutine = StartCoroutine(FadeOutBGMOnly(duration));
    }

    private IEnumerator FadeOutBGMOnly(float duration)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = null;
        bgmSource.volume = startVolume;
    }

    // BGM 강제 정지
    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    // 재생되던 BGM으로 복귀
    public void RestoreLastBGM(float fadeDuration = 1f)
    {
        if (lastBGMClip != null)
        {
            ChangeBGM(lastBGMClip, fadeDuration, lastBGMVolume);
        }
    }

    // 효과음 재생
    public void PlaySFX(AudioClip clip, float volume = 0.6f)
    {
        if (clip == null || sfxPool.Count == 0) return;

        AudioSource source = sfxPool[currentSFXIndex];
        currentSFXIndex = (currentSFXIndex + 1) % sfxPoolSize;

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    private void InitSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            sfxPool.Add(source);
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }

    public void StopAllSFX()
    {
        foreach (var source in sfxPool)
        {
            source.Stop();
            source.clip = null;
        }
    }

    public void FadeOutAndStopSFX(float duration = 1f)
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            StartCoroutine(FadeOutSFXCoroutine(duration));
        }
    }

    private IEnumerator FadeOutSFXCoroutine(float duration)
    {
        float startVolume = sfxSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        sfxSource.Stop();
        sfxSource.clip = null;
        sfxSource.volume = startVolume;
    }
    // BGM 볼륨조절
    public void SetBGMVolume(float sliderValue , AudioSource audioSource)
    {
        float adjustedVolume = Mathf.Pow(sliderValue, 2f); // 지수 적용
        audioSource.volume = adjustedVolume;
    }

    //SFX 볼륨조절
    public void SetSFXVolume(float sliderValue)
    {
        float adjustedVolume = Mathf.Pow(sliderValue, 2f); // 지수 적용

        if (sfxSource) 
            sfxSource.volume = adjustedVolume;

        if (sfxLoopSource) 
            sfxLoopSource.volume = adjustedVolume;

        if (enemySource) 
            enemySource.volume = adjustedVolume;

        foreach (var source in sfxPool) 
            source.volume = adjustedVolume;
    }

    //public void SetBGMMute(bool mute)
    //{
    //    bgmSource.mute = mute;
    //}

    // 음소거
    //public void SetSFXMute(bool mute)
    //{
    //    foreach (var source in sfxPool)
    //    {
    //        source.mute = mute;
    //    }
    //}

    // 루프 효과음 재생
    public void PlayLoopingSFX(AudioClip clip, float volume = 0.8f)
    {
        if (clip == null || sfxLoopSource == null)
            return;

        if (sfxLoopSource.isPlaying && sfxLoopSource.clip == clip)
            return; // 이미 재생 중이면 중복 실행 안 함

        sfxLoopSource.clip = clip;
        sfxLoopSource.volume = volume;
        sfxLoopSource.loop = true;
        sfxLoopSource.Play();

        Debug.Log($"Looping SFX started: {clip.name}");
    }

    public void StopLoopingSFX()
    {
        if (sfxLoopSource != null && sfxLoopSource.isPlaying)
        {
            sfxLoopSource.Stop();
            sfxLoopSource.clip = null;
        }

        Debug.Log("Looping SFX stopped.");
    }

    public void FadeInLoopingSFX(AudioClip clip, float duration = 1f, float targetVolume = 0.5f)
    {
        if (clip == null || sfxLoopSource == null)
            return;

        if (sfxLoopSource.isPlaying && sfxLoopSource.clip == clip)
            return;

        sfxLoopSource.clip = clip;
        sfxLoopSource.volume = 0f;
        sfxLoopSource.loop = true;
        sfxLoopSource.Play();

        StartCoroutine(FadeInCoroutine(duration, targetVolume));
    }

    private IEnumerator FadeInCoroutine(float duration, float targetVolume)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            sfxLoopSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }

        sfxLoopSource.volume = targetVolume;
    }

    public void FadeOutAndStopLoopingSFX(float duration = 1f)
    {
        if (sfxLoopSource != null && sfxLoopSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = sfxLoopSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            sfxLoopSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        sfxLoopSource.Stop();
        sfxLoopSource.clip = null;
        sfxLoopSource.volume = startVolume; // 나중 재사용 대비해서 복원
    }

}
