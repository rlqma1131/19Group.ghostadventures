using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IUIClosable
{
    void Close();
    bool IsOpen();
}

public class UIManager : Singleton<UIManager>
{
    // ==============================================================================================

    // 1. 플레이모드 UI
    [SerializeField] private SoulGauge soulGauge; // 영혼에너지 
    [SerializeField] private Prompt prompt; // 프롬프트
    [SerializeField] private Prompt_UnPlayMode prompt2; // 컷신용 프롬프트
    [SerializeField] private QTEUI qte1;// QTE
    [SerializeField] private QTEUI2 qte2; // QTE2
    [SerializeField] private QTEUI3 qte3; // QTE3
    public GameObject scanUI; // 스캔UI
    [SerializeField] private MemoryStorage memoryStorage;// 기억저장소
    [SerializeField] private Inventory_Player inventory_Player; // 인벤토리-플레이어
    [SerializeField] private Inventory_PossessableObject inventory_PossessableObject; // 인벤토리-빙의오브젝트
    [SerializeField] private InventoryExpandViewer inventoryExpandViewer; // 인벤토리 확대뷰어
    [SerializeField] private ESCMenu escMenu; // ESC 메뉴
    [SerializeField] private NoticePopup noticePopup;
    [SerializeField] private NoticePopup saveNoticePopup;
    [SerializeField] private GameObject[] puzzleStatusPanels;

    // QTE 이펙트 캔버스 추가
    [SerializeField] private GameObject qteEffectCanvas; // QTE 이펙트 캔버스

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
    public GameObject guidButton;
    public OverlayHoleFitter overlay;

    // QTE 이펙트 캔버스 외부 접근용
    public GameObject QTEEffectCanvas => qteEffectCanvas;

    // -------------------------------------------------------------------------------------------

    // 2. 게임 시작/엔딩 UI
    public GameObject gameover; // 게임오버(텍스트)

    // -------------------------------------------------------------------------------------------

    // 3. 상호작용 UI
    public GameObject q_Key;
    public GameObject a_Key;
    public GameObject d_Key;
    public GameObject spacebar_Key;
    public GameObject interactInfo;
    public GameObject unpossessKey;
    
    // --------------------------------------------------------------------------------------------


    [Header("UICanvas 전체 키고 끌때 사용")]
    [SerializeField] private GameObject playModeUI; // 플레이모드 모든 UI(Canvas)
    [SerializeField] private GameObject startEndingUI; // 게임 시작/엔딩 모든 UI(Canvas)
    [SerializeField] private GameObject TutorialUI; // 튜토리얼 모든 UI(Canvas)
    [SerializeField] private GameObject interactUI; // 상호작용 모든 UI(Canvas)


    [Header("ESC 닫기용 UI List")]
    [SerializeField] private List<MonoBehaviour> closableUI; // ESC키로 닫을 모든 UI List
    [SerializeField] private List<GameObject> allUIs; // 모든 UI (현재 미사용)


    // ===========================================================================================    

    // 커서 관리
    [Header("커서 셋팅")]
    [SerializeField] private Texture2D defaultCursor; // 기본
    [SerializeField] private Texture2D findClueCursor; // 단서
    [SerializeField] private Texture2D hideAreaCursor; // 은신처
    [SerializeField] private Texture2D lockDoorCursor; // 잠긴문
    [SerializeField] private Texture2D openDoorCursor; // 열린문
    [SerializeField] private Texture2D moveAbleCursor; // 움직임가능하다는표시 커서
    private GameObject lastHovered;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private EventSystem eventSystem;

    // -------------------------------------------------------------------------------------------
    public AudioClip clickSound;
    // -------------------------------------------------------------------------------------------
    void PlayClickSound()
    {
        if (clickSound != null)
            SoundManager.Instance.PlaySFX(clickSound, 0.2f);
    }


    // private void Start()
    // {
    //     playModeUI.SetActive(false);
    //     startEndingUI.SetActive(true);
    //     gameover.SetActive(false);
    //     // gameover.SetActive(false);
    // }

