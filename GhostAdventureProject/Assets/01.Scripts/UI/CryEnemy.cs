using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ch3 울보
//4마리
//일반병동 - 약품실, 원장실
//폐병동 - 폐병실2,4 
//머리위에 원하는 단서 또는 아이템이 있음. 가져가면 울음 그침.
//방에 있다가 플레이어가 들어왔을 때 (룸트래커로 확인) 울기 시작함.
//만약 플레이어가 방에서 그냥 나간다면 사신이 복도를 왔다갔다하면서 플레이어를 쫓아감
//울보를 달래주려면 오르골3개 QTE를 성공시켜야 함.
//성공하면 울음 멈춤
//실패하면 (총3번의 기회) 크게 울며 변신 -> 플레이어 공격

//만들어야 하는 것들
//함수 - 울기 (사운드 플레이)
//함수 - 울음 그치기 (사운드 스탑)
//함수 - 큰울음 (사운드 플레이, 변신애니메이션)
//함수 - 공격(방 안에 있는 플레이어를 공격함)
//오르골 QTE작동 - 오르골과 연결하기 (QTE성공,실패여부에 따라 울보의 상태변경)
//


// ?울보가 원하는 아이템을 찾으러 다닐 때 문 앞에 대기하고 있는 사신에게 죽는건 아닌지

public class CryEnemy : MonoBehaviour
{
    [SerializeField] private ClueData needClue; // 원하는 단서
    [SerializeField] private AudioClip cry_small; // 기본울음
    [SerializeField] private AudioClip cry_big; // 큰울음
    [SerializeField] private AudioSource cryMusic;
    [SerializeField] private Ch3_MusicBox musicBox;
    private GameObject player;
    private SoundManager soundManager;
    [SerializeField] string cryRoomName; // 울보가 있는 방의 이름
    private string roomName; // 플레이어가 있는 방의 이름
    [SerializeField] private List<Ch3_MusicBox> myMusicBoxes; // 연결된 3개의 오르골
    private HashSet<Ch3_MusicBox> clearedBoxes = new HashSet<Ch3_MusicBox>(); // QTE 성공한 오르골


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log(player);
        soundManager = SoundManager.Instance;
        roomName = player.GetComponentInChildren<PlayerRoomTracker>().roomName_RoomTracker;
    }

    void Update()
    {
        if(cryRoomName == roomName) StartCrying();

        if(Ch3_MusicBox.musicBox_FailCount == 3)
        {
            StartBigCrying();
        }
    }

    // 울보가 있는 방과 플레이어가 있는 방이 같으면 울기 시작
    public void StartCrying()
    {
        if(cryRoomName == roomName)
        {
            soundManager.PlaySFX(cry_small);
        }

        clearedBoxes.Clear();
        soundManager.PlaySFX(cry_small);

        // 오르골들에게 자신을 연결
        foreach (var box in myMusicBoxes)
        {
            box.linkedEnemy = this;
        }
    }


    private void StopCrying()
    {
        soundManager.StopSFX();
    }

    private void StartBigCrying()
    {
        soundManager.StopSFX();
        soundManager.PlaySFX(cry_big);
    }

    private void ChangeState()
    {
        //오르골 QTE가 실패하면
        //애니메이션
    }

    private void Attack()
    {
        //플레이어를 쫓아가서 공격함.
    }



    public void OnMusicBoxSuccess(Ch3_MusicBox box)
    {
        if (!clearedBoxes.Contains(box))
        {
            clearedBoxes.Add(box);
            Debug.Log($"울보 오르골 {clearedBoxes.Count}개 해제됨");

            if (clearedBoxes.Count >= 3)
            {
                StopCrying();
            }
        }
    }
}
