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
        TrunOnCanvas();
    }

    private void TrunOnCanvas()
    {
        Choice_canvas.SetActive(true);
        ExitText.SetActive(true);
        ExitDoor.UnlockDoors();
        TriggerColider.SetActive(true);

    }


}
