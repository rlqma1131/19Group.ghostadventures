using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.Playables;

//챕터2 시작시 NPC이벤트를 위한 스크립트
public class Ch02_NPCEvent : MonoBehaviour
{
    [SerializeField] PlayableDirector director; // 타임라인 디렉터
    [SerializeField] Global_RoomInfo roomInfo; // 방 정보
    [SerializeField] GameObject kid; // Ch2_kid 

    bool isTimelinePlaying; // 타임라인 재생 여부
    SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    Player player;

    void Start() 
    {
        player = GameManager.Instance.Player;
    }

    void Update() {
        if (roomInfo.roomCount >= 1) {
            kid.SetActive(false); // 방 방문 수가 1 이상이면 Ch2_kid 비활성화 (한 번만 이벤트 발생하게하기 위함)
        }
    }

    void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.CompareTag("Player") && !isTimelinePlaying && roomInfo.roomCount == 0)
        // 플레이어가 충돌하고 타임라인이 재생 중이 아니며 방 방문 수가 0일 때
        {
            if (director != null && !EventManager.Instance.IsEventCompleted(GetComponent<UniqueId>().Id)) {
                EventManager.Instance.MarkEventCompleted(GetComponent<UniqueId>().Id);

                spriteRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
                Vector3 scale = player.transform.localScale;
                spriteRenderer.flipX = true; // 플레이어 스프라이트를 왼쪽으로 뒤집기
                //scale.x = -Mathf.Abs(scale.x); // 항상 왼쪽 보게
                player.transform.localScale = scale;
                player.PossessionSystem.CanMove = false; // 플레이어 이동 불가능하게 설정
                UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
                director.Play(); //타임라인 재생
                EnemyAI.PauseAllEnemies(); // 모든 적 일시정지
                isTimelinePlaying = true; // 타임라인 재생 상태로 설정
                roomInfo.roomCount++; // 방 방문 수 증가
                SaveManager.SetRoomVisitCount(roomInfo.roomName, roomInfo.roomCount);
            }
        }
    }

    void OnEnable() {
        if (director == null) return;
        director.stopped += OnTimelineStopped; // 타임라인이 중지될 때 이벤트 등록
    }


    void OnTimelineStopped(PlayableDirector playable) {
        player.PossessionSystem.CanMove = true; // 플레이어 이동 가능하게 설정
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 다시 열기
        UIManager.Instance.PromptUI.ShowPrompt("이 쪽지는 나를 말하는 건가?", 1.5f); // 프롬프트 UI 닫기
        EnemyAI.ResumeAllEnemies();
        // Ch2_kid 오브젝트 비활성화
    }

    void OnDisable() {
        if (director == null || spriteRenderer == null) return;

        spriteRenderer.flipX = false; // 플레이어 스프라이트를 원래대로 돌리기
        director.stopped -= OnTimelineStopped;
    }
}