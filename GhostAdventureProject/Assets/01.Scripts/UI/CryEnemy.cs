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
    [SerializeField] private AudioClip cry_small; // 기본울음
    [SerializeField] private AudioClip cry_big; // 큰울음
    [SerializeField] private AudioClip smile; // 웃음
    [SerializeField] private AudioClip successQTE_Sound; // 오르골소리(3개 모두 작동시)
    [SerializeField] private AudioSource cryMusic;

    [SerializeField] private ClueData needClue; // 원하는 단서
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
    private bool playSound_successQTE = false;
    public int failCount = 0;
    private int successCount = 0;
    [SerializeField] private GameObject cryingtrigger;

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
        anim = GetComponentInChildren<Animator>();
        soundManager = SoundManager.Instance;
        cryingtrigger.SetActive(false);

        currentState = CryEnemyState.Known;
        // StartCrying(); // 임시
    }

    void Update()
    {
        roomName = player.GetComponentInChildren<PlayerRoomTracker>().roomName_RoomTracker;
        if (cryRoomName == roomName && !isCrying)
        {
            StartCrying();
        }
        // if (clearedBoxes.Count >= 3 && cryRoomName != roomName && playSound_successQTE)
        // {
        //     soundManager.StopSFX();
        //     playSound_successQTE = false;
        // }
        // else if(clearedBoxes.Count >= 3 && cryRoomName == roomName && !playSound_successQTE)
        // {
        //     soundManager.PlaySFX(successQTE_Sound);
        //     playSound_successQTE = true;
        // }
    }

    void FixedUpdate()
    {
        switch(currentState)
        {
            case CryEnemyState.Chase:
                // if (roomName != cryRoomName)
                // {
                //     StopChase();
                //     return;
                // }
                ChasePlayer();
                break;

            case CryEnemyState.Attack:
                // if (roomName != cryRoomName)
                // {
                //     StopChase();
                //     return;
                // }

                
                AttackPlayer();
                break;
        }
    }

    // 울보가 있는 방과 플레이어가 있는 방이 같으면 울기 시작
    public void StartCrying()
    {
        isCrying = true;
        clearedBoxes.Clear();
        soundManager.PlayLoopingSFX(cry_small);
        cryingtrigger.gameObject.SetActive(true);
        cryingtrigger.SetActive(true);

        // 오르골들에게 자신을 연결
        foreach (var box in myMusicBoxes)
        {
            box.linkedEnemy = this;
        }
    }

    // 울음 멈춤
    private void StopCryingAndSmile()
    {
        soundManager.StopLoopingSFX();
        soundManager.PlaySFX(smile);
        soundManager.PlaySFX(successQTE_Sound);
        playSound_successQTE = true;
        anim.SetBool("Jump", true);
        cryingtrigger.SetActive(false);
    }

    private void StopSuccessQTE_Sound()
    {
        soundManager.StopSFX();
        playSound_successQTE = false;
    }

    // 오르골3개 실패시 - 큰 울음 소리
    private void StartBigCrying()
    {
        soundManager.StopLoopingSFX();
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

    // ChangeState 애니메이션 종료시 작동
    public void OnChangeStateAnimationEnd()
    {
        anim.SetBool("ChangeState", false);
        currentState = CryEnemyState.Chase;
    }

    private void ChasePlayer()
    {
        if (roomName == cryRoomName)
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
    // else
    // {
    //     // 방이 다르면 추적하지 않음
    //     StopChase();
    // }
    }
    

    // private void StopChase()
    // {
    //     rb.velocity = Vector2.zero; // 혹시라도 움직이는 중이라면 멈추기
    //     anim.SetBool("Chase", false);
    //     anim.SetBool("Attack", false);
    //     anim.Play("CryEnemy_Idle"); // Idle 애니메이션
    //     currentState = CryEnemyState.Known; // 다시 대기 상태로 전환
    // }

    private void AttackPlayer()
    {
        anim.SetBool("Attack", true);
        Debug.Log("AfterAttack");
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
        PlayerLifeManager.Instance.currentPlayerLives = 1;
        PlayerLifeManager.Instance.LosePlayerLife();
    }

    public void OnMusicBoxSuccess(Ch3_MusicBox box)
    {
        if (!clearedBoxes.Contains(box))
        {
            clearedBoxes.Add(box);
            Debug.Log($"울보 오르골 {clearedBoxes.Count}개 해제됨");

            if (clearedBoxes.Count >= 3)
            {
                StopCryingAndSmile();
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
            StopCryingAndSmile();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && currentState == CryEnemyState.Known)
        {
            // soundManager.StopLoopingSFX();
            soundManager.PlaySFX(cry_big);
            // soundManager.PlayLoopingSFX(cry_small);
        }
    }
}
