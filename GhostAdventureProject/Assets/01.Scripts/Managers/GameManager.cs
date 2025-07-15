using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Managers")]
    [SerializeField] private GameObject chapterEndingManager;
    [SerializeField] private GameObject uiManager;
    [SerializeField] private GameObject possessionStateManager;
    [SerializeField] private GameObject soundManager;
    [SerializeField] private GameObject cutSceneManager;
    [SerializeField] private GameObject qteEffectManager;

    public GameObject playerPrefab;
    private GameObject currentPlayer;
    private PlayerController playerController;

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (currentPlayer == null)
        {
            GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            currentPlayer = go;
            playerController = go.GetComponent<PlayerController>();
            DontDestroyOnLoad(go);

            // Player 생성 시 EnemyAI와 StealthDoor에 알림
            NotifyPlayerSpawned(go);
        }
    }

    /// <summary>
    /// Player가 생성되었을 때 모든 관련 컴포넌트에 알림
    /// </summary>
    private void NotifyPlayerSpawned(GameObject player)
    {
        // EnemyAI 캐시 업데이트
        EnemyAI.UpdatePlayerReference(player.transform);

        // StealthDoor 캐시에 Player 추가
        StealthDoor.AddNewTarget(player, "Player");

        Debug.Log("[GameManager] Player 스폰 완료 - 모든 시스템에 알림 전송됨");
    }

    /// <summary>
    /// Player가 파괴될 때 호출
    /// </summary>
    public void OnPlayerDestroyed()
    {
        if (currentPlayer != null)
        {
            // 캐시에서 Player 제거
            EnemyAI.UpdatePlayerReference(null);
            StealthDoor.RemoveTarget(currentPlayer);

            currentPlayer = null;
            playerController = null;

            Debug.Log("[GameManager] Player 파괴됨 - 모든 시스템에 알림 전송됨");
        }
    }

    public GameObject Player => currentPlayer;
    public PlayerController PlayerController => playerController;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}");

        // 씬 전환 시 모든 캐시 초기화
        ClearAllCaches();

        EnsureManagerExists<ChapterEndingManager>(chapterEndingManager);
        EnsureManagerExists<UIManager>(uiManager);
        EnsureManagerExists<PossessionStateManager>(possessionStateManager);
        EnsureManagerExists<SoundManager>(soundManager);
        EnsureManagerExists<CutsceneManager>(cutSceneManager);
        EnsureManagerExists<QTEEffectManager>(qteEffectManager);

        // Player가 이미 존재한다면 새 씬에서도 인식하도록 알림
        if (currentPlayer != null)
        {
            NotifyPlayerSpawned(currentPlayer);
        }

        Debug.Log("[GameManager] 씬 로드 완료 - 모든 캐시 초기화됨");
    }

    /// <summary>
    /// 모든 최적화 캐시 초기화
    /// </summary>
    private void ClearAllCaches()
    {
        // EnemyAI 캐시 초기화
        EnemyAI.ClearAllCaches();

        // StealthDoor 캐시 초기화
        StealthDoor.ForceUpdateCache();

        Debug.Log("[GameManager] 모든 성능 최적화 캐시가 초기화되었습니다");
    }

    private void EnsureManagerExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (Singleton<T>.Instance == null)
        {
            Instantiate(prefab);
            Debug.Log($"[{typeof(T).Name}] 자동 생성됨");
        }
    }

    /// <summary>
    /// 게임 종료 시 정리
    /// </summary>
    private void OnApplicationQuit()
    {
        ClearAllCaches();
    }

    /// <summary>
    /// Cat 오브젝트가 생성되었을 때 호출하는 메서드
    /// </summary>
    public void RegisterCat(GameObject cat)
    {
        StealthDoor.AddNewTarget(cat, "Cat");
        Debug.Log($"[GameManager] Cat 등록됨: {cat.name}");
    }

    /// <summary>
    /// Cat 오브젝트가 파괴되었을 때 호출하는 메서드
    /// </summary>
    public void UnregisterCat(GameObject cat)
    {
        StealthDoor.RemoveTarget(cat);
        Debug.Log($"[GameManager] Cat 등록 해제됨: {cat.name}");
    }
}