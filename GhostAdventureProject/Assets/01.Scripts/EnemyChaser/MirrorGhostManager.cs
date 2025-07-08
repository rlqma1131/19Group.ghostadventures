// MirrorGhostManager.cs (최종 통합 버전)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class MirrorData
{
    [Header("거울 설정")]
    public Transform mirror;
    public GameObject ghostVisual;
    public AudioSource audioSource;
    public AudioClip[] stageSounds;
    public AudioClip[] extraStageSounds;
    public PlayableDirector[] stageVisuals;

    [Header("상태 정보")]
    public GhostState currentState = GhostState.Waiting;
    public int currentStage = 0;
    public float stayTimer = 0f;
    public float stageTimer = 0f;
    public bool isActive = false;
    public float observingTimer = 0f;

    public void ResetMirror()
    {
        currentState = GhostState.Waiting;
        currentStage = 0;
        stayTimer = 0f;
        stageTimer = 0f;
        isActive = false;
        observingTimer = 0f;

        if (ghostVisual != null)
            ghostVisual.SetActive(false);

        foreach (var visual in stageVisuals)
        {
            if (visual != null) visual.Stop();
        }
    }
}

public enum GhostState { Waiting, Observing, Preparing, Emerging, DeathCutscene }

public class MirrorGhostManager : MonoBehaviour
{
    [Header("거울 목록")]
    public List<MirrorData> mirrors = new();

    [Header("기능 설정")]
    public float detectionRange = 10f;
    public float stayThreshold = 5f;
    public float stageProgressTime = 2f;

    [Header("게임 설정")]
    public Camera playerCamera;

    private Transform player;
    private MirrorData currentActiveMirror = null;

    void Start()
    {
        FindPlayer();
        InitializeAllMirrors();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        UpdateMirrorDetection();
        UpdateActiveMirror();
    }

    void FindPlayer()
    {
        if (GameManager.Instance?.Player != null)
        {
            player = GameManager.Instance.Player.transform;
            return;
        }

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            return;
        }

        player = FindObjectOfType<PlayerController>()?.transform;
    }

    void InitializeAllMirrors()
    {
        foreach (var mirror in mirrors)
            mirror.ResetMirror();
    }

    void UpdateMirrorDetection()
    {
        MirrorData closest = GetClosestMirror();

        if (currentActiveMirror != closest)
        {
            if (currentActiveMirror != null)
                currentActiveMirror.isActive = false;

            currentActiveMirror = closest;

            if (closest != null)
                currentActiveMirror.isActive = true;
        }

        if (currentActiveMirror == null)
        {
            foreach (var m in mirrors)
            {
                if (m.isActive)
                {
                    m.isActive = false;
                    m.stayTimer = 0f;
                    m.stageTimer = 0f;
                }
            }
        }
    }

    MirrorData GetClosestMirror()
    {
        MirrorData result = null;
        float minDist = float.MaxValue;

        foreach (var m in mirrors)
        {
            if (m.mirror == null) continue;
            float dist = Vector3.Distance(player.position, m.mirror.position);
            if (dist <= detectionRange && dist < minDist)
            {
                result = m;
                minDist = dist;
            }
        }

        return result;
    }

    void UpdateActiveMirror()
    {
        if (currentActiveMirror == null || !currentActiveMirror.isActive) return;

        Vector3 playerForward = GameManager.Instance.PlayerController.transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 toMirror = new Vector3(currentActiveMirror.mirror.position.x - player.position.x, 0, 0).normalized;
        float dot = Vector3.Dot(playerForward, toMirror);
        bool isLooking = dot > 0.5f;
        bool isBack = dot < -0.3f;
        bool isHiding = GameManager.Instance.PlayerController.TryGetComponent(out PlayerHide hide) && hide.IsHiding;

        UpdateMirrorState(currentActiveMirror, isLooking, isBack, isHiding);
    }

    void UpdateMirrorState(MirrorData m, bool looking, bool back, bool hiding)
    {
        switch (m.currentState)
        {
            case GhostState.Waiting:
                m.stayTimer += Time.deltaTime;
                if (m.stayTimer >= stayThreshold)
                {
                    m.currentState = GhostState.Observing;
                    m.observingTimer = 0f;
                    StartStage(m);
                }
                break;

            case GhostState.Observing:
                if (hiding || back)
                {
                    m.currentState = GhostState.Preparing;
                    m.stageTimer += Time.deltaTime;
                    if (m.stageTimer >= stageProgressTime)
                    {
                        NextStage(m);
                        m.stageTimer = 0f;
                    }
                }
                break;

            case GhostState.Preparing:
                m.stageTimer += Time.deltaTime;
                if (m.stageTimer >= stageProgressTime)
                {
                    NextStage(m);
                    m.stageTimer = 0f;
                }
                if (!hiding && looking)
                {
                    m.currentState = GhostState.Observing;
                }
                break;

            case GhostState.Emerging:
                StartCoroutine(EmergingSequence(m));
                break;

            case GhostState.DeathCutscene:
                StartCoroutine(DeathSequence(m));
                break;
        }
    }

    void StartStage(MirrorData m)
    {
        m.currentStage = 1;
        PlayStageEffect(m, 1);
    }

    void NextStage(MirrorData m)
    {
        m.currentStage++;

        if (m.currentStage <= 7)
        {
            PlayStageEffect(m, m.currentStage);
        }

        if (m.currentStage == 6)
        {
            m.currentState = GhostState.Emerging;
        }
        else if (m.currentStage == 7)
        {
            m.currentState = GhostState.DeathCutscene;
        }
    }

    void PlayStageEffect(MirrorData m, int stage)
    {
        if (m.audioSource != null && stage <= m.stageSounds.Length)
            m.audioSource.PlayOneShot(m.stageSounds[stage - 1]);

        if (m.extraStageSounds != null && stage <= m.extraStageSounds.Length && m.extraStageSounds[stage - 1] != null)
            m.audioSource.PlayOneShot(m.extraStageSounds[stage - 1]);

        if (m.stageVisuals != null && stage <= m.stageVisuals.Length && m.stageVisuals[stage - 1] != null)
            m.stageVisuals[stage - 1].Play();

        if (stage == 4)
            TriggerCameraShake();
    }

    IEnumerator EmergingSequence(MirrorData m)
    {
        Debug.Log("[6단계] 거울 깨짐 + 유령 튀어나옴");
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(0.5f);
        m.currentState = GhostState.DeathCutscene;
    }

    IEnumerator DeathSequence(MirrorData m)
    {
        Debug.Log("[7단계] 사망 연출");
        yield return new WaitForSeconds(3f);

        if (PlayerLifeManager.Instance != null)
        {
            PlayerLifeManager.Instance.HandleGameOver();
        }
        else
        {
            Debug.LogWarning("PlayerLifeManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    void TriggerCameraShake()
    {
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.Shake(0.3f, 0.2f);
        }
    }
    void TriggerCameraShake()
    {
        StartCoroutine(CameraShake(0.3f, 0.2f));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }
}
