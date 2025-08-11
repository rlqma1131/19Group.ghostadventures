using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor.Timeline;
using UnityEngine.Playables;
//using UnityEditor.Rendering.LookDev;

public class Ch1_ClearDoor : BaseInteractable
{
    [SerializeField] private Ch1_MemoryPositive_01_TeddyBear TeddyBear;
    [SerializeField] private Ch1_GarageEventManager garageEvent;
    [SerializeField] private PlayableDirector playable;
    Inventory_Player inventory_Player;
    private bool canOpenDoor = false;
    //public bool testing = true; // 테스트용 변수



    void Start()
    {
        inventory_Player = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        playable.stopped += OnTimelineFinished;
    }
    private void Update()
    {
        if(!canOpenDoor)
            return; 

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 이름 안맞췄을 때
            if (!garageEvent.Answer.correct)
            {
                UIManager.Instance.PromptUI.ShowPrompt("안에서 더 해야할 일이 남았어", 2f);
            }
            // 이름 맞췄는데, 기억조각을 안 모았을 때
            else if (garageEvent.Answer.correct && !TeddyBear.Completed_TeddyBear)
            {
                UIManager.Instance.PromptUI.ShowPrompt("곰인형을 살펴봐야 해..", 2f);
            }
            // 이름 맞추고, 기억조각도 모았을 때
            else if (garageEvent.Answer.correct && TeddyBear.Completed_TeddyBear)
            {
                PossessionSystem.Instance.CanMove = false;
                playable.Play();
                EnemyAI.PauseAllEnemies();
                UIManager.Instance.PlayModeUI_CloseAll();
                inventory_Player.RemoveClueBeforeStage();
                Destroy(GameManager.Instance.Player.gameObject); // 플레이어 비활성화
            }
        }
    }

    
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            canOpenDoor = true;
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }
        if (collision.CompareTag("Player")&& TeddyBear.Completed_TeddyBear && canOpenDoor && garageEvent.Answer.correct)
        {
            UIManager.Instance.PromptUI.ShowPrompt_Random("여기서 볼 일은 끝난거 같아 ", "이제 여기서 나가자");
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpenDoor = false;
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }



    void OnTimelineFinished(PlayableDirector obj)
    {
        EnemyAI.ResumeAllEnemies();
        PossessionSystem.Instance.CanMove = true;
        SceneManager.LoadScene("Ch01_To_Ch02");
        UIManager.Instance.PlayModeUI_CloseAll();
    }



}

