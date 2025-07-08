using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageEffect
{
    [Header("사운드")]
    public AudioClip[] sounds;           // 각 단계별 사운드 배열

    [Header("비주얼")]
    public GameObject visual;            // 비주얼 이펙트 GameObject

    [Header("특수 효과 (4단계에서만 사용)")]
    public bool triggerCameraShake = false;
    public bool continuousShake = false;     // 지속적인 흔들림 여부
    public float shakeDuration = 0.3f;       // 일회성 흔들림 지속시간
    public float shakeMagnitude = 0.2f;
}

[System.Serializable]
public class MirrorData
{
    [Header("거울 설정")]
    public Transform mirror;
    public GameObject ghostVisual;
    public AudioSource audioSource;

    [Header("단계별 효과 (인덱스 1~7 사용)")]
    public StageEffect[] stageEffects = new StageEffect[8];  // 인덱스 1~7 사용

    [Header("특수 단계 설정")]
    public float emergingDelay = 1f;        // 6단계 대기 시간
    public float emergingDuration = 0.5f;   // 6단계 지속 시간
    public float deathDelay = 3f;           // 7단계 대기 시간

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

        // 모든 단계별 비주얼 비활성화
        foreach (var stageEffect in stageEffects)
        {
            if (stageEffect != null && stageEffect.visual != null)
                stageEffect.visual.SetActive(false);
        }
    }
}

public enum GhostState
{
    Waiting,      // 대기
    Observing,    // 관찰
    Preparing,    // 추격 준비
    Emerging,     // 출현
    DeathCutscene // 사망 컷씬
}

public class MirrorGhostManager : MonoBehaviour
{
    [Header("거울 목록")]
    public List<MirrorData> mirrors = new List<MirrorData>();

    [Header("기능 설정")]
    public float detectionRange = 10f;
    public float stayThreshold = 5f;
    public float stageProgressTime = 2f;

    [Header("게임 설정")]
    public Camera playerCamera;
    public Vector3 originalCameraPosition = new Vector3(0, 0, -10);  // 카메라 원래 위치

    private Transform player;
    private MirrorData currentActiveMirror = null;
    private bool isPlayerControlDisabled = false;
    private Coroutine currentShakeCoroutine = null;  // 현재 진행 중인 흔들림 코루틴
    private bool isEmergingInProgress = false;       // 6단계 진행 중 플래그
    private bool isDeathInProgress = false;          // 7단계 진행 중 플래그

