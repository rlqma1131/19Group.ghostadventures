using System;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerLifeManager : MonoBehaviour
{
    [Header("Player Life Settings")]
    [SerializeField] int maxPlayerLives = 5; //  이것만 바꿔주면 , 판당 목숨도 알아서 조절되고 QTE 시스템도 알아서 작동할것입니다. 
    [SerializeField] int currentPlayerLives;

    [Header("Timeline Reference")] 
    [SerializeField] PlayableDirector director;
    [SerializeField] List<ParticleSystem> particleSystems;
    
    Player player;
    
    // 이벤트 시스템 - 다른 스크립트들이 구독할 수 있음
    public static event Action<int> OnLifeChanged; // 생명이 변경될 때

    void Awake() => Initialize_Comp();

    void Reset() => Initialize_Comp();

    void Initialize_Comp() {
        if (!director) director = GetComponentInChildren<PlayableDirector>();
        if (particleSystems.Count <= 0) particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
    }

    public void Initialize(Player player) {
        this.player = player;
        currentPlayerLives = maxPlayerLives; // 생명 초기화
        OnLifeChanged += value => {
            if (value <= 0) HandleGameOver();
        };
    }

    // ================================
    // 생명 시스템 관련 메서드들
    // ================================

    public void LosePlayerLife() => SubtractionLife();

    public void SuddenDeath() {
        while(currentPlayerLives <= 0) SubtractionLife();
    }

    public void HandleGameOver()
    {
        UIManager.Instance.QTE_UI_2.gameObject.SetActive(false);
        UIManager.Instance.Inventory_PlayerUI.RemoveClueBeforeStage();
        UIManager.Instance.PlayModeUI_CloseAll();
        SoundManager.Instance.FadeOutAndStopLoopingSFX();
        
        if (EnemyVolumeOverlay.Instance) EnemyVolumeOverlay.Instance.Suspend(true);

        if (director) {
            player.PossessionSystem.CanMove = false; // 플레이어 이동 비활성화
            
            // Register Events
            director.stopped += ResetState;
            // Stop Particles
            foreach (var ps in particleSystems) ps?.Stop();
            // Play Timeline
            director.Play();
        }
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
    public void ResetLives() {
        currentPlayerLives = maxPlayerLives;
        OnLifeChanged?.Invoke(currentPlayerLives);
        Debug.Log("생명이 리셋되었습니다.");
    }

    // 생명 추가 (아이템 등으로 회복할 때)
    public void AddLife(int amount = 1) {
        currentPlayerLives = Mathf.Min(currentPlayerLives + amount, maxPlayerLives);
        OnLifeChanged?.Invoke(currentPlayerLives);
        Debug.Log($"생명 회복! 현재 생명: {currentPlayerLives}");
    }

    // 생명 감소
    public void SubtractionLife() {
        currentPlayerLives--;
        OnLifeChanged?.Invoke(currentPlayerLives);
        Debug.Log($"생명 감소! 남은 생명: {currentPlayerLives}");
    }

    // 최대 생명 설정 (난이도별로 다를 때)
    public void SetMaxLives(int newMaxLives) {
        maxPlayerLives = newMaxLives;
        currentPlayerLives = Mathf.Min(currentPlayerLives, maxPlayerLives);
        OnLifeChanged?.Invoke(currentPlayerLives);
    }

    public void ResetState(PlayableDirector director) {
        UIManager.Instance.QTE_UI_2.isdead = false; // QTE UI 상태 초기화
        player.PossessionSystem.CanMove = true; // 플레이어 이동 활성화

        if (EnemyVolumeOverlay.Instance != null)
            EnemyVolumeOverlay.Instance.Suspend(false);
    }
}