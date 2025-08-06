using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    [SerializeField] private UITweenAnimator uITweenAnimator; // UI 애니메이션 컴포넌트
    [SerializeField] private TextMeshProUGUI text; // 프롬프트 컴포넌트
    public string roomName_RoomTracker;

    private void Start()
    {
        uITweenAnimator = UIManager.Instance.GetComponentInChildren<UITweenAnimator>();
        if (uITweenAnimator != null)
        {
            text = uITweenAnimator.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Room")
        {
            RoomInfo room = other.GetComponent<RoomInfo>();
            if (room != null ||!room.isCount)
            {   
                room.isCount = true; // 방에 들어갈 때마다 카운트할지 여부 설정
                room.roomCount++;
                Debug.Log($"Entered {room.roomName}, count: {room.roomCount}");

                    //맵이름 두두둥장
                    roomName_RoomTracker = room.roomName;
                    text.text = $"{room.roomName}";
                    //uITweenAnimator.SlideInAndOut();
                    uITweenAnimator.FadeInAndOut();
                
                if (room.roomName == "거실" && !TutorialManager.Instance.HasCompleted(TutorialStep.LivingRoomIntro_Start))
                {
                    TutorialManager.Instance.Show(TutorialStep.LivingRoomIntro_Start);
                }
                
                if (room.roomName == "다용도실" && !TutorialManager.Instance.HasCompleted(TutorialStep.LaundryRoom))
                {
                    TutorialManager.Instance.Show(TutorialStep.LaundryRoom);                    
                }

                if (room.roomName == "놀이터")
                {
                    TutorialManager.Instance.Show(TutorialStep.Test);         
                }
                if (room.roomName == "일반병동 - 1F 로비")
                {
                    TutorialManager.Instance.Show(TutorialStep.Test);         
                }
            }


        }
    }
}
 