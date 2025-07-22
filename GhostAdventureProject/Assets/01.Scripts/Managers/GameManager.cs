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

    public GameObject Player => currentPlayer;
    public PlayerController PlayerController => playerController;


    // 게임 이어하기
    private bool loadFromSave = false;
    private SaveData pendingSaveData;

    protected override void Awake()
    {
        base.Awake();

        // StartScene에서는 플레이어 스폰 안함
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            TrySpawnPlayer();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == "StartScene")
        {
            Debug.Log("[GameManager] StartScene 로드됨 - Player 제거");
            Destroy(currentPlayer);
            currentPlayer = null;
            playerController = null;
            return;
        }

        Debug.Log($"씬 로드됨: {scene.name}");

        EnsureManagerExists<ChapterEndingManager>(chapterEndingManager);
        EnsureManagerExists<UIManager>(uiManager);
        EnsureManagerExists<PossessionStateManager>(possessionStateManager);
        EnsureManagerExists<SoundManager>(soundManager);
        EnsureManagerExists<CutsceneManager>(cutSceneManager);
        EnsureManagerExists<QTEEffectManager>(qteEffectManager);

        TrySpawnPlayer();

        Debug.Log("[GameManager] 씬 로드 완료 - EnemyAI는 자동으로 Player 찾음");
    }

    public void SetPendingLoad(SaveData data)
    {
        loadFromSave = true;
        pendingSaveData = data;
    }

    public void TrySpawnPlayer()
    {
        if (currentPlayer != null)
        {
            Debug.LogWarning("[GameManager] 이미 플레이어가 존재합니다.");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;

        if (loadFromSave && pendingSaveData != null)
        {
            spawnPosition = pendingSaveData.playerPosition;
            Debug.Log($"[GameManager] 이어하기 위치에서 스폰: {spawnPosition}");
        }
        else
        {
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[GameManager] 새로운 씬 로드됨: {sceneName}");
            string startPointName = $"StartPoint_{sceneName}";
            Debug.Log($"[GameManager] 시작 위치 이름: {startPointName}");
            Transform startPoint = GameObject.Find(startPointName)?.transform;
            Debug.Log($"[GameManager] 시작 위치 찾기: {startPoint != null}");
            spawnPosition = startPoint != null ? startPoint.position : Vector3.zero;
            Debug.Log($"[GameManager] 시작 위치에서 스폰: {spawnPosition}");
        }

        GameObject go = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        DontDestroyOnLoad(go);
        currentPlayer = go;
        playerController = go.GetComponent<PlayerController>();

        Debug.Log("[GameManager] Player 스폰 완료");

        // 초기화
        loadFromSave = false;
        pendingSaveData = null;
    }

    public void OnPlayerDestroyed()
    {
        if (currentPlayer != null)
        {
            currentPlayer = null;
            playerController = null;
            Debug.Log("[GameManager] Player 파괴됨");
        }
    }

    private void EnsureManagerExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (Singleton<T>.Instance == null)
        {
            Instantiate(prefab);
            Debug.Log($"[{typeof(T).Name}] 자동 생성됨");
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveManager.DeleteSave();
            Debug.Log("[GameManager] 저장 데이터 삭제됨");
        }
    }
#endif
}
