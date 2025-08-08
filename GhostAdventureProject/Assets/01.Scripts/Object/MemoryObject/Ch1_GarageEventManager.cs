using UnityEngine;
using UnityEngine.Playables;
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

    public KeyBoard_Enter Answer => answer;

    void Start()
    {
        bear = GetComponent<Ch1_MemoryPositive_01_TeddyBear>();
        cutsceneDirector.stopped += OnTimelineFinished;
        cutsceneDirector_correct.stopped += OnTimelineFinished2;
    }

    void Update()
    {
        if (!bear.PlayerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            {
                UIManager.Instance.PromptUI.ShowPrompt("...아무 일도 일어나지 않았다.", 2f);
            }
            // 1장 단서 모두 모이고 상호작용 시 이벤트 발생
            else
            {
                if (!isCutscenePlaying)
                {
                    // [컷씬] 꼬마유령 이벤트
                    PossessionSystem.Instance.CanMove = false;
                    UIManager.Instance.PlayModeUI_CloseAll();
                    cutsceneDirector.Play();
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

    void OnTimelineFinished(PlayableDirector pd)
    {
        keyboard.OpenKeyBoard();
        UIManager.Instance.PlayModeUI_OpenAll();
        isCutscenePlaying = true;
    }

    void OnTimelineFinished2(PlayableDirector pd)
    {
        SoundManager.Instance.RestoreLastBGM(1f);
        keyboard.Close();
        EnemyAI.ResumeAllEnemies();
        PossessionSystem.Instance.CanMove = true;
        isCutscenePlaying2 = true;
        UIManager.Instance.PlayModeUI_OpenAll();
    }
}