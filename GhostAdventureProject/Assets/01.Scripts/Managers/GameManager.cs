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

            Debug.Log("[GameManager] Player 스폰 완료 - EnemyAI는 자동으로 GameManager에서 참조");
        }
    }

    /// <summary>
    /// Player가 파괴될 때 호출
    /// </summary>
    public void OnPlayerDestroyed()
    {
        if (currentPlayer != null)
        {
            currentPlayer = null;
            playerController = null;

            Debug.Log("[GameManager] Player 파괴됨");
        }
    }

    public GameObject Player => currentPlayer;
    public PlayerController PlayerController => playerController;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}");

        EnsureManagerExists<ChapterEndingManager>(chapterEndingManager);
        EnsureManagerExists<UIManager>(uiManager);
        EnsureManagerExists<PossessionStateManager>(possessionStateManager);
        EnsureManagerExists<SoundManager>(soundManager);
        EnsureManagerExists<CutsceneManager>(cutSceneManager);
        EnsureManagerExists<QTEEffectManager>(qteEffectManager);

        Debug.Log("[GameManager] 씬 로드 완료 - EnemyAI는 자동으로 Player 찾음");
    }

    private void EnsureManagerExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (Singleton<T>.Instance == null)
        {
            Instantiate(prefab);
            Debug.Log($"[{typeof(T).Name}] 자동 생성됨");
        }
    }
}