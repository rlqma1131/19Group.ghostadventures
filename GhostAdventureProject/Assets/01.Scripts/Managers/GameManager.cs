using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// 디버깅용으로 저장 데이터 삭제 기능
    /// P키
    /// </summary>
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
    [Header("Managers")]
    [SerializeField] private GameObject chapterEndingManager;
    [SerializeField] private GameObject uiManager;
    [SerializeField] private GameObject possessionStateManager;
    [SerializeField] private GameObject soundManager;
    [SerializeField] private GameObject cutSceneManager;
    [SerializeField] private GameObject qteEffectManager;
    [SerializeField] private GameObject tutorialManager;
    
    public GameObject playerPrefab;

    [SerializeField] private GameObject currentPlayer;
    private PlayerController playerController;

    public GameObject Player => currentPlayer;
    public PlayerController PlayerController => playerController;


    // 게임 이어하기
    private bool loadFromSave = false;
    private SaveData pendingSaveData;

    protected override void Awake()
    {
        base.Awake();
        string sceneName = SceneManager.GetActiveScene().name;
        Application.runInBackground = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 플레이모드 UI 닫기
        if (UIManager.Instance != null)
            UIManager.Instance.PlayModeUI_CloseAll(); 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName != "StartScene" && sceneName != "IntroScene_Real" 
            && mode != LoadSceneMode.Additive && sceneName != "Ch01_To_Ch02" && sceneName != "Ch02_To_Ch03" && sceneName != "Ch03_To_Ch04" && sceneName != "Ch03_Memory01")
        {
            // 플레이모드 UI 열기
            if (UIManager.Instance != null)
                UIManager.Instance.PlayModeUI_OpenAll(); 

            TrySpawnPlayer();
        }

        if (sceneName == "StartScene")
        {
            Debug.Log("[GameManager] StartScene 로드됨 - Player 제거");
            Destroy(currentPlayer);
            currentPlayer = null;
            playerController = null;
            return;
        }

        EnsureManagerExists<ChapterEndingManager>(chapterEndingManager);
        EnsureManagerExists<UIManager>(uiManager);
        EnsureManagerExists<PossessionStateManager>(possessionStateManager);
        EnsureManagerExists<SoundManager>(soundManager);
        EnsureManagerExists<CutsceneManager>(cutSceneManager);
        EnsureManagerExists<QTEEffectManager>(qteEffectManager);
        EnsureManagerExists<TutorialManager>(tutorialManager);

        Debug.Log($"씬 로드됨: {scene.name}");

        // 이어하기에 저장한 데이터 적용
        //if (loadFromSave && pendingSaveData != null)
        //{
        //    ApplySaveData(pendingSaveData);
        //}
    }

    //public void SetPendingLoad(SaveData data)
    //{
    //    loadFromSave = true;
    //    pendingSaveData = data;
    //}

    //private void ApplySaveData(SaveData data)
    //{
    //    //  Clue 복원
    //    var inventory = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
    //    inventory.RemoveClueBeforeStage();
    //    foreach (string clueName in data.collectedClueNames)
    //    {
    //        ClueData clue = Resources.Load<ClueData>("ClueData/" + clueName);
    //        if (clue != null)
    //            inventory.AddClue(clue);
    //    }

    //    // Memory 복원
    //    MemoryManager.Instance.ClearScannedDebug();
    //    foreach (string memoryID in data.collectedMemoryIDs)
    //    {
    //        MemoryData memory = Resources.Load<MemoryData>("MemoryData/" + memoryID);
    //        if (memory != null)
    //            MemoryManager.Instance.TryCollect(memory);
    //    }

    //    foreach (string title in data.scannedMemoryTitles)
    //    {
    //        MemoryData memory = Resources.LoadAll<MemoryData>("MemoryData")
    //            .FirstOrDefault(m => m.memoryTitle == title);
    //        if (memory != null && !MemoryManager.Instance.IsCanStore(memory))
    //            MemoryManager.Instance.TryCollect(memory);
    //    }

    //    Debug.Log("[GameManager] 저장된 인벤토리 및 기억 복원 완료");
    //}

    public void TrySpawnPlayer()
    {
        if (currentPlayer != null)
        {
            Debug.LogWarning("[GameManager] 이미 플레이어가 존재합니다.");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;

        // 이어하기 했을 때
        if (loadFromSave && pendingSaveData != null)
        {
            spawnPosition = pendingSaveData.playerPosition;
            Debug.Log($"[GameManager] 이어하기 위치에서 스폰: {spawnPosition}");
        }
        // 새로 시작 or 씬 로드
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

        // 저장데이터 초기화
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

    //private void RunPlayer()
    //{
    //    if (currentPlayer != null)
    //    {
    //        PossessionSystem.Instance.CanMove = true;
    //    }
    //}

    //private void PausePlayer() 
    //{ 
    //    if (currentPlayer != null)
    //    {
    //        PossessionSystem.Instance.CanMove = false;
    //    }
    //}
}
