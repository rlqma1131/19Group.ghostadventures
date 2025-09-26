using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;

public class Ch2_Bat_Trigger : MonoBehaviour
{
    readonly static int Move = Animator.StringToHash("Move");
    
    [Header("Target of Triggering")] [Tooltip("정답인형 객체넣기")]
    [SerializeField] Ch2_Doll_YameScan_correct correctDoll; // 정답인형
    
    [Header("References")]
    [SerializeField] Animator ani;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Collider2D col;

    [Header("Movement Settings")]
    [SerializeField] float flySpeed = 2f;           // 속도(유지)
    [SerializeField] int curvePoints = 5;           // 경로 분할 수(많을수록 더 요철)
    [SerializeField] float jitterAmplitude = 1.0f;  // 흔들림 세기(월드 단위)
    [SerializeField] float pathSpan = 25f;          // 시작점에서 목표까지 대략 거리(카메라 밖까지 충분히)

    [Header("VFX Settings")]
    [SerializeField] float fadeDuration = 0.25f;
    
    [Header("Audio Settings")]
    [SerializeField] SoundEventConfig soundConfig;
    [SerializeField] AudioClip TriggerSound_clip;
    [SerializeField] AudioClip ClearSound_clip;
    
    Transform player;
    bool Clear_Bat;                         // 박쥐들 다 없어졌는지 확인
    bool triggered;
    bool dying;
    Vector3 flyDir;
    
    void Start()
    {
        ani = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        player = GameManager.Instance.Player.transform;
    }

    void Update() {
        if (correctDoll.isOpen_UnderGroundDoor && !Clear_Bat) {
            MoveToTargetDirection(player);
            PlaySoundAndFadeOut(ClearSound_clip);
            Clear_Bat = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        MoveToTargetDirection(other.transform);
    }

    void MoveToTargetDirection(Transform target) {
        ani.SetBool(Move, true);
        if (soundConfig && !correctDoll.isOpen_UnderGroundDoor && !Clear_Bat) {
            SoundTrigger.TriggerSound(target.position, soundConfig.soundRange, soundConfig.chaseDuration);
            TutorialManager.Instance.Show(TutorialStep.TouchBat);
        }

        if (col) col.enabled = false;

        // ★ 트리거 순간 플레이어를 향한 레이 방향 고정
        flyDir = (target.position - transform.position).normalized;
        if (flyDir.sqrMagnitude < 1e-6f) flyDir = Vector3.right;
        RunPathTween();
    }

    void RunPathTween()
    {
        PlaySoundAndFadeOut(TriggerSound_clip);
        // 수직 방향(2D용)
        Vector3 perp = new Vector3(-flyDir.y, flyDir.x, 0f).normalized;

        // 화면 밖까지 대략 가는 타깃(직선 기준)
        Vector3 end = transform.position + flyDir * pathSpan;

        // 경로 생성: 시작→끝 사이에 약간의 perp 오프셋을 가진 웨이포인트들
        var path = new List<Vector3>();
        path.Add(transform.position);
        for (int i = 1; i < curvePoints; i++)
        {
            float t = i / (float)curvePoints;
            // 직선 보간 + 수직 흔들림(좌우 랜덤)
            float side = Random.Range(-1f, 1f);
            Vector3 point = Vector3.Lerp(transform.position, end, t)
                          + perp * (side * jitterAmplitude * (1f - Mathf.Abs(0.5f - t) * 2f)); 
            // 중앙 구간에서 흔들림이 크고, 양 끝은 작게(자연스러움)
            path.Add(point);
        }
        path.Add(end);

        // 좌우 뒤집기(초기 방향 기준)
        if (sr) sr.flipX = flyDir.x < 0f;

        // 시퀀스 구성
        var seq = DOTween.Sequence();

        // 이동: 속도 기반, 2D에 맞춘 Path 옵션
       seq.Append(transform.DOPath(path.ToArray(),
                flySpeed,                      // duration이 아니라 SetSpeedBased 쓸 거라 '속도'처럼 동작
                PathType.Linear,
                PathMode.TopDown2D).SetSpeedBased(true).SetEase(Ease.Linear));
       
        // 화면 밖으로 충분히 나갔다고 가정 후 페이드
        if (sr && fadeDuration > 0f)
            seq.Append(sr.DOFade(0f, fadeDuration));

        // 완료 시 숨김
        seq.OnComplete(() =>
        {
            if (this) gameObject.SetActive(false);
        });

        // 안전: 숨겼을 시 트윈 정리
        seq.SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    public void PlaySoundAndFadeOut(AudioClip clip, float volume = 1f) {
        if (!clip) return;

        // 임시 오브젝트 + AudioSource 생성
        GameObject tempObj = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.Play();

        // 페이드아웃 트윈
        tempSource.DOFade(0f, clip.length)
                  .SetEase(Ease.Linear)
                  .OnComplete(() => Destroy(tempObj)); // 완전 끝나면 숨기기
    }
}