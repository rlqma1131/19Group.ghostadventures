using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    //ui창 열고 닫기
    //ui상태관리(어떤ui켜져있는지 추적)
    //시각효과 제어(미정)


    // 각 UI에 연결 ======================================================
    [SerializeField] private SoulGauge soulGauge; // 영혼에너지 
    [SerializeField] private Prompt prompt; // 프롬프트
    [SerializeField] private QTEUI qte1;// QTE
    [SerializeField] private QTEUI2 qte2; // QTE2
    public GameObject scanUI; // 스캔UI
    [SerializeField] private MemoryStorage memoryStorage;// 기억저장소
    [SerializeField] private Inventory_Player inventory_player; // 인벤토리-플레이어
    [SerializeField] private InventoryExpandViewer inventoryExpandViewer; // 인벤토리 확대뷰어
    [SerializeField] private PlayButton_test playbutton; // 플레이버튼
    public GameObject gameover; // 게임오버(텍스트)

    // 외부 접근용
    public SoulGauge SoulGaugeUI => soulGauge;
    public Prompt PromptUI => prompt;
    public QTEUI QTE_UI => qte1;
    public QTEUI2 QTE_UI_2 => qte2;
    public MemoryStorage MemoryStorageUI => memoryStorage;
    public Inventory_Player Inventory_PlayerUI => inventory_player;
    public InventoryExpandViewer InventoryExpandViewerUI => inventoryExpandViewer;
    // ==================================================================
    
    [SerializeField] private List <GameObject> allUIs; // 모든 UI

    private void Start()
    {
        gameover.SetActive(false);
    }

    // 게임시작시 UI 셋팅
    // private void Start()
    // {
    //     foreach(GameObject ui in allUIs)
    //     {
    //         ui.SetActive(false);
    //     }
    //     playbutton.gameObject.SetActive(true);
    // }

    // targetUI 하나만 보이게 하기
    public void ShowOnly(GameObject targetUI)
    {
        foreach(GameObject ui in allUIs)
        {
            ui.SetActive(ui == targetUI);
        }
    }

    // 모든 UI 보이게 하기(play버튼 제외)
    public void ShowAll()
    {
        foreach(GameObject ui in allUIs)
        {
            ui.SetActive(true);
        }
        playbutton.gameObject.SetActive(false);
    }


}
