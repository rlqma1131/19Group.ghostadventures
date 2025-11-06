using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class Ch04_EndingTrigger : MonoBehaviour
{

    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject Choice_canvas;
    [SerializeField] private LockedDoor ExitDoor;
    [SerializeField] private GameObject ExitText;
    [SerializeField] private GameObject TriggerColider;
    private void Start()
    {
        if (director != null)
        {
            // 타임라인 종료 이벤트 등록
            director.stopped += OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector obj)
    {
        //화면에 선택지 캔버스 활성화
        TurnOnCanvas();
    }

    private void TurnOnCanvas()
    {
        if (Choice_canvas != null && !ReferenceEquals(Choice_canvas, null))
            Choice_canvas.SetActive(true);

        if (ExitText != null && !ReferenceEquals(ExitText, null))
            ExitText.SetActive(true);

        if (ExitDoor != null && !ReferenceEquals(ExitDoor, null))
            ExitDoor.UnlockDoors();

        if (TriggerColider != null && !ReferenceEquals(TriggerColider, null))
            TriggerColider.SetActive(true);
    }



}