    void Start()
    {
        FindPlayer();
        InitializeAllMirrors();

        // 카메라 원래 위치 저장
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
            Debug.Log($"카메라 원래 위치 저장: {originalCameraPosition}");
        }
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
        // GameManager를 통해 플레이어 찾기
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player.transform;
            Debug.Log("GameManager를 통해 플레이어 찾음");
            return;
        }

        // 태그로 플레이어 찾기
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("태그를 통해 플레이어 찾음");
            return;
        }

        // PlayerController 컴포넌트로 찾기
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
            Debug.Log("PlayerController를 통해 플레이어 찾음");
            return;
        }

        Debug.LogError("플레이어를 찾을 수 없습니다!");
    }

    void InitializeAllMirrors()
    {
        foreach (var mirror in mirrors)
        {
            mirror.ResetMirror();
        }
    }

    void UpdateMirrorDetection()
    {
        MirrorData closestMirror = GetClosestMirror();

        if (currentActiveMirror != closestMirror)
        {
            if (currentActiveMirror != null)
            {
                currentActiveMirror.isActive = false;
                Debug.Log($"거울 감지 범위 벗어남: {currentActiveMirror.mirror.name}");
            }

            currentActiveMirror = closestMirror;

            if (currentActiveMirror != null)
            {
                ActivateMirror(currentActiveMirror);
            }
        }

        if (currentActiveMirror == null)
        {
            foreach (var mirror in mirrors)
            {
                if (mirror.isActive)
                {
                    mirror.isActive = false;
                    mirror.stayTimer = 0f;
                    mirror.stageTimer = 0f;
                    Debug.Log($"거울 {mirror.mirror.name} - 감지 리셋만 수행 (단계는 유지)");
                }
            }
        }
    }

    MirrorData GetClosestMirror()
    {
        MirrorData closest = null;
        float closestDistance = float.MaxValue;

        foreach (var mirror in mirrors)
        {
            if (mirror.mirror == null) continue;

            float distance = Vector3.Distance(player.position, mirror.mirror.position);

            if (distance <= detectionRange && distance < closestDistance)
            {
                closest = mirror;
                closestDistance = distance;
            }
        }

        return closest;
    }

    void ActivateMirror(MirrorData mirror)
    {
        mirror.isActive = true;
        OnMirrorActivated(mirror);
    }

    void DeactivateMirror(MirrorData mirror)
    {
        mirror.isActive = false;

        // 4단계 이상에서 거울이 비활성화되면 흔들림 중지
        if (mirror.currentStage >= 4)
        {
            StopContinuousShake();
        }

        mirror.ResetMirror();
        OnMirrorDeactivated(mirror);
    }

    void UpdateActiveMirror()
    {
        if (currentActiveMirror == null || !currentActiveMirror.isActive)
            return;

        UpdatePlayerDetection(currentActiveMirror);
        UpdateGhostBehavior(currentActiveMirror);
    }

    void UpdatePlayerDetection(MirrorData mirror)
    {
        Vector3 playerForward = GameManager.Instance.PlayerController.transform.localScale.x > 0
            ? Vector3.right
            : Vector3.left;

        Vector3 toMirror = new Vector3(
            mirror.mirror.position.x - player.position.x,
            0f,
            0f
        ).normalized;

        float dotProduct = Vector3.Dot(playerForward, toMirror);

        bool isLookingAtMirror = dotProduct > 0.5f;
        bool isBackTurned = dotProduct < -0.3f;

        UpdateMirrorState(mirror, isLookingAtMirror, isBackTurned);
    }

    void UpdateMirrorState(MirrorData mirror, bool isLookingAtMirror, bool isBackTurned)
    {
        bool isHiding = false;
        if (GameManager.Instance.PlayerController.TryGetComponent<PlayerHide>(out var hide))
        {
            isHiding = hide.IsHiding;
        }

        switch (mirror.currentState)
        {
            case GhostState.Waiting:
                HandleWaitingState(mirror, isHiding);
                break;
            case GhostState.Observing:
                HandleObservingState(mirror, isLookingAtMirror, isBackTurned, isHiding);
                break;
            case GhostState.Preparing:
                HandlePreparingState(mirror, isLookingAtMirror, isHiding);
                break;
            case GhostState.Emerging:
                HandleEmergingState(mirror);
                break;
            case GhostState.DeathCutscene:
                HandleDeathCutscene(mirror);
                break;
        }
    }

    void UpdateGhostBehavior(MirrorData mirror)
    {
        // 이 메서드는 UpdateMirrorState에서 호출됨
    }

    void HandleWaitingState(MirrorData mirror, bool isHiding)
    {
        mirror.stayTimer += Time.deltaTime;

        if (mirror.stayTimer >= stayThreshold)
        {
            ChangeMirrorState(mirror, GhostState.Observing);
            mirror.observingTimer = 0f;
            StartStageProgression(mirror);
        }
    }

    void HandleObservingState(MirrorData mirror, bool isLookingAtMirror, bool isBackTurned, bool isHiding)
    {
        if (isHiding || isBackTurned)
        {
            if (mirror.currentState != GhostState.Preparing)
            {
                ChangeMirrorState(mirror, GhostState.Preparing);
            }

            mirror.stageTimer += Time.deltaTime;

            if (mirror.stageTimer >= stageProgressTime)
            {
                ProgressToNextStage(mirror);
                mirror.stageTimer = 0f;
            }

            return;
        }

        if (isLookingAtMirror)
        {
            OnPlayerLookingAtMirror(mirror);
        }
    }

    void HandlePreparingState(MirrorData mirror, bool isLookingAtMirror, bool isHiding)
    {
        mirror.stageTimer += Time.deltaTime;

        if (mirror.stageTimer >= stageProgressTime)
        {
            ProgressToNextStage(mirror);
            mirror.stageTimer = 0f;
        }

        if (!isHiding && isLookingAtMirror)
        {
            ChangeMirrorState(mirror, GhostState.Observing);
        }
    }

    void HandleEmergingState(MirrorData mirror)
    {
        if (!isEmergingInProgress)
        {
            isEmergingInProgress = true;
            StartCoroutine(EmergingSequence(mirror));
        }
    }

    void HandleDeathCutscene(MirrorData mirror)
    {
        if (!isDeathInProgress)
        {
            isDeathInProgress = true;
            DisablePlayerControl();
            StartCoroutine(DeathSequence(mirror));
        }
    }

    void StartStageProgression(MirrorData mirror)
    {
        mirror.currentStage = 1;
        PlayStageEffect(mirror, mirror.currentStage);
    }

    void ProgressToNextStage(MirrorData mirror)
    {
        mirror.currentStage++;

        if (mirror.currentStage <= 7)
        {
            PlayStageEffect(mirror, mirror.currentStage);
        }

        if (mirror.currentStage == 6)
        {
            ChangeMirrorState(mirror, GhostState.Emerging);
        }
        else if (mirror.currentStage == 7)
        {
            ChangeMirrorState(mirror, GhostState.DeathCutscene);
        }
    }

    void PlayStageEffect(MirrorData mirror, int stage)
    {
        // 배열 인덱스 검증 (stage는 1부터 시작하므로 그대로 사용)
        if (stage < 1 || stage >= mirror.stageEffects.Length || mirror.stageEffects[stage] == null)
            return;

        StageEffect effect = mirror.stageEffects[stage];

        // 사운드 재생
        if (mirror.audioSource != null && effect.sounds != null)
        {
            foreach (var sound in effect.sounds)
            {
                if (sound != null)
                    mirror.audioSource.PlayOneShot(sound);
            }
        }

        // 비주얼 효과 - 현재 단계만 활성화, 나머지는 비활성화
        for (int i = 1; i < mirror.stageEffects.Length; i++)
        {
            if (mirror.stageEffects[i] != null && mirror.stageEffects[i].visual != null)
            {
                mirror.stageEffects[i].visual.SetActive(i == stage);
            }
        }

        // 카메라 흔들림 효과 (4단계에서만)
        if (stage == 4 && effect.triggerCameraShake)
        {
            if (effect.continuousShake)
            {
                StartContinuousShake(effect.shakeMagnitude);
            }
            else
            {
                TriggerCameraShake(effect.shakeDuration, effect.shakeMagnitude);
            }
        }

        // 디버그 로그
        Debug.Log($"[거울 유령] {mirror.mirror.name} - {stage}단계 연출 발동");

        // 단계별 특수 효과
        switch (stage)
        {
            case 1:
                Debug.Log("[1단계] 또각또각 발소리");
                OnStage1(mirror);
                break;
            case 2:
                Debug.Log("[2단계] 거울에 그림자 비침");
                OnStage2(mirror);
                // 2단계부터 거울에서만 F, E 키 차단
                PlayerHide.mirrorKeysBlocked = true;
                Debug.Log("거울에서만 F, E 키 차단 시작!");
                break;
            case 3:
                Debug.Log("[3단계] 연구원 형상 등장");
                OnStage3(mirror);
                break;
            case 4:
                Debug.Log("[4단계] 유령 점점 가까이");
                OnStage4(mirror);
                break;
            case 5:
                Debug.Log("[5단계] 얼굴 들이댐");
                OnStage5(mirror);
                break;
            case 6:
                Debug.Log("[6단계] 거울 깨짐 + 유령 튀어나옴");
                OnStage6(mirror);
                break;
            case 7:
                Debug.Log("[7단계] 사망 연출");
                OnStage7(mirror);
                break;
        }
    }

    void ChangeMirrorState(MirrorData mirror, GhostState newState)
    {
        mirror.currentState = newState;
        OnStateChanged(mirror, newState);
    }

    IEnumerator EmergingSequence(MirrorData mirror)
    {
        Debug.Log("[6단계] 거울 깨짐 + 유령 튀어나옴");
        OnMirrorBreak(mirror);
        yield return new WaitForSeconds(mirror.emergingDelay);

        Debug.Log("[6단계-연장] 유령 등장 연출");
        OnGhostEmerge(mirror);
        yield return new WaitForSeconds(mirror.emergingDuration);

        Debug.Log("[6단계 완료] 7단계로 전환");
        ChangeMirrorState(mirror, GhostState.DeathCutscene);
    }

    IEnumerator DeathSequence(MirrorData mirror)
    {
        Debug.Log("[7단계] 사망 연출 시작");
        OnPlayerDeath(mirror);

        // 즉시 흔들림 중지하고 업데이트 정지
        StopContinuousShake();
        mirror.isActive = false;

        yield return new WaitForSeconds(mirror.deathDelay);

        Debug.Log("게임오버 처리 시작");

        // 게임오버 전에 한 번 더 카메라 위치 확인
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        // 게임오버
        if (PlayerLifeManager.Instance != null)
        {
            PlayerLifeManager.Instance.HandleGameOver();
        }
        else
        {
            Debug.LogWarning("PlayerLifeManager 인스턴스를 찾을 수 없습니다.");
        }
    }

    void TriggerCameraShake(float duration = 0.3f, float magnitude = 0.2f)
    {
        StartCoroutine(CameraShake(duration, magnitude));
    }

    void StartContinuousShake(float magnitude)
    {
        // 이미 진행 중인 흔들림이 있으면 중지
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        currentShakeCoroutine = StartCoroutine(ContinuousShake(magnitude));
    }

    void StopContinuousShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }

        // 카메라를 원래 위치로 복구
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPosition;
            playerCamera.transform.localRotation = Quaternion.identity;
            Debug.Log($"카메라 위치 원복: {originalCameraPosition}");
        }
    }

    IEnumerator ContinuousShake(float magnitude)
    {
        while (true)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 원래 위치에서 흔들림 적용
            playerCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);

            yield return new WaitForSeconds(0.05f); // 초당 20번 흔들림
        }
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 원래 위치에서 흔들림 적용
            playerCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        // 원래 위치로 복구
        playerCamera.transform.localPosition = originalCameraPosition;
    }

    void DisablePlayerControl()
    {
        isPlayerControlDisabled = true;
        OnPlayerControlDisabled();
    }

 
 

    // ===========================================
    // 연출팀에서 구현할 이벤트 메서드들
    // ===========================================

    void OnMirrorActivated(MirrorData mirror)
    {
        Debug.Log($"거울 활성화: {mirror.mirror.name}");
    }

    void OnMirrorDeactivated(MirrorData mirror)
    {
        Debug.Log($"거울 비활성화: {mirror.mirror.name}");
    }

    void OnPlayerLookingAtMirror(MirrorData mirror)
    {
        // 플레이어가 거울을 보고 있을 때 연출
    }

    void OnStateChanged(MirrorData mirror, GhostState newState)
    {
        Debug.Log($"거울 {mirror.mirror.name} 상태 변경: {newState}");
    }

    void OnStage1(MirrorData mirror) { }
    void OnStage2(MirrorData mirror) { }
    void OnStage3(MirrorData mirror) { }
    void OnStage4(MirrorData mirror) { }
    void OnStage5(MirrorData mirror) { }
    void OnStage6(MirrorData mirror) { }  // 새로 추가
    void OnStage7(MirrorData mirror) { }  // 새로 추가

    void OnMirrorBreak(MirrorData mirror) { }
    void OnGhostEmerge(MirrorData mirror) { }
    void OnPlayerDeath(MirrorData mirror) { }
    void OnPlayerControlDisabled() { }
    void OnPlayerControlEnabled() { }
    void OnGameReset() { }

    // ===========================================
    // 퍼블릭 메서드들
    // ===========================================

    public void DestroyMirror(Transform mirrorTransform)
    {
        MirrorData mirror = mirrors.Find(m => m.mirror == mirrorTransform);
        if (mirror != null)
        {
            DeactivateMirror(mirror);
            mirrors.Remove(mirror);
        }
    }

    public void AddMirror(MirrorData newMirror)
    {
        mirrors.Add(newMirror);
        newMirror.ResetMirror();
    }

    public MirrorData GetCurrentActiveMirror()
    {
        return currentActiveMirror;
    }

    public List<MirrorData> GetAllMirrors()
    {
        return mirrors;
    }

    // ===========================================
    // 디버그용 메서드들
    // ===========================================

    void OnDrawGizmos()
    {
        if (player == null) return;

        foreach (var mirror in mirrors)
        {
            if (mirror.mirror == null) continue;

            float distance = Vector3.Distance(player.position, mirror.mirror.position);

            if (distance <= detectionRange)
            {
                Gizmos.color = mirror.isActive ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(mirror.mirror.position, detectionRange);

                if (mirror.isActive)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(player.position, mirror.mirror.position);
                }
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), $"Active Mirror: {(currentActiveMirror?.mirror?.name ?? "None")}");

        if (currentActiveMirror != null)
        {
            GUI.Label(new Rect(10, 40, 200, 30), $"State: {currentActiveMirror.currentState}");
            GUI.Label(new Rect(10, 70, 200, 30), $"Stage: {currentActiveMirror.currentStage}");
            GUI.Label(new Rect(10, 100, 200, 30), $"Stay Timer: {currentActiveMirror.stayTimer:F1}");
        }

        GUI.Label(new Rect(10, 130, 200, 30), $"Total Mirrors: {mirrors.Count}");
    }
}