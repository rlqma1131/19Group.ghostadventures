using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerLifeManager : MonoBehaviour
{
    // 싱글톤
    public static PlayerLifeManager Instance {  get; private set; }

    [Header("생명 시스템 설정")]
    public int maxPlayerLives = 2; //  이것만 바꿔주면 , 판당 목숨도 알아서 조절되고 QTE 시스템도 알아서 작동할것입니다. 
    public int currentPlayerLives;

    // 이벤트 시스템 - 다른 스크립트들이 구독할 수 있음
    public static event Action<int> OnLifeChanged; // 생명이 변경될 때
    public static event Action OnGameOver; // 게임오버 시
    public static event Action OnLifeLost; // 생명을 잃었을 때 (스턴 처리용)
    private Animator playerAnimator;
    private EnemyVolumeTrigger volume; // EnemyVolumeTrigger 컴포넌트
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Player는 이미 GameManager에서 DontDestroyOnLoad 처리됨
        }
        else
        {
            Destroy(this); // 컴포넌트만 제거
        }
        playerAnimator = GetComponentInChildren<Animator>();

    }

    private void Start()
    {
        currentPlayerLives = maxPlayerLives; // 생명 초기화
        volume = GetComponentInChildren<EnemyVolumeTrigger>();
        OnLifeChanged?.Invoke(currentPlayerLives); // UI 업데이트용
    }

    // ================================
    // 생명 시스템 관련 메서드들
    // ================================

    public void LosePlayerLife()
    {
        SubtractionLife();
        OnLifeChanged?.Invoke(currentPlayerLives);
        // PossessionSystem.Instance.CanMove = false;

        // TODO: 피격 애니메이션 재생
       // playerAnimator?.SetTrigger("StruggleIn");  // <- Animator에 "StruggleIn" 트리거 설정 필요

        if (currentPlayerLives <= 0)
        {
            HandleGameOver();
        }
        else
        {
            OnLifeLost?.Invoke(); // AI에게 알림
        }
    }

    public void HandleGameOver()
    {
        var qteUI = FindObjectOfType<QTEUI2>();
        if (qteUI != null)
            qteUI.gameObject.SetActive(false);
        
        UIManager.Instance.PlayModeUI_CloseAll();
        GameObject player = GameManager.Instance.Player;
        PlayableDirector director = player.GetComponentInChildren<PlayableDirector>();
        if (volume != null)
        {
            volume.Ondead = true;
        }
        else
        {
            Debug.LogWarning("EnemyVolumeTrigger가 PlayerLifeManager에서 발견되지 않았습니다.");
        }
        if (volume != null && volume.globalVolume.profile != null)
        {
            if (volume.globalVolume.profile.TryGet<ColorAdjustments>(out var ca))
            {
                ca.colorFilter.value = volume.farColor; // EnemyVolumeTrigger에 정의된 farColor로 복원
            }
        }

        if (director != null)
        {
            director.stopped += ResetIsdead; 
            director.Play();
        }

        if (volume != null)
        {
            volume.enabled = false; // 볼륨 효과 활성화
        }
        //UIManager.Instance.startEndingUI_OpenAll(); // 게임오버 UI 표시(테스트용)
        //추후 onGameOver 이벤트에 연결예정

        // 임시로 게임 일시정지
        // Time.timeScale = 0f;
    }

    // ================================
    // 공개 메서드들 (외부에서 호출 가능)
    // ================================

    // 현재 생명 수 반환
    public int GetCurrentLives()
    {
        return currentPlayerLives;
    }

    // 생명 리셋 (챕터 시작 시 호출)
    public void ResetLives()
    {
        currentPlayerLives = maxPlayerLives;
        OnLifeChanged?.Invoke(currentPlayerLives);
        Debug.Log("생명이 리셋되었습니다.");
    }

    // 생명 추가 (아이템 등으로 회복할 때)
    public void AddLife(int amount = 1)
    {
        currentPlayerLives = Mathf.Min(currentPlayerLives + amount, maxPlayerLives);
        OnLifeChanged?.Invoke(currentPlayerLives);
        Debug.Log($"생명 회복! 현재 생명: {currentPlayerLives}");
    }

    // 생명 감소
    public void SubtractionLife()
    {
        currentPlayerLives--;
        Debug.Log($"생명 감소! 남은 생명: {currentPlayerLives}");
    }

    // 최대 생명 설정 (난이도별로 다를 때)
    public void SetMaxLives(int newMaxLives)
    {
        maxPlayerLives = newMaxLives;
        currentPlayerLives = Mathf.Min(currentPlayerLives, maxPlayerLives);
        OnLifeChanged?.Invoke(currentPlayerLives);
    }

    public void StartStruggleAnimation()
    {
        playerAnimator?.SetTrigger("StruggleIn");
    }

    public void StopStruggleAnimation()
    {
        playerAnimator?.SetTrigger("StruggleOut");
    }

    public void ResetIsdead(PlayableDirector director)
    {
        UIManager.Instance.QTE_UI_2.isdead = false; // QTE UI 상태 초기화

    }
    
}