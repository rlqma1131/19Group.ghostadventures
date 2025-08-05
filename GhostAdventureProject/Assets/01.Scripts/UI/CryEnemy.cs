using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ch3 울보
//4마리
//일반병동 - 약품실, 원장실
//폐병동 - 폐병실2,4 
//머리위에 원하는 단서 또는 아이템이 있음. 가져가면 울음 그침.
//만약 플레이어가 방에서 그냥 나간다면 사신이 복도를 왔다갔다하면서 플레이어를 쫓아감

public enum CryEnemyState { Known, Chase, Attack }
public class CryEnemy : MonoBehaviour
{
    [SerializeField] private ClueData needClue; // 원하는 단서
    [SerializeField] private AudioClip cry_small; // 기본울음
    [SerializeField] private AudioClip cry_big; // 큰울음
    [SerializeField] private AudioSource cryMusic;
    [SerializeField] private Ch3_MusicBox musicBox;
    private GameObject player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private SoundManager soundManager;
    [SerializeField] private List<Ch3_MusicBox> myMusicBoxes; // 연결된 3개의 오르골
    private HashSet<Ch3_MusicBox> clearedBoxes = new HashSet<Ch3_MusicBox>(); // QTE 성공한 오르골
    private Animator anim;

    [SerializeField] string cryRoomName; // 울보가 있는 방의 이름
    private string roomName; // 플레이어가 있는 방의 이름


    private bool isCrying = false; // 울고 있는지 확인용
    private int failCount = 0;
    private int successCount = 0;

    private CryEnemyState currentState;

    [Header("울보 설정")]
    [SerializeField] private float moveSpeed = 4; // 스피드
    [SerializeField] private float chaseRange; // 추격 범위
    [SerializeField] private float attackRange; // 공격 범위

    private Vector2 lastDirection;


    void Start()
    {   
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        Debug.Log(player);
        soundManager = SoundManager.Instance;
        roomName = player.GetComponentInChildren<PlayerRoomTracker>().roomName_RoomTracker;
        anim = GetComponentInChildren<Animator>();
        currentState = CryEnemyState.Known;
        StartCrying();
    }

    void Update()
    {
        roomName = player.GetComponentInChildren<PlayerRoomTracker>().roomName_RoomTracker;

        if (cryRoomName == roomName && !isCrying)
        {
            StartCrying();
        }

    }

    void FixedUpdate()
    {
        switch(currentState)
        {
            case CryEnemyState.Chase:
            ChasePlayer();
            break;

            case CryEnemyState.Attack:
            Attack();
            break;

        }
    }

    // 울보가 있는 방과 플레이어가 있는 방이 같으면 울기 시작
    public void StartCrying()
    {
        isCrying = true;

        clearedBoxes.Clear();
        soundManager.PlayLoopingSFX(cry_small);

        // 오르골들에게 자신을 연결
        foreach (var box in myMusicBoxes)
        {
            box.linkedEnemy = this;
        }
    }

    // 울음 멈춤
    private void StopCrying()
    {
        soundManager.StopSFX();
    }

    // 큰 울음 소리
    private void StartBigCrying()
    {
        soundManager.StopSFX();
        soundManager.PlaySFX(cry_big);
        StartCoroutine(WaitAndChangeState(cry_big.length));
    }
    private IEnumerator WaitAndChangeState(float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState();
    }
    // 상태 변경
    private void ChangeState()
    {
        anim.SetBool("ChangeState", true);
    }
    public void OnChangeStateAnimationEnd()
{
    currentState = CryEnemyState.Chase;
}
    private void ChasePlayer()
    {
        // if (roomName == cryRoomName)

            Debug.Log("추격 시작!");
            Vector2 direction = (player.transform.position - transform.position).normalized;
            lastDirection = direction;

            // 이동
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 0.5f)
            {
                currentState = CryEnemyState.Attack;
            }
        // else
        // {
        //     // 방이 다르면 추적하지 않음
        //     StopChase();
        // }
    }
    

    private void StopChase()
    {
        rb.velocity = Vector2.zero;
        anim.Play("CryEnemy_Idle");
    }

    private void Attack()
    {
        Debug.Log("Attack애니메이션 작동");
        anim.SetBool("Attack", true);
    }
    public void AfterAttack()
    {
        if (Mathf.Abs(transform.position.x - player.transform.position.x) >= 0.5f)
        {
            currentState = CryEnemyState.Chase;
        }
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

    

    public void OnMusicBoxFail()
    {
        failCount++;
        if (failCount >= 3)
        {
            StartBigCrying();
        }
    }

    public void OnMusicBoxSuccess()
    {
        successCount++;
        if(successCount >=3)
        {
            StopCrying();
        }
    }
}
