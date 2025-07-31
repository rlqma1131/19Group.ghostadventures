using Cinemachine;
using UnityEngine;

public class Ch3_Console : BaseInteractable
{
    [SerializeField] private GameObject qKey;
    [SerializeField] private ItemData cardKey;

    [Header("줌 화면")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera; // 줌 카메라

    [Header("퍼즐 버튼")]
    [SerializeField] private Ch3_ConsoleButton[] buttons;
    

    [Header("단서 종이")]
    [SerializeField] private GameObject paper;

    Inventory_PossessableObject inventory; // 빙의 인벤토리(Item을 갖고 있는지 확인용)

    private bool canUse = false;
    private bool isZoomed = false;

    private Ch3_ConsoleButton currentActiveButton;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!canUse)
                return;

            Debug.Log("Q키 눌림");
            inventory = Inventory_PossessableObject.Instance;
            
            Debug.Log("인벤토리: " + inventory);
            Debug.Log("카드키: " + cardKey);

            if (inventory == null || cardKey != inventory.selectedItem()
                || !PossessionStateManager.Instance.IsPossessing())
            {
                UIManager.Instance.PromptUI.ShowPrompt("조작하려면 카드키가 필요해보여", 2f);
                return;
            }

            if (cardKey == inventory.selectedItem() && cardKey != null)
            {
                UIManager.Instance.PlayModeUI_CloseAll();
                qKey.SetActive(false);
                zoomCamera.Priority = 20;
                isZoomed = true;
                return;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && isZoomed)
        {
            UIManager.Instance.PlayModeUI_OpenAll();
            zoomCamera.Priority = 5;
            isZoomed = false;
            qKey.SetActive(true);

            foreach (var button in buttons)
            {
                button.HideQuestion();
            }
            currentActiveButton = null;
        }
    }

    public void CheckAllAnswers()
    {
        foreach (var btn in buttons)
        {
            if (!btn.IsCorrectlyAnswered())
                return; // 하나라도 정답 아니면 중단
        }

        Debug.Log("정답!");
    }

    public void OnButtonClicked(Ch3_ConsoleButton clickedButton)
    {
        if (currentActiveButton == clickedButton)
        {
            // 같은 버튼 다시 누름
            clickedButton.HideQuestion();
            currentActiveButton = null;
        }
        else
        {
            // 다른 버튼 누름
            if (currentActiveButton != null)
                currentActiveButton.HideQuestion();

            clickedButton.ShowQuestion();
            currentActiveButton = clickedButton;
        }
    }

    public void ResetCurrentButton(Ch3_ConsoleButton button)
    {
        if (currentActiveButton == button)
            currentActiveButton = null;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(true);
            qKey.SetActive(true);
            canUse = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(false);
            qKey.SetActive(false);
            canUse = false;
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
