using UnityEngine;
using UnityEngine.Playables;
public class Cutscene_NPC : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject GarageDoor;
    public RoomInfo roomInfo;

    public bool isCutscenePlaying = false;

    void Start()
    {
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
            PossessionSystem.Instance.CanMove = false;
            UIManager.Instance.PlayModeUI_CloseAll();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCutscenePlaying && roomInfo.roomCount >= 1)
        {
            Play_NPCscene();
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        PossessionSystem.Instance.CanMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        EnemyAI.ResumeAllEnemies();
        UIManager.Instance.PromptUI.ShowPrompt("차고의 문이 조금 열렸어.", 2f);
    }
}