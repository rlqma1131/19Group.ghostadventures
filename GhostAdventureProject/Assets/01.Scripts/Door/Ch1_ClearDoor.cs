using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class Ch1_ClearDoor : BaseInteractable
{
    [SerializeField] private Ch1_MemoryPositive_01_TeddyBear TeddyBear;
    [SerializeField] private Ch1_GarageEventManager garageEvent;
    [SerializeField] private PlayableDirector playable;
    Inventory_Player inventory_Player;
    bool canOpenDoor = false;
    //public bool testing = true; // 테스트용 변수

    override protected void Start()
    {
        base.Start();
        inventory_Player = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        playable.stopped += OnTimelineFinished;
    }

    void Update() {
        if (!canOpenDoor) return; 

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!SaveManager.IsPuzzleSolved("곰인형"))
            {
                UIManager.Instance.PromptUI.ShowPrompt("안에서 더 해야할 일이 남았어..", 2f);
            }
            // 이름 맞추고, 기억조각도 모았을 때
            else if (SaveManager.IsPuzzleSolved("곰인형"))
            {
                player.PossessionSystem.CanMove = false;
                playable.Play();
                UIManager.Instance.PlayModeUI_CloseAll();
                inventory_Player.RemoveClueBeforeStage();
                Destroy(GameManager.Instance.PlayerObj.gameObject); // 플레이어 비활성화
            }
        }
    }

    
    
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            canOpenDoor = true;
            player.InteractSystem.AddInteractable(gameObject);
        }
        if (collision.CompareTag("Player")&& SaveManager.IsPuzzleSolved("곰인형") && canOpenDoor && garageEvent.Answer.correct)
        {
            UIManager.Instance.PromptUI.ShowPrompt_Random("여기서 볼 일은 끝난거 같아 ", "이제 여기서 나가자");
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            canOpenDoor = false;
            player.InteractSystem.RemoveInteractable(gameObject);
        }
    }
    
    void OnTimelineFinished(PlayableDirector obj)
    {
        player.PossessionSystem.CanMove = true;
        SceneManager.LoadScene("Ch01_To_Ch02");
        UIManager.Instance.PlayModeUI_CloseAll();
    }
}

