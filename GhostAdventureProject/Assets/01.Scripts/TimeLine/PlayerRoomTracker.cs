using TMPro;
using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    [SerializeField] private UITweenAnimator uITweenAnimator; // UI 애니메이션 컴포넌트
    [SerializeField] private TextMeshProUGUI text; // 프롬프트 컴포넌트

    private void Awake()
    {
        uITweenAnimator = FindObjectOfType<UITweenAnimator>();
        text = GameObject.Find("InfoText").GetComponent<TextMeshProUGUI>();

    }
    private void OnTriggerEnter2D(Collider2D other)
    {

        if(other.tag == "Room")
        {

        RoomInfo room = other.GetComponent<RoomInfo>();
        if (room != null)
        {
            room.roomCount++;
            Debug.Log($"Entered {room.roomName}, count: {room.roomCount}");


                //맵이름 두두둥장
                text.text = $"{room.roomName}";
                uITweenAnimator.SlideInAndOut();
        }

        }

       

    }
}
 