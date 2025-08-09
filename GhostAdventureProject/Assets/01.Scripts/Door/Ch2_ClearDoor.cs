//using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch2_ClearDoor : BaseInteractable
{
    private bool playerNearby = false;
   [SerializeField] private bool canOpenDoor = false;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!canOpenDoor)
            {
                UIManager.Instance.PromptUI.ShowPrompt("아직은 나갈 수 없어", 2f);
                return;
            }
            else
            {
                // CH3로 이동
                SceneManager.LoadScene("Ch02_To_Ch03");
                Destroy(GameManager.Instance.Player.gameObject);
                UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //SetHighlight(true);
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
            playerNearby = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetHighlight(false);
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
            playerNearby = false;
        }
    }

    public void ActivateClearDoor()
    {
        canOpenDoor = true;
    }
}
