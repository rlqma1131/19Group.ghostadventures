using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 거울 데이터 클래스
//설정 가이드:
//1.거울 목록(Mirrors)

//Size: 스테이지에 있는 거울 개수만큼 설정
//각 Element마다 개별 거울 설정

//2. 거울별 설정 (각 Element)

//Mirror: 실제 거울 Transform (필수)
//Ghost Visual: 유령 모델/이펙트 오브젝트
//Audio Source: 해당 거울의 오디오 소스
//Stage Sounds: 1~5단계별 효과음(5개)
//Stage Visuals: 1~5단계별 비주얼 오브젝트 (5개)

//3. 기능 설정

//Detection Range: 거울 감지 범위 (미터)
//Stay Threshold: 감지 시작 시간 (초)
//Stage Progress Time: 단계 진행 간격 (초)

//4. 게임 설정

//Player Camera: 플레이어 카메라(화면 효과용)

//팁:

//거울이 3개면 Size를 3으로 설정
//Stage Sounds/Visuals는 항상 5개로 고정
//상태 정보는 런타임에 자동으로 업데이트됨
[System.Serializable]
public class MirrorData
{
    [Header("거울 설정")]
    public Transform mirror;
    public GameObject ghostVisual;
    public AudioSource audioSource;
    public AudioClip[] stageSounds;
    public GameObject[] stageVisuals;

    [Header("상태 정보")]
    public GhostState currentState = GhostState.Waiting;
    public int currentStage = 0;
    public float stayTimer = 0f;
    public float stageTimer = 0f;
    public bool isActive = false;

