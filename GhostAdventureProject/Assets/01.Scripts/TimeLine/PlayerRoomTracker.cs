using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    [SerializeField] private UITweenAnimator uITweenAnimator; // UI 애니메이션 컴포넌트
    [SerializeField] private TextMeshProUGUI text; // 프롬프트 컴포넌트
    private string prevRoomInfo;
    public string roomName_RoomTracker;

    private void Start()
    {
        // 방문 수 로드
        var rooms = FindObjectsOfType<RoomInfo>();
        foreach (var room in rooms)
        {
            if (SaveManager.TryGetRoomVisitCount(room.roomName, out int count))
                room.roomCount = count;
        }

        // UI 참조
        if (uITweenAnimator == null)
            uITweenAnimator = UIManager.Instance.GetComponentInChildren<UITweenAnimator>(true);
        if (text == null && uITweenAnimator != null)
            text = uITweenAnimator.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 태그보다 컴포넌트 존재로 판별하는 편이 안전
        var room = other.GetComponent<RoomInfo>() ?? other.GetComponentInParent<RoomInfo>();
        if (room == null) return;

        room.roomCount++;
        SaveManager.SetRoomVisitCount(room.roomName, room.roomCount);

        // 같은 방이면 UI 재출력 안 함
        if (prevRoomInfo != room.roomName)
        {
            if (text != null) text.text = room.roomName;
            uITweenAnimator?.FadeInAndOut();
            prevRoomInfo = room.roomName;              // 여기서 갱신!
        }

        roomName_RoomTracker = room.roomName;

        // 첫 방문 튜토리얼
        if (room.roomCount == 1)
        {
            if (room.roomName == "거실")
                TutorialManager.Instance.Show(TutorialStep.LivingRoomIntro_Start);
            else if (room.roomName == "다용도실")
                TutorialManager.Instance.Show(TutorialStep.LaundryRoom);
        }
    }
}

 