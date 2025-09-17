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
    [Header("사운드")]
    [SerializeField] private AudioClip cry_small;               // 기본울음 소리
    [SerializeField] private AudioClip cry_big;                 // 큰울음 소리
    [SerializeField] private AudioClip smile;                   // 웃음 소리
    [SerializeField] private AudioClip successQTE_Sound;        // 오르골소리(3개 모두 작동 성공시)

    [Header("퍼즐")]
    [SerializeField] private ClueData needClue;                 // 원하는 단서
    [SerializeField] private GameObject speechBubble;           // 말풍선
    [SerializeField] private List<Ch3_MusicBox> myMusicBoxes;   // 연결된 3개의 오르골
    public HashSet<Ch3_MusicBox> clearedBoxes = new HashSet<Ch3_MusicBox>(); // 작동 성공한 오르골
    private string playerInRoomName;                            // 플레이어가 있는 방의 이름
    [SerializeField] string cryEnemyInRoomName;                 // 울보가 있는 방의 이름
    [SerializeField] LockedDoor door;                           // 방 문
    
    [Header("울보 설정")]
    [SerializeField] private float moveSpeed = 4;               // 스피드
    [SerializeField] private float chaseRange;                  // 추격 범위
    [SerializeField] private float attackRange;                 // 공격 범위

    private GameObject player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private SoundManager soundManager;
    private Animator anim;

    private bool isCrying = false;                              // 울고 있는지 확인
    private bool playSound_successQTE = false;                  // 오르골소리 플레이 여부 확인
    private bool clear = false;                                 // 오르골3개 성공 확인
    public int failCount = 0;                                   // QTE 실패 횟수
    private int successCount = 0;                               // QTE 성공 횟수
    private CryEnemyState currentState;                         // 상태

    [SerializeField] private GameObject cryingtrigger;
    private Vector2 lastDirection;
    private bool attackMode = false;


    void Start()
    {   
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        soundManager = SoundManager.Instance;
        
        cryingtrigger.SetActive(false);
        currentState = CryEnemyState.Known;
    }

    void Update()
    {
        playerInRoomName = player.GetComponentInChildren<Global_PlayerRoomTracker>().roomName_RoomTracker;
        if (cryEnemyInRoomName == playerInRoomName && !clear && !isCrying)
        {
            StartCrying();
        }
        else if (cryEnemyInRoomName != playerInRoomName && isCrying)
        {
            soundManager.StopLoopingSFX();
            isCrying = false;
        }
        else if (cryEnemyInRoomName != playerInRoomName && clear && !isCrying)
        {
            soundManager.StopLoopingSFX();
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
                AttackPlayer();
                break;
        }
    }

    // 울보가 있는 방과 플레이어가 있는 방이 같으면 울기 시작
    public void StartCrying()
    {
        isCrying = true;
        
        door.UnlockPair();  // 락도어 - 열림     
        // clearedBoxes.Clear();
        cryingtrigger.SetActive(true);
        soundManager.PlayLoopingSFX(cry_small);
        // 오르골들에게 자신을 연결
        foreach (var box in myMusicBoxes)
        {
            box.linkedEnemy = this;
        }
    }

    // 울음 멈춤 -> 웃음
    private void StopCryingAndSmile()
    {
        isCrying = false;
        clear = true;

        soundManager.StopLoopingSFX();                      // 기본울음 멈춤
        soundManager.PlayLoopingSFX(smile);                 // 웃음소리 플레이
        soundManager.PlayLoopingSFX(successQTE_Sound);      // 오르골 소리 플레이
        playSound_successQTE = true;                        // 오르골 소리 플레이 상태임

        anim.SetBool("Jump", true);
        cryingtrigger.SetActive(false);
    }

    // 오르골3개 실패시 - 큰 울음 소리
    private void StartBigCrying()
    {
        door.LockPair();

        soundManager.StopLoopingSFX();
        soundManager.PlaySFX(cry_big);
        StartCoroutine(WaitAndChangeState(cry_big.length));
        
        attackMode = true;
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

    // ChangeState 애니메이션 종료시 작동
    public void OnChangeStateAnimationEnd()
    {
        if(attackMode)
        {
            anim.SetBool("ChangeState", false);
            currentState = CryEnemyState.Chase;
        }
        else
            anim.SetBool("BackState", true);
            anim.SetBool("ChangeState", false);
    }

    // BackState 애니메이션 종료시 작동
    public void OnBackeStateAnimationEnd()
    {
        anim.SetBool("BackState", false);
    }

    private void ChasePlayer()
    {
        if (playerInRoomName == cryEnemyInRoomName)
        {
            anim.SetBool("Chase", true);
            Vector2 direction = (player.transform.position - transform.position).normalized;
            lastDirection = direction;
            // 이동
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
            
            if(transform.position.x - player.transform.position.x >0)
                sr.flipX = true;
            else
                sr.flipX = false;

            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 0.1f)
            {
                currentState = CryEnemyState.Attack;
            }
        }
    }

    private void AttackPlayer()
    {
        anim.SetBool("Attack", true);

        if (Mathf.Abs(transform.position.x - player.transform.position.x) >= 0.2f)
        {
            anim.SetBool("Attack", false);
            currentState = CryEnemyState.Chase;
        }
        else anim.SetBool("Attack", true);
    }

    // Attack 애니메이션 종료시 작동
    public void AfterAttack()
    {   
        PlayerLifeManager.Instance.currentPlayerLives = 1;  // 공격당하면 무조건 죽음
        PlayerLifeManager.Instance.LosePlayerLife();
    }

    public void OnMusicBoxSuccess()
    {
        successCount++;
        if(successCount >=3)
        {
            StopCryingAndSmile();
        }
    }
    public void OnMusicBoxFail()
    {
        failCount++;
        if (failCount >= 5)
        {
            StartBigCrying();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && currentState == CryEnemyState.Known && !clear)
        {
            anim.SetBool("ChangeState", true);
            soundManager.PlaySFX(cry_big);
        }
    }
}
