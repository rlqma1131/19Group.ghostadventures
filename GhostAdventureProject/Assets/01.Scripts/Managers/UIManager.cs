using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

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
    [SerializeField] private Prompt prompt2; // 컷신용 프롬프트
    [SerializeField] private QTEUI qte1;// QTE
    [SerializeField] private QTEUI2 qte2; // QTE2
    public GameObject scanUI; // 스캔UI
    [SerializeField] private MemoryStorage memoryStorage;// 기억저장소
    [SerializeField] private Inventory_Player inventory_Player; // 인벤토리-플레이어
    [SerializeField] private Inventory_PossessableObject inventory_PossessableObject; // 인벤토리-빙의오브젝트
    [SerializeField] private InventoryExpandViewer inventoryExpandViewer; // 인벤토리 확대뷰어
    [SerializeField] private ESCMenu escMenu; // ESC 메뉴
    

    // QTE 이펙트 캔버스 추가
    [SerializeField] private GameObject qteEffectCanvas; // QTE 이펙트 캔버스

    // 외부 접근용
    public SoulGauge SoulGaugeUI => soulGauge;
    public Prompt PromptUI => prompt;
    public Prompt PromptUI2 => prompt2;
    public QTEUI QTE_UI => qte1;
    public QTEUI2 QTE_UI_2 => qte2;
    public MemoryStorage MemoryStorageUI => memoryStorage;
    public Inventory_Player Inventory_PlayerUI => inventory_Player;
    public Inventory_PossessableObject Inventory_PossessableObjectUI => inventory_PossessableObject;
    public InventoryExpandViewer InventoryExpandViewerUI => inventoryExpandViewer;
    public ESCMenu ESCMenuUI => escMenu;

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
    
    // --------------------------------------------------------------------------------------------


    [Header("UICanvas 전체 키고 끌때 사용")]
    [SerializeField] private GameObject playModeUI; // 플레이모드 모든 UI(Canvas)
    [SerializeField] private GameObject startEndingUI; // 게임 시작/엔딩 모든 UI(Canvas)
    [SerializeField] private GameObject interactUI; // 상호작용 모든 UI(Canvas)


    [Header("ESC 닫기용 UI List")]
    [SerializeField] private List<MonoBehaviour> closableUI; // ESC키로 닫을 모든 UI List
    [SerializeField] private List<GameObject> allUIs; // 모든 UI (현재 미사용)


    // ===========================================================================================    


    // private void Start()
    // {
    //     playModeUI.SetActive(false);
    //     startEndingUI.SetActive(true);
    //     gameover.SetActive(false);
    //     // gameover.SetActive(false);
    // }


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

     private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TryCloseTopUI();
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




    
}

