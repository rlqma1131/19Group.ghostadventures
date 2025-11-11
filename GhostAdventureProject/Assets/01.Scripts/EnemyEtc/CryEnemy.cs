using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Ch3에 나오는 적 울보 스크립트입니다.
///     울보는 플레이어가 방에 들어오면 울기 시작합니다. 방문 바깥에 사신이 모여듭니다(cryingtrigger).
///     울보의 울음을 멈추기 위해서는 오르골 3개를 작동시켜야 합니다.
///     오르골3개를 작동시키면 울보는 울음을 멈추고 웃습니다.
///     오르골 QTE를 5번 틀리면 울보는 비명을 지르며 플레이어를 공격합니다. 방문은 나갈 수 없게 잠깁니다. 무조건 게임오버됩니다.
///     CryEnemy와 MusicBox 스크립트를 확인해주세요.
/// </summary>
public class CryEnemy : MonoBehaviour
{
    public enum CryEnemyState
    {
        Idle,
        Chase,
        Attack
    }

    [Header("사운드")] 
    [SerializeField] AudioClip cry_Sound; // 울음 소리
    [SerializeField] AudioClip scream_Sound; // 비명 소리
    [SerializeField] AudioClip smile_Sound; // 웃음 소리
    [SerializeField] AudioClip musicBox_Sound; // 오르골소리(3개 모두 작동 성공시)

    [Header("퍼즐")] 
    [SerializeField] ItemData wantItem; // 원하는 아이템
    [SerializeField] SpriteRenderer thoughtBubble; // 말풍선
    [SerializeField] Image ItemIcon; // 원하는 아이템 아이콘이 표시될 Image
    [SerializeField] List<Ch3_MusicBox> myMusicBoxes; // 연결된 3개의 오르골
    [SerializeField] LockedDoor door; // 방 문
    string cryEnemyInRoomName; // 울보가 있는 방의 이름
    string playerInRoomName; // 플레이어가 있는 방의 이름

    [Header("울보 설정")] 
    [SerializeField] float moveSpeed = 4; // 스피드

    Player player;
    Rigidbody2D rb;
    SpriteRenderer sr;
    SoundManager soundManager;
    Animator anim;

    bool attackMode; // 공격모드
    bool isCrying; // 울고 있는지 확인
    bool isSmile; // 웃고 있는지 확인
    public int qteFailCount; // QTE 실패 횟수
    int musicboxPlayCount; // 오르골 작동 횟수
    CryEnemyState currentState; // 상태

    [SerializeField] GameObject cryingtrigger;

    void Start() {
        player = GameManager.Instance.Player;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        soundManager = SoundManager.Instance;

        cryingtrigger.SetActive(false);
        currentState = CryEnemyState.Idle;

        if (ItemIcon == null) return;
        if (wantItem == null)
            ItemIcon.sprite = null;
        else
            ItemIcon.sprite = wantItem.Item_Icon;
    }

    // 플레이어가 있는 방이 어딘지 계속 체크 -> 플레이어가 방에 들어오면 울기 시작. 
    void Update() {
        playerInRoomName = player.GetComponentInChildren<Global_PlayerRoomTracker>().roomName_RoomTracker;
        if (cryEnemyInRoomName == playerInRoomName && !isSmile && !isCrying) {
            StartCrying();
        }
        else if (cryEnemyInRoomName != playerInRoomName && isCrying) {
            isCrying = false;
        }
    }

    void FixedUpdate() {
        switch (currentState) {
            case CryEnemyState.Chase:
                ChasePlayer();
                break;

            case CryEnemyState.Attack:
                AttackPlayer();
                break;
        }
    }

    // 울기 시작
    public void StartCrying() {
        isCrying = true;

        door.UnlockDoors();
        cryingtrigger.SetActive(true);
        soundManager.StopSFX();
        soundManager.ChangeBGM(cry_Sound);

        // 오르골들에게 자신을 연결
        foreach (Ch3_MusicBox box in myMusicBoxes) {
            box.linkedEnemy = this;
        }
    }

    // 울음 멈춤 -> 웃음
    void StopCryingAndSmile() {
        isCrying = false;
        isSmile = true;

        soundManager.PlaySFX(smile_Sound);
        soundManager.ChangeBGM(musicBox_Sound);

        anim.SetBool("Jump", true);
        cryingtrigger.SetActive(false);
    }

    // 크게 울기 시작
    void StartBigCrying() {
        door.LockDoors(); // 문 - 닫힘.
        soundManager.StopBGM();
        soundManager.PlaySFX(scream_Sound);
        StartCoroutine(WaitAndChangeState(scream_Sound.length));
        attackMode = true;
    }

    IEnumerator WaitAndChangeState(float delay) {
        yield return new WaitForSeconds(delay);
        ChangeState();
    }

    // 상태 변경
    void ChangeState() {
        anim.SetBool("ChangeState", true);
    }

    // ChangeState 애니메이션 종료시 작동
    public void OnChangeStateAnimationEnd() {
        if (attackMode) {
            anim.SetBool("ChangeState", false);
            currentState = CryEnemyState.Chase;
        }
        else {
            anim.SetBool("BackState", true);
        }

        anim.SetBool("ChangeState", false);
    }

    // BackState 애니메이션 종료시 작동
    public void OnBackeStateAnimationEnd() {
        anim.SetBool("BackState", false);
    }

    // 플레이어를 쫓음
    void ChasePlayer() {
        if (playerInRoomName == cryEnemyInRoomName) {
            anim.SetBool("Chase", true);
            Vector2 direction = (player.transform.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * (moveSpeed * Time.deltaTime));

            if (transform.position.x - player.transform.position.x > 0)
                sr.flipX = true;
            else
                sr.flipX = false;

            if (Mathf.Abs(transform.position.x - player.transform.position.x) < 0.1f) {
                currentState = CryEnemyState.Attack;
            }
        }
    }

    // 플레이어를 공격함
    void AttackPlayer() {
        anim.SetBool("Attack", true);

        if (Mathf.Abs(transform.position.x - player.transform.position.x) >= 0.2f) {
            anim.SetBool("Attack", false);
            currentState = CryEnemyState.Chase;
        }
    }

    // Attack 애니메이션 종료시 작동
    public void AfterAttack() {
        player.Condition.HandleGameOver(); // 게임오버
    }

    // 오르골 작동 성공했을 때
    public void OnMusicBoxPlaySuccess() {
        musicboxPlayCount++;
        if (musicboxPlayCount >= 3) {
            StopCryingAndSmile();
        }
    }

    // 오르골 QTE 실패했을 때
    public void OnMusicBoxQteFail() {
        qteFailCount++;
        if (qteFailCount >= 5) {
            StartBigCrying();
        }
    }

    public bool IsSmile() => isSmile;
    public bool IsAttackMode() => attackMode;

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && currentState == CryEnemyState.Idle && !isSmile) {
            anim.SetBool("ChangeState", true);
            soundManager.PlaySFX(scream_Sound);
        }

        if (collision.CompareTag("Room") && cryEnemyInRoomName == null) {
            cryEnemyInRoomName = collision.GetComponent<Global_RoomInfo>().roomName;
        }
    }
}