    private void Start()
    {
        SetDefaultCursor();
        if(SceneManager.GetActiveScene().name == "IntroScene_Real" || SceneManager.GetActiveScene().name =="Ch01_To_Ch02"
            || SceneManager.GetActiveScene().name == "Ch02_To_Ch03" || SceneManager.GetActiveScene().name == "Ch03_To_Ch04" || SceneManager.GetActiveScene().name == "StartScene")
        {
            PlayModeUI_CloseAll();
        }
        eventSystem = FindObjectOfType<EventSystem>();

        // 씬 안의 모든 Button 찾기
        Button[] buttons = FindObjectsOfType<Button>(true); // 비활성화 포함

        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => PlayClickSound());
        }
    
    }
    private void Update() {

    if (Input.GetMouseButtonDown(0)) // 클릭 시 확인
    {
        // Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

        // if (hit.collider != null)
        // {
        //     Debug.Log("감지됨: " + hit.collider.gameObject.name);
        // }
        // else
        // {
        //     Debug.Log("히트 안 됨!");
        // }

        //============================================================================
        //   PointerEventData pointerData = new PointerEventData(eventSystem)
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
        //============================================================================
        
    
        }
        
        // for (int i = closableUI.Count - 1; i >= 0; i--)
        // {
        //     if (closableUI[i] is IUIClosable closable && closable.IsOpen())
        //     {
        //         SetDefaultCursor();
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.Escape))
        TryCloseTopUI();
    }

    public void TutorialUI_CloseAll()
    {
        TutorialUI.SetActive(false);
    }

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


    public void SetCursor(Texture2D cursor)
    {
        Cursor.SetCursor(cursor, hotspot, CursorMode.Auto);
    }
    // 기본 커서
    public void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }
    // 단서 커서
    public void FindClueCursor()
    {
        Cursor.SetCursor(findClueCursor, hotspot, CursorMode.Auto);
    }
    // 은신처 커서
    public void HideAreaCursor()
    {
        Cursor.SetCursor(hideAreaCursor, hotspot, CursorMode.Auto);
    }
    public void LockDoorCursor()
    {
        Cursor.SetCursor(lockDoorCursor, hotspot, CursorMode.Auto);
    }
    public void OpenDoorCursor()
    {
        Cursor.SetCursor(openDoorCursor, hotspot, CursorMode.Auto);
    }

    public void ClearCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    public void MoveAbleCursor()
    {
        Cursor.SetCursor(moveAbleCursor, hotspot, CursorMode.Auto);
    }

    // targetUI 하나만 보이게 하기
    public void ShowOnly(GameObject targetUI)
    {
        foreach (GameObject ui in allUIs)
        {
            ui.SetActive(ui == targetUI);
        }
    }

    // 모든 UI 보이게 하기(play버튼 제외)
    // public void ShowAll()
    // {
    //     foreach (GameObject ui in allUIs)
    //     {
    //         ui.SetActive(true);
    //     }
    //     playbutton.gameObject.SetActive(false);
    // }

    // 플레이모드UI Canvas 끄기
    public void PlayModeUI_CloseAll()
    {
        playModeUI.SetActive(false);
    }

    // 스타트엔딩UI Canvas 끄기
    public void startEndingUI_CloseAll()
    {
        startEndingUI.SetActive(false);
    }

    // 플레이모드UI 모두 켜기
    public void PlayModeUI_OpenAll()
    {
        playModeUI.SetActive(true);
    }

    // 스타트엔딩UI 모두 켜기
    public void startEndingUI_OpenAll()
    {
        startEndingUI.SetActive(true);
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
        // 아무 UI도 안 켜져 있으면 ESC 메뉴 토글
        escMenu.ESCMenuToggle();
    }


    // Q_Key 보이기 / 숨기기
    public void Show_Q_Key(Vector3 worldPosition)
    {
        q_Key.SetActive(true);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        screenPos.y += 40f;
        q_Key.transform.position = screenPos;
    }
    public void Hide_Q_Key()
    {
        q_Key.SetActive(false);
    }

    // A_Key 보이기 / 숨기기
    public void Show_A_Key(Vector3 worldPosition)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        screenPos.y += 360f;
        a_Key.transform.position = screenPos;
        a_Key.SetActive(true);
    }
    public void Hide_A_Key()
    {
        a_Key.SetActive(false);
    }

    // D_Key 보이기 / 숨기기
    public void Show_D_Key(Vector3 worldPosition)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        screenPos.y += 360f;
        d_Key.transform.position = screenPos;
        d_Key.SetActive(true);
    }
    public void Hide_D_Key()
    {
        a_Key.SetActive(false);
    }

    // SpaceBar_Key 보이기 / 숨기기
    public void Show_SpaceBar_Key()
    {
        spacebar_Key.SetActive(true);
    }
    public void Hide_SpaceBar_Key()
    {
        spacebar_Key.SetActive(false);
    }

    // public Vector3 SetPosition_Q_Key(Vector3 target, float amount)
    // {
    //     Vector2 targetPos = Camera.main.WorldToScreenPoint(target);
    //     targetPos.y += amount;
    //     return targetPos;
    // }

    public Vector3 SetPosition_Q_Key(Vector3 target)
    {
        Vector3 worldPos;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(target);

        RectTransform canvasRect = q_Key.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out worldPos
        );
        return worldPos;
    }

    // public void Show_Q_Key(Vector3 worldPosition)
    // {
    //     q_Key.SetActive(true);
    //     Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
    //     screenPos.y += 40f;
    //     q_Key.transform.position = screenPos;
    // }
    // public void Update_Q_Key(BasePossessable current)
    // {
    //     if (current != null && current.IsPossessed() && !current.HasActivated())
    //     {
    //         Show_Q_Key(current.transform.position);
    //     }
    //     else
    //     {
    //         Hide_Q_Key();
    //     }
    // }

    // 퍼즐 진척도 UI 관리
    public void ShowPuzzleStatus(PuzzleStatus.Chapter chapter)
    {
        int target = (int)chapter;
        for (int i = 0; i < puzzleStatusPanels.Length; i++)
        {
            bool active = (i == target);
            if (puzzleStatusPanels[i] == null) continue;

            puzzleStatusPanels[i].SetActive(active);

            // 선택: CanvasGroup까지 있으면 인터랙션/히트 차단도 같이
            var cg = puzzleStatusPanels[i].GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = active ? 1f : 0f;
                cg.interactable = active;
                cg.blocksRaycasts = active;
            }
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

}
    


