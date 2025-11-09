using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Player;
using AYellowpaper.SerializedCollections;
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
    [SerializeField] private GameObject saveStateApplier;
    [SerializeField] private GameObject eventManager;

    [Header("Player References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject currentPlayer;
    [SerializeField] private Player player;
    
    [Header("Required memory ids in each chapter")]
    [SerializeField] SerializedDictionary<MemoryData.Chapter, List<string>> memoryIds = new();

    public GameObject PlayerObj => currentPlayer;
    public Player Player => player;
    public PlayerController PlayerController => player.Controller;
    public SerializedDictionary<MemoryData.Chapter, List<string>> MemoryIds => memoryIds;
    public bool ByPassEnabled { get; set; }
    
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
        
        bool IsChapterStart(string name)
        {
            return name == "Ch01_House" || name == "Ch02_PlayGround" || name == "Ch03_Hospital";
        }
        
        if (IsChapterStart(sceneName) && !SceneLoadContext.CameThroughLoading)
        {
            Debug.Log($"[GameManager] 챕터 '{sceneName}'가 로딩씬 없이 열림 → 로딩씬 경유로 재진입");
            LoadThroughLoading(sceneName);
            return; // 이하 초기화/스폰 실행 안 함
        }
        
        if (sceneName != "LoadingScene")
            SceneLoadContext.CameThroughLoading = false;
        
        EnsureManagerExists<SaveStateApplier>(saveStateApplier);

        if (sceneName != "StartScene"
            && sceneName != "IntroScene_Real"
            && mode != LoadSceneMode.Additive
            && sceneName != "Ch01_To_Ch02"
            && sceneName != "Ch02_To_Ch03"
            && sceneName != "Ch03_To_Ch04" 
            && sceneName != "Ch03_Memory01"
            && sceneName != "LoadingScene"
            && sceneName != "End_Exit"
            && sceneName != "End_분기"
            && sceneName != "End_인정"
            )

        {
            // 플레이모드 UI 열기
            if (UIManager.Instance != null)
                UIManager.Instance.PlayModeUI_OpenAll(); 

            TrySpawnPlayer();

            // 현재 씬 이름과 플레이어 위치 저장
            Vector3 playerPos = currentPlayer != null ? currentPlayer.transform.position : Vector3.zero;
            SaveManager.SetSceneAndPosition(sceneName, playerPos);

            // 바로 저장 실행
            SaveManager.SaveGame();
        }

        if ((sceneName == "StartScene" || sceneName == "End_Exit" || sceneName == "End_인정" || sceneName == "End_분기"))
        {
            if(currentPlayer != null)
            {
                Debug.Log("[GameManager] StartScene 로드됨 - Player 제거");
                Destroy(currentPlayer);
                currentPlayer = null;
                player = null;
            }
            return;
        }

        var chapterEndingManagerComp = EnsureManagerExists<ChapterEndingManager>(chapterEndingManager);
        var uiManagerComp = EnsureManagerExists<UIManager>(uiManager);
        var possessionStateManagerComp = EnsureManagerExists<PossessionStateManager>(possessionStateManager);
        var soundManagerComp = EnsureManagerExists<SoundManager>(soundManager);
        var cutsceneManagerComp = EnsureManagerExists<Global_CutsceneManager>(cutSceneManager);
        var qteEffectManagerComp = EnsureManagerExists<QTEEffectManager>(qteEffectManager);
        var tutorialManagerComp = EnsureManagerExists<TutorialManager>(tutorialManager);
        var eventManagerComp = EnsureManagerExists<EventManager>(eventManager);

        if (player) {
            tutorialManagerComp.Initialize_Player(player);
            possessionStateManagerComp.Initialize_Player(player);
            qteEffectManagerComp.Initialize_Player(player);
            uiManagerComp.Initialize_Player(player);
        }

        // 퍼즐 진척도 UI ( 씬에 맞게 로드 )
        UIManager.Instance.AutoSelectPuzzleStatusByScene();
        
        Debug.Log($"씬 로드됨: {scene.name}");
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

        // 이어하기 했을 때
        if (loadFromSave && pendingSaveData != null)
        {
            spawnPosition = pendingSaveData.playerPosition.ToVector3();
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
        currentPlayer = go;
        player = go.GetComponent<Player>();
        DontDestroyOnLoad(go);

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
            player = null;
            Debug.Log("[GameManager] Player 파괴됨");
        }
    }

    private T EnsureManagerExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        if (Singleton<T>.Instance != null) return Singleton<T>.Instance;
        
        GameObject obj = Instantiate(prefab);
        Debug.Log($"[{typeof(T).Name}] 자동 생성됨");
        return obj.GetComponent<T>();
    }
    
    public static ClueStage GetStageForCurrentChapter()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "Ch01_House": return ClueStage.Stage1;
            case "Ch02_PlayGround": return ClueStage.Stage2;
            case "Ch03_Hospital": return ClueStage.Stage3;
            default: return ClueStage.Stage4;
        }
    }
    
    public static void LoadThroughLoading(string nextScene, Color? bgBaseColor = null)
    {
        SceneLoadContext.RequestedNextScene = nextScene;
        SceneLoadContext.RequestedBaseBgColor = bgBaseColor;
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
    }
}
