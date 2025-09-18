using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.Playables;
public class Cutscene_NPC : MonoBehaviour

//챕터1 아이방   시작시 NPC이벤트 재생을 위한 스크립트
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject GarageDoor;
    
    public Global_RoomInfo roomInfo; // 차고 방문 횟수
    public bool isCutscenePlaying = false;

    Player player;
    
    void Start() {
        player = GameManager.Instance.Player;
        if (director != null)
            director.stopped += OnTimelineStopped;
    }

    private void Play_NPCscene()
    {
        if (director != null && !EventManager.Instance.IsEventCompleted(GetComponent<UniqueId>().Id))
        {
            EventManager.Instance.MarkEventCompleted(GetComponent<UniqueId>().Id);

            director.Play();
            EnemyAI.PauseAllEnemies();
            GarageDoor.SetActive(false);
            isCutscenePlaying = true;
            player.PossessionSystem.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //차고 1번이라도 방문했고, 콜라이더 충돌시 재생
        if (other.CompareTag("Player") && !isCutscenePlaying && roomInfo.roomCount >= 1)
        {
            Play_NPCscene();
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        player.PossessionSystem.CanMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        EnemyAI.ResumeAllEnemies();
        UIManager.Instance.PromptUI.ShowPrompt("차고의 문이 조금 열렸어.", 2f);
    }
}