using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Managers")]
    [SerializeField] private GameObject ChapterEndingManager;
    [SerializeField] private GameObject uiManager;
    [SerializeField] private GameObject PossessionStateManager;
    [SerializeField] private GameObject soundManager;
    [SerializeField] private GameObject cutSceneManager;
    [SerializeField] private GameObject qteEffectManager; // QTE 이펙트 매니저 추가

    // 디버깅용 (추후 삭제 예정)
    [SerializeField] private GameObject HideAreaManager;

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
        }
    }

    public GameObject Player => currentPlayer;
    public PlayerController PlayerController => playerController;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}");

        EnsureManagerExists<ChapterEndingManager>(ChapterEndingManager);
        EnsureManagerExists<UIManager>(uiManager);
        EnsureManagerExists<PossessionStateManager>(PossessionStateManager);
        EnsureManagerExists<SoundManager>(soundManager);
        EnsureManagerExists<CutsceneManager>(cutSceneManager);
        EnsureManagerExists<QTEEffectManager>(qteEffectManager); // QTE 이펙트 매니저 추가
        EnsureManagerExists<Ch1_HideAreaEvent>(HideAreaManager); // 디버깅용, 추후 삭제 예정

        // 다른 매니저들도 같은 방식으로
        // 추후 스테이지 초기화, UI 초기화 등 여기에 추가
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