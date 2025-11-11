using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public interface IUIClosable
{
    // Esc키로 닫을 수 있는 UI는 IUIClosable를 상속받습니다.UIManager - closableUI List에도 추가해줘야 정상작동 됩니다.
    void Close();
    bool IsOpen();
}

public class UIManager : Singleton<UIManager>
{
    // 1. 플레이모드 UI
    [SerializeField] private SoulGauge soulGauge;                   // 영혼에너지 
    [SerializeField] private Prompt prompt;                         // 프롬프트
    [SerializeField] private Prompt_UnPlayMode prompt2;             // 컷신용 프롬프트
    [SerializeField] private QTEUI qte1;                            // QTE
    [SerializeField] private QTEUI2 qte2;                           // QTE2
    [SerializeField] private QTEUI3 qte3;                           // QTE3
    public GameObject scanUI;                                       // 스캔UI
    [SerializeField] private MemoryStorage memoryStorage;           // 기억저장소
    [SerializeField] private Inventory_Player inventory_Player;     // 인벤토리-플레이어
    [SerializeField] private Inventory_PossessableObject inventory_PossessableObject; // 인벤토리-빙의오브젝트
    [SerializeField] private InventoryExpandViewer inventoryExpandViewer; // 인벤토리 확대뷰어
    [SerializeField] private ESCMenu escMenu;                       // ESC 메뉴
    [SerializeField] private NoticePopup noticePopup;               // 알림팝업 (왼쪽 상단)
    [SerializeField] private NoticePopup saveNoticePopup;           // 저장팝업 (오른쪽 하단)
    [SerializeField] private GameObject[] puzzleStatusPanels;       // * 퍼즐상태 (오른쪽 상단)
    [SerializeField] private GameObject qteEffectCanvas;            // QTE 이펙트 캔버스
    [SerializeField] Image fullScreenFadeImage;
    
    // 외부 접근용
    public SoulGauge SoulGaugeUI => soulGauge;
    public Prompt PromptUI => prompt;
    public Prompt_UnPlayMode PromptUI2 => prompt2;
    public QTEUI QTE_UI => qte1;
    public QTEUI2 QTE_UI_2 => qte2;
    public QTEUI3 QTE_UI_3 => qte3;
    public MemoryStorage MemoryStorageUI => memoryStorage;
    public Inventory_Player Inventory_PlayerUI => inventory_Player;
    public Inventory_PossessableObject Inventory_PossessableObjectUI => inventory_PossessableObject;
    public InventoryExpandViewer InventoryExpandViewerUI => inventoryExpandViewer;
    public ESCMenu ESCMenuUI => escMenu;
    public NoticePopup NoticePopupUI => noticePopup;
    public NoticePopup SaveNoticePopupUI => saveNoticePopup;
    public GameObject QTEEffectCanvas => qteEffectCanvas;
    public GameObject memoryStorageButton;  // 기억저장소 버튼
    public GameObject guidButton;           // 가이드버튼
    public GameObject guidBlackPanel;       // 가이드 UI

    // -------------------------------------------------------------------------------------------

    // 2. 게임 시작/엔딩 UI
    public GameObject gameover;     // 게임오버

    // -------------------------------------------------------------------------------------------

    // 3. 상호작용 UI
    public GameObject q_Key;        // Q키
    public GameObject a_Key;        // A키
    public GameObject d_Key;        // D키
    
    // 4. 미니토스트 UI------------------------------------------------------------------------------
    
    public GameObject spacebar_Key; // Space키
    public GameObject unpossessKey; // E키
    public GameObject tabkeyUI;     // Tab키
    
    // --------------------------------------------------------------------------------------------


    [Header("UICanvas 전체 키고 끌때 사용")]
    [SerializeField] public GameObject playModeUI; // 플레이모드 모든 UI(Canvas)
    [SerializeField] private GameObject startEndingUI; // 게임 시작/엔딩 모든 UI(Canvas)
    [SerializeField] private GameObject TutorialUI; // 튜토리얼 모든 UI(Canvas)
    [SerializeField] private GameObject interactUI; // 상호작용 모든 UI(Canvas)


