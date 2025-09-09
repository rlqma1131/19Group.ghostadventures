using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
/// <summary>
/// TeddyBear 한테 붙는 클래스
/// </summary>
public class Ch1_GarageEventManager : MonoBehaviour
{
    private Ch1_MemoryPositive_01_TeddyBear bear;
    [SerializeField] private KeyBoard keyboard;
    [SerializeField] private KeyBoard_Enter answer;
    [SerializeField] private PlayableDirector cutsceneDirector;
    [SerializeField] private PlayableDirector cutsceneDirector_correct;
    private bool isCutscenePlaying = false;
    private bool isCutscenePlaying2 = false;
    [SerializeField] EnergyRestoreZone energyRestoreZone;

    private bool playerNearby = false;
    private bool openKeyboard = false;
    [SerializeField] SpriteRenderer door;
    //NPC컷신보고 상호작용 가능하게하기 위해 추가
    [SerializeField]Cutscene_NPC cutscene_NPC;
    public KeyBoard_Enter Answer => answer;

    void Start()
    {
        bear = GetComponent<Ch1_MemoryPositive_01_TeddyBear>();
        cutsceneDirector.stopped += OnTimelineFinished;
        cutsceneDirector_correct.stopped += OnTimelineFinished2;
    }

    void Update()
    {
        //if (!bear.PlayerNearby)
        //    return;
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            {
                UIManager.Instance.PromptUI.ShowPrompt("단서가 부족해...", 2f);
            }
            else
            {
                if(!isCutscenePlaying && !EventManager.Instance.IsEventCompleted("Ch1_NPCEvent"))
                {
                    UIManager.Instance.PromptUI.ShowPrompt("아이방에서 누군가의 기척이 느껴져..", 2f);
                }

                if (!isCutscenePlaying && EventManager.Instance.IsEventCompleted("Ch1_NPCEvent"))
                {
                    PlayerInteractSystem.Instance.eKey.SetActive(false);
                    // [컷씬] 꼬마유령 이벤트
                    PossessionSystem.Instance.CanMove = false;
                    GameManager.Instance.PlayerController.animator.SetBool("Move", false);

                    UIManager.Instance.PlayModeUI_CloseAll();
                    EnemyAI.PauseAllEnemies();

                    SoulEnergySystem.Instance.DisableHealingEffect(); // 에너지 회복존 비활성화

                    cutsceneDirector.Play();
                }
                else if (isCutscenePlaying && !openKeyboard && !answer.correct)
                {
                    PlayerInteractSystem.Instance.eKey.SetActive(false);

                    SoulEnergySystem.Instance.DisableHealingEffect(); // 에너지 회복존 비활성화

                    openKeyboard = true;
                    keyboard.OpenKeyBoard();
                    PossessionSystem.Instance.CanMove = false;
                }
            }
        }

        if (!answer.correct)
            return;

        if (!isCutscenePlaying2 && answer.correct)
        {
            // [컷씬] 정답 이벤트
            UIManager.Instance.Inventory_PlayerUI.RemoveCluesByStage(GameManager.GetStageForCurrentChapter());
            UIManager.Instance.PlayModeUI_CloseAll();
            SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드아웃
            cutsceneDirector_correct.Play();
            PossessionSystem.Instance.CanMove = false;
            // 기억조각 스캔 가능하도록 활성화
            bear.ActivateTeddyBear();
            EnemyAI.PauseAllEnemies();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = true;
            PlayerInteractSystem.Instance.eKey.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerNearby = false;
            PlayerInteractSystem.Instance.eKey.SetActive(false);
        }
    }

    // 첫 상호작용, 벽에 피흐르는 이벤트 끝났을 때
    void OnTimelineFinished(PlayableDirector pd)
    {
        keyboard.OpenKeyBoard();
        UIManager.Instance.PlayModeUI_OpenAll();
        isCutscenePlaying = true;
    }

    // 정답 이벤트 끝났을 때
    void OnTimelineFinished2(PlayableDirector pd)
    {
        SoundManager.Instance.RestoreLastBGM(1f);
        keyboard.Close();

        SoulEnergySystem.Instance.EnableHealingEffect();
        EnemyAI.ResumeAllEnemies();
        PossessionSystem.Instance.CanMove = true;
        isCutscenePlaying2 = true;
        energyRestoreZone.IsActive = true; // 에너지 회복존 비활성화
        UIManager.Instance.PlayModeUI_OpenAll();
        door.color = new Color(door.color.r, door.color.g, door.color.b, 0f); // 문 투명하게

    }
}