using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class Ch1_GarageEvent : BaseInteractable
{
    private Ch1_MemoryPositive_01_TeddyBear bear;
    [SerializeField] private KeyBoard keyboard;
    [SerializeField] private KeyBoard_Enter answer;
    [SerializeField] private PlayableDirector cutsceneDirector;
    [SerializeField] private PlayableDirector cutsceneDirector_correct;

    private bool playerNearby = false;
    private bool isCutscenePlaying = false;
    private bool isCutscenePlaying2 = false;


    void Start()
    {
        bear = GetComponent<Ch1_MemoryPositive_01_TeddyBear>();

        cutsceneDirector.stopped += OnTimelineFinished;
        cutsceneDirector_correct.stopped += OnTimelineFinished2;
    }

    void Update()
    {
        if (!playerNearby)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            //if (!ChapterEndingManager.Instance.AllCh1CluesCollected())
            //{
            //    UIManager.Instance.PromptUI.ShowPrompt("...아무 일도 일어나지 않았다.", 2f);
            //}
            //else
            {

                if (!isCutscenePlaying)
                {


                PossessionSystem.Instance.canMove = false;
                UIManager.Instance.PlayModeUI_CloseAll();
                cutsceneDirector.Play();
                }
                // 1장 단서 모두 모이고 충돌 시 이벤트 발생



                // [컷씬 재생]
                // 꼬마유령 등장
                // 깜짝놀래키기

                // 컷씬 끝나면
                // 이름 입력 창 띄우기
            }
        }

        if (!answer.correct)
            return;

        if (!isCutscenePlaying2 && answer.correct)
        {

        UIManager.Instance.PlayModeUI_CloseAll();
        cutsceneDirector_correct.Play();
        PossessionSystem.Instance.canMove = false;
        bear.ActivateTeddyBear();

        }
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

    void OnTimelineFinished(PlayableDirector pd)
    {
        keyboard.OpenKeyBoard();
        PossessionSystem.Instance.canMove = true;
        UIManager.Instance.PlayModeUI_OpenAll();
        isCutscenePlaying = true;

    }    
    void OnTimelineFinished2(PlayableDirector pd)
    {

        keyboard.Close();
        PossessionSystem.Instance.canMove = true;
        isCutscenePlaying2 = true;
        UIManager.Instance.PlayModeUI_OpenAll();

    }
}

