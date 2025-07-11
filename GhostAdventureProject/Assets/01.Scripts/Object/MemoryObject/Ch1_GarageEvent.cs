using UnityEngine;

public class Ch1_GarageEvent : BaseInteractable
{
    [SerializeField] private KeyBoard keyboard;
    [SerializeField] private KeyBoard_Enter answer;
    private Ch1_MemoryPositive_01_TeddyBear bear;

    private bool playerNearby = false;

    void Start()
    {
        bear = GetComponent<Ch1_MemoryPositive_01_TeddyBear>();    
    }

    void Update()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            {
                UIManager.Instance.PromptUI.ShowPrompt("...아무 일도 일어나지 않았다.", 2f);
            }
            else
            {
                // 1장 단서 모두 모이고 충돌 시 이벤트 발생
                PossessionSystem.Instance.CanMove = false;

                // [컷씬 재생]
                // 꼬마유령 등장
                // 깜짝놀래키기

                // 컷씬 끝나면
                // 이름 입력 창 띄우기
                keyboard.OpenKeyBoard();
            }
        }

        if (!answer.correct)
            return;

        bear.ActivateTeddyBear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}