    [Header("ESC 닫기용 UI List")]
    [SerializeField] private List<MonoBehaviour> closableUI; // ESC키로 닫을 모든 UI List
    [SerializeField] private List<GameObject> allUIs; // 모든 UI (현재 미사용)

    // ---------------------------------------------------------------------------------------

    // 커서 관리
    [Header("커서 셋팅")]
    [SerializeField] private Texture2D defaultCursor;   // 기본
    [SerializeField] private Texture2D findClueCursor;  // 단서
    [SerializeField] private Texture2D hideAreaCursor;  // 은신처
    [SerializeField] private Texture2D lockDoorCursor;  // 잠긴문
    [SerializeField] private Texture2D openDoorCursor;  // 열린문
    [SerializeField] private Texture2D moveAbleCursor;  // 움직임가능하다는표시 커서
    [SerializeField] private Texture2D swipeCursor;     // 드래그 가능하다는 표시 커서
    [SerializeField] private Vector2 hotspot = Vector2.zero; // 클릭판정지점

    // -------------------------------------------------------------------------------------------
    public AudioClip clickSound;                        // UI 클릭 사운드
    // -------------------------------------------------------------------------------------------
    
    public void Initialize_Player(Player player) {
        qte3.Initialize(player);
        escMenu.Initialize(player);
        memoryStorage.Initialize(player);
    }
    
