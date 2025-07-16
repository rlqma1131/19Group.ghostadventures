using UnityEngine;
using UnityEngine.SceneManagement;

public class Ch1_ClearDoor : MonoBehaviour
{
    [SerializeField] private Ch1_MemoryPositive_01_TeddyBear TeddyBear;
    [SerializeField] private Ch1_GarageEventManager garageEvent;

    private bool canOpenDoor = false;

    private void Update()
    {
        if(!canOpenDoor)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 이름 안맞췄을 때
            if (!garageEvent.Answer.correct)
            {
                UIManager.Instance.PromptUI.ShowPrompt("...잠겨 있다.", 2f);
            }
            // 이름 맞췄는데, 기억조각을 안 모았을 때
            else if (!TeddyBear.Completed_TeddyBear)
            {
                UIManager.Instance.PromptUI.ShowPrompt("곰인형을 살펴봐야 해..", 2f);
            }
            // 이름 맞추고, 기억조각도 모았을 때
            else if (TeddyBear.Completed_TeddyBear)
            {
                SceneManager.LoadScene("Ch02");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            canOpenDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpenDoor = false;
        }
    }
}

