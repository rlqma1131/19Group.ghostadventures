using DG.Tweening;
using TMPro;
using UnityEngine;

// 플레이어가 방에 들어갈 때마다 방문 횟수를 기록하고 방이름을 UI에 표시하는 스크립트
public class Global_PlayerRoomTracker : MonoBehaviour
{
    [SerializeField] private UITweenAnimator uITweenAnimator; // UI 애니메이션 컴포넌트
    [SerializeField] private TextMeshProUGUI text; // 프롬프트 컴포넌트
    public string roomName_RoomTracker;
    public string prevRoomInfo;

    private void Start()
    {
        // 저장된 방문 수 로드
        var rooms = FindObjectsOfType<Global_RoomInfo>();
        foreach (var room in rooms)
        {
            if (SaveManager.TryGetRoomVisitCount(room.roomName, out int count))
                room.roomCount = count;
        }

        // UI 참조 세팅
        uITweenAnimator = UIManager.Instance.GetComponentInChildren<UITweenAnimator>();
        if (uITweenAnimator != null)
            text = uITweenAnimator.GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Room")) return;

        var room = other.GetComponent<Global_RoomInfo>();
        if (room == null) return;

        room.roomCount++;

        // 저장 데이터 반영
        SaveManager.SetRoomVisitCount(room.roomName, room.roomCount);

        // UI 표시
        roomName_RoomTracker = room.roomName;
        // if (text != null) text.text = room.roomName;
        // if (uITweenAnimator != null) uITweenAnimator.FadeInAndOut();
                // 같은 방이면 UI 재출력 안 함
        if (prevRoomInfo != room.roomName)
        {
            if (text != null) text.text = room.roomName;
            uITweenAnimator?.FadeInAndOut();
            prevRoomInfo = room.roomName;              // 여기서 갱신!
        }

        // 첫 방문 시 튜토리얼
        if (room.roomCount == 1)
        {
            if (room.roomName == "거실" && !TutorialManager.Instance.HasCompleted(TutorialStep.LivingRoomIntro_Start))
                TutorialManager.Instance.Show(TutorialStep.LivingRoomIntro_Start);
            else if (room.roomName == "다용도실" && !TutorialManager.Instance.HasCompleted(TutorialStep.LaundryRoom))
                TutorialManager.Instance.Show(TutorialStep.LaundryRoom);
            // else if (room.roomName == "놀이터")
            //     TutorialManager.Instance.Show(TutorialStep.Test);
            // else if (room.roomName == "일반병동 - 1F 로비")
            //     TutorialManager.Instance.Show(TutorialStep.Test);
        }
    }
}

 