    private void Start()
    {
        SetCursor(CursorType.Default);

        if (SceneManager.GetActiveScene().name == "IntroScene_Real"
         || SceneManager.GetActiveScene().name == "Ch01_To_Ch02"
         || SceneManager.GetActiveScene().name == "Ch02_To_Ch03"
         || SceneManager.GetActiveScene().name == "Ch03_To_Ch04"
         || SceneManager.GetActiveScene().name == "StartScene"
         || SceneManager.GetActiveScene().name == "End_Exit"
         || SceneManager.GetActiveScene().name == "End_분기"
         || SceneManager.GetActiveScene().name == "End_인정")
        {
            PlayModeUI_CloseAll();
        }


        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => ButtonClickSound());
        }
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TryCloseTopUI();

        // if (Input.GetMouseButtonDown(0)) // ** 어떤 오브젝트가 클릭되는지 확인할때 사용 **
        // {
        //     PointerEventData pointerData = new PointerEventData(eventSystem)
        //     {
        //         position = Input.mousePosition
        //     };

        //     List<RaycastResult> results = new List<RaycastResult>();
        //     EventSystem.current.RaycastAll(pointerData, results);

        //     if (results.Count > 0)
        //     {
        //         GameObject uiObject = results[0].gameObject;
        //         Canvas parentCanvas = uiObject.GetComponentInParent<Canvas>();

        //         Debug.Log($"✅ 감지된 UI 오브젝트: {uiObject.name}");

        //         if (parentCanvas != null)
        //         {
        //             Debug.Log($"↳ 이 오브젝트는 Canvas '{parentCanvas.name}' 소속입니다.");
        //         }
        //         else
        //         {
        //             Debug.Log("⚠ 감지된 UI는 Canvas에 속해 있지 않습니다.");
        //         }

        //         return; // UI 감지되었으면 여기서 종료
        //     }

        //     // 2D 오브젝트 감지
        //     Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        //     if (hit.collider != null)
        //     {
        //         Debug.Log("🎯 2D 오브젝트 감지됨: " + hit.collider.gameObject.name);
        //     }
        //     else
        //     {
        //         Debug.Log("❌ 아무것도 감지되지 않음!");
        //     }
        // }
    }

    // 커서 바꾸기
    public enum CursorType {Default, FindClue, HideArea, LockDoor, OpenDoor}
    public void SetCursor(CursorType type, Texture2D customCursor = null)
    {
        switch(type)
        {
            case CursorType.Default:
            customCursor = defaultCursor;
            break;
            
            case CursorType.FindClue:
            customCursor = findClueCursor;
            break;

            case CursorType.HideArea:
            customCursor = hideAreaCursor;
            break;

            case CursorType.LockDoor:
            customCursor = lockDoorCursor;
            break;

            case CursorType.OpenDoor:
            customCursor = openDoorCursor;
            break;
        }
        
        Cursor.SetCursor(customCursor, hotspot, CursorMode.Auto);
    }    

    // 플레이모드UI 모두 켜기
    public void PlayModeUI_OpenAll()
    {
        playModeUI.SetActive(true);
        Debug.Log("플레이모드UI 켜기");
    }
    
    // 플레이모드UI Canvas 끄기
    public void PlayModeUI_CloseAll()
    {
        playModeUI.SetActive(false);
        Debug.Log("플레이모드UI 끄기");
    }
    
    // FadeOut/In Function
    public void FadeOutIn(float fadeDuration = 2f, Action onStart = null, Action onProcess = null, Action onEnd = null) {
        onStart?.Invoke();
        FadeOut(fadeDuration / 2f, () => {
            onProcess?.Invoke();
            FadeIn(fadeDuration / 2f, onEnd);
        });
    }

    void FadeOut(float fadeDuration = 1f, Action onComplete = null) {
        fullScreenFadeImage.gameObject.SetActive(true);
        fullScreenFadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    void FadeIn(float fadeDuration = 1f, Action onComplete = null) {
        fullScreenFadeImage.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            fullScreenFadeImage.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // 스타트엔딩UI 모두 켜기
    public void startEndingUI_OpenAll()
    {
        startEndingUI.SetActive(true);
    }

    // 스타트엔딩UI Canvas 끄기
    public void startEndingUI_CloseAll()
    {
        startEndingUI.SetActive(false);
    }

    // QTE 이펙트 캔버스 제어 메서드들
    public void ShowQTEEffectCanvas()
    {
        if (qteEffectCanvas != null)
        {
            qteEffectCanvas.SetActive(true);
        }
    }
    public void HideQTEEffectCanvas()
    {
        if (qteEffectCanvas != null)
        {
            qteEffectCanvas.SetActive(false);
        }
    }
    public void ToggleQTEEffectCanvas()
    {
        if (qteEffectCanvas != null)
        {
            qteEffectCanvas.SetActive(!qteEffectCanvas.activeSelf);
        }
    }

    // 가장 위에 있는 UI를 닫음
    private void TryCloseTopUI()
    {
        for (int i = closableUI.Count - 1; i >= 0; i--)
        {
            if (closableUI[i] is IUIClosable closable && closable.IsOpen())
            {
                closable.Close();
                return;
            }
        }
        // 아무 UI도 안 켜져 있으면 ESC 메뉴 열기
        escMenu.ESCMenuToggle();
    }
    
    // 튜토리얼UI 키고 3초 뒤 끄기    
    public void TutorialUI_OpenAll()
    {
        TutorialUI.SetActive(true);
        StartCoroutine(CloseAfterDelay(3f));
    }
    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TutorialUI.SetActive(false);
    }

    // 퍼즐 진척도 UI 관리
    public void ShowPuzzleStatus(PuzzleStatus.Chapter chapter)
    {
        int target = (int)chapter;
        for (int i = 0; i < puzzleStatusPanels.Length; i++)
        {
            var go = puzzleStatusPanels[i];
            if (go == null) continue;

            bool active = (i == target);
            go.SetActive(active);

            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = active ? 1f : 0f; cg.interactable = active; cg.blocksRaycasts = active; }
        }
    }

    public void AutoSelectPuzzleStatusByScene()
    {
        var name = SceneManager.GetActiveScene().name;
        var chapter = DetectChapter(name);
        ShowPuzzleStatus(chapter);
    }

    private PuzzleStatus.Chapter DetectChapter(string sceneName)
    {
        if (sceneName.Contains("Ch01")) return PuzzleStatus.Chapter.Chapter1;
        if (sceneName.Contains("Ch02")) return PuzzleStatus.Chapter.Chapter2;
        if (sceneName.Contains("Ch03")) return PuzzleStatus.Chapter.Chapter3;

        return PuzzleStatus.Chapter.Chapter1; // 기본값
    }

   // 버튼 클릭시 사운드    
   void ButtonClickSound()
    {
        if (clickSound != null)
            SoundManager.Instance.PlaySFX(clickSound, 0.2f);
    }
}
    


