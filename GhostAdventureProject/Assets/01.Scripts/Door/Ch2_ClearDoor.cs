using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch2_ClearDoor : BaseInteractable
{
    private bool playerNearby = false;
    private bool canOpenDoor = false;

    private void Update()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!canOpenDoor)
            {
                UIManager.Instance.PromptUI.ShowPrompt("...잠겨 있다.", 2f);
                return;
            }
            else
            {
                // CH3로 이동
                Debug.Log("챕터3 슈우우우우우우ㅜㅇ우웃");
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
