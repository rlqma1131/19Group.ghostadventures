using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEditor.Rendering.LookDev;

public class Ch1_ClearDoor : BaseInteractable
{
    [SerializeField] private Ch1_MemoryPositive_01_TeddyBear TeddyBear;
    [SerializeField] private Ch1_GarageEventManager garageEvent;
    [SerializeField] private PlayableDirector playable;

    private bool canOpenDoor = false;
    public bool testing = true; // 테스트용 변수


    void Start()
    {
        
        playable.stopped += OnTimelineFinished;
    }
    private void Update()
    {
        if(!canOpenDoor)
            return; 

        if (Input.GetKeyDown(KeyCode.E))
        {
            // 이름 안맞췄을 때
            if (!garageEvent.Answer.correct && !testing)
            {
                UIManager.Instance.PromptUI.ShowPrompt("...잠겨 있다.", 2f);
            }
            // 이름 맞췄는데, 기억조각을 안 모았을 때
            else if (!TeddyBear.Completed_TeddyBear && !testing)
            {
                UIManager.Instance.PromptUI.ShowPrompt("곰인형을 살펴봐야 해..", 2f);
            }
            // 이름 맞추고, 기억조각도 모았을 때
            else if (TeddyBear.Completed_TeddyBear || testing)
            {
                PossessionSystem.Instance.CanMove = false;
                playable.Play();
                UIManager.Instance.PlayModeUI_CloseAll();
                GameManager.Instance.Player.gameObject.SetActive(false); // 플레이어 비활성화
                //SceneManager.LoadScene("Ch02");
            }
        }
    }

    
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            canOpenDoor = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canOpenDoor = false;
        }
    }



    void OnTimelineFinished(PlayableDirector obj)
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        PossessionSystem.Instance.CanMove = true;
        SceneManager.LoadScene("Ch01_To_Ch02");
    }



}

