//using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch2_ClearDoor : BaseInteractable
{
    private bool playerNearby = false;
    [SerializeField] private Ch2_MemoryPositive_01_HandPrint handPrint;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!SaveManager.HasCollectedMemoryID(handPrint.data.memoryID))
            {
                UIManager.Instance.PromptUI.ShowPrompt("아직은 나갈 수 없어", 2f);
                return;
            }
            else
            {
                // CH3로 이동
                SceneManager.LoadScene("Ch02_To_Ch03");
                Destroy(GameManager.Instance.PlayerObj.gameObject);
                UIManager.Instance.Inventory_PlayerUI.RemoveClueBeforeStage();
                UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            //SetHighlight(true);
            player.InteractSystem.AddInteractable(gameObject);
            playerNearby = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            SetHighlight(false);
            player.InteractSystem.RemoveInteractable(gameObject);
            playerNearby = false;
        }
    }
}