    public void ResetMirror()
    {
        currentState = GhostState.Waiting;
        currentStage = 0;
        stayTimer = 0f;
        stageTimer = 0f;
        isActive = false;

        if (ghostVisual != null)
            ghostVisual.SetActive(false);

        foreach (var visual in stageVisuals)
        {
            if (visual != null)
                visual.SetActive(false);
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

    private Transform player;
    private MirrorData currentActiveMirror = null;
    private bool isPlayerControlDisabled = false;

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
        // 가장 가까운 거울 찾기
        MirrorData closestMirror = GetClosestMirror();

        // 현재 활성 거울이 범위를 벗어났거나, 더 가까운 거울이 있으면 변경
        if (currentActiveMirror != closestMirror)
        {
            if (currentActiveMirror != null)
            {
                // 이전 거울 비활성화
                DeactivateMirror(currentActiveMirror);
            }

            currentActiveMirror = closestMirror;

            if (currentActiveMirror != null)
            {
                // 새 거울 활성화
                ActivateMirror(currentActiveMirror);
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
        // 플레이어 시선 방향 계산
        Vector3 playerForward = player.forward;
        Vector3 toMirror = (mirror.mirror.position - player.position).normalized;
        float dotProduct = Vector3.Dot(playerForward, toMirror);

        bool isLookingAtMirror = dotProduct > 0.5f;
        bool isBackTurned = dotProduct < -0.3f;

        UpdateMirrorState(mirror, isLookingAtMirror, isBackTurned);
    }

    void UpdateMirrorState(MirrorData mirror, bool isLookingAtMirror, bool isBackTurned)
    {
        switch (mirror.currentState)
        {
            case GhostState.Waiting:
                HandleWaitingState(mirror);
                break;
            case GhostState.Observing:
                HandleObservingState(mirror, isLookingAtMirror, isBackTurned);
                break;
            case GhostState.Preparing:
                HandlePreparingState(mirror, isLookingAtMirror);
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

    void HandleWaitingState(MirrorData mirror)
    {
        mirror.stayTimer += Time.deltaTime;

        if (mirror.stayTimer >= stayThreshold)
        {
            ChangeMirrorState(mirror, GhostState.Observing);
            StartStageProgression(mirror);
        }
    }

    void HandleObservingState(MirrorData mirror, bool isLookingAtMirror, bool isBackTurned)
    {
        if (isLookingAtMirror)
        {
            OnPlayerLookingAtMirror(mirror);
        }
        else if (isBackTurned)
        {
            ChangeMirrorState(mirror, GhostState.Preparing);
        }
    }

    void HandlePreparingState(MirrorData mirror, bool isLookingAtMirror)
    {
        mirror.stageTimer += Time.deltaTime;

        if (mirror.stageTimer >= stageProgressTime)
        {
            ProgressToNextStage(mirror);
            mirror.stageTimer = 0f;
        }

        if (isLookingAtMirror)
        {
            ChangeMirrorState(mirror, GhostState.Observing);
        }
    }

    void HandleEmergingState(MirrorData mirror)
    {
        StartCoroutine(EmergingSequence(mirror));
    }

    void HandleDeathCutscene(MirrorData mirror)
    {
        DisablePlayerControl();
        StartCoroutine(DeathSequence(mirror));
    }

    void StartStageProgression(MirrorData mirror)
    {
        mirror.currentStage = 1;
        PlayStageEffect(mirror, mirror.currentStage);
    }

    void ProgressToNextStage(MirrorData mirror)
    {
        mirror.currentStage++;

        if (mirror.currentStage <= 5)
        {
            PlayStageEffect(mirror, mirror.currentStage);
        }
        else if (mirror.currentStage == 6)
        {
            ChangeMirrorState(mirror, GhostState.Emerging);
        }
    }

    void PlayStageEffect(MirrorData mirror, int stage)
    {
        // 사운드 재생
        if (mirror.audioSource != null && mirror.stageSounds != null && stage <= mirror.stageSounds.Length)
        {
            mirror.audioSource.clip = mirror.stageSounds[stage - 1];
            mirror.audioSource.Play();
        }

        // 비주얼 효과
        if (mirror.stageVisuals != null && stage <= mirror.stageVisuals.Length)
        {
            for (int i = 0; i < mirror.stageVisuals.Length; i++)
            {
                if (mirror.stageVisuals[i] != null)
                    mirror.stageVisuals[i].SetActive(i == stage - 1);
            }
        }

        // 단계별 특수 효과
        switch (stage)
        {
            case 1:
                OnStage1(mirror);
                break;
            case 2:
                OnStage2(mirror);
                break;
            case 3:
                OnStage3(mirror);
                break;
            case 4:
                OnStage4(mirror);
                break;
            case 5:
                OnStage5(mirror);
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
        OnMirrorBreak(mirror);
        yield return new WaitForSeconds(1f);

        OnGhostEmerge(mirror);
        yield return new WaitForSeconds(0.5f);

        ChangeMirrorState(mirror, GhostState.DeathCutscene);
    }

    IEnumerator DeathSequence(MirrorData mirror)
    {
        OnPlayerDeath(mirror);
        yield return new WaitForSeconds(3f);

        ResetGame();
    }

    void DisablePlayerControl()
    {
        isPlayerControlDisabled = true;
        OnPlayerControlDisabled();
    }

    void EnablePlayerControl()
    {
        isPlayerControlDisabled = false;
        OnPlayerControlEnabled();
    }

    void ResetGame()
    {
        InitializeAllMirrors();
        currentActiveMirror = null;
        EnablePlayerControl();
        OnGameReset();
    }

    // ===========================================
    // 연출팀에서 구현할 이벤트 메서드들
    // ===========================================

    void OnMirrorActivated(MirrorData mirror)
    {
        // 거울 활성화 시 연출
        Debug.Log($"거울 활성화: {mirror.mirror.name}");
    }

    void OnMirrorDeactivated(MirrorData mirror)
    {
        // 거울 비활성화 시 연출
        Debug.Log($"거울 비활성화: {mirror.mirror.name}");
    }

    void OnPlayerLookingAtMirror(MirrorData mirror)
    {
        // 플레이어가 거울을 보고 있을 때 연출
    }

    void OnStateChanged(MirrorData mirror, GhostState newState)
    {
        // 상태 변경 시 연출
        Debug.Log($"거울 {mirror.mirror.name} 상태 변경: {newState}");
    }

    void OnStage1(MirrorData mirror) { }
    void OnStage2(MirrorData mirror) { }
    void OnStage3(MirrorData mirror) { }
    void OnStage4(MirrorData mirror) { }
    void OnStage5(MirrorData mirror) { }

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