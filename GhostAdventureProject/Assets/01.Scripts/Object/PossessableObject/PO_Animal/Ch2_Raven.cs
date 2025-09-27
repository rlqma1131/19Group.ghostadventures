using UnityEngine;

public class Ch2_Raven : MoveBasePossessable
{
    [Header("Raven Object References")]
    [SerializeField] Animator highlightAnim;
    [SerializeField] Ch2_SandCastle SandCastle; // 모래성
    [SerializeField] Transform startSpot; // 스타트지점 (빙의해제시 되돌아감)

    bool sandCastleBreakAble;
    Rigidbody2D rb;

    override protected void Awake() {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    override protected void Start() {
        base.Start();
        hasActivated = true;
    }

    override protected void Update() {
        if (!hasActivated) return;

        if (sandCastleBreakAble) {
            Vector2 catPos = transform.position;
            catPos.y += 0.5f;
            // q_Key.SetActive(true);
        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.Q) && isPossessed)
            anim.SetTrigger("Attack");
    }

    void FixedUpdate() {
        if (isPossessed) return;

        Vector2 direction = startSpot.position - transform.position;

        // 거리가 충분히 가까우면 멈춤
        if (direction.magnitude <= 0.05f) {
            anim.SetBool(MoveHash, false);
            rb.velocity = Vector2.zero; // 혹시 남아있는 물리속도가 있을 경우
            return;
        }

        // 방향 벡터 정규화 후 이동
        direction.Normalize();
        rb.MovePosition(rb.position + direction * ((moveSpeed - 1f) * Time.fixedDeltaTime));

        anim.SetBool(MoveHash, true);

        // 방향에 따라 스프라이트 뒤집기
        spriteRenderer.flipX = startSpot.position.x < transform.position.x;
    }

    public override void Move() {
        Vector3 move = GetInputVector();

        // 이동 여부 판단
        bool isMoving = move.sqrMagnitude > 0.01f;
        if (anim) anim.SetBool(MoveHash, isPossessed || isMoving);

        if (isMoving) {
            transform.position += move * (moveSpeed * Time.deltaTime);

            // 좌우 Flip
            if (spriteRenderer && Mathf.Abs(move.x) > 0.01f) spriteRenderer.flipX = move.x < 0f;
        }
    }

    public override void OnQTESuccess() {
        player.SoulEnergy.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    override protected void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if (collision.gameObject == SandCastle.gameObject) {
            sandCastleBreakAble = true;
        }
    }

    override protected void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.gameObject == SandCastle.gameObject) {
            sandCastleBreakAble = false;
        }
    }

    public override void Unpossess()
    {
        base.Unpossess();
        if(SandCastle.IsCrumbled()) {
            hasActivated = false;
            MarkActivatedChanged();
        }
    }
}