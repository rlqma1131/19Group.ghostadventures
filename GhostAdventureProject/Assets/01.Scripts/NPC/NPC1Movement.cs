using UnityEngine;

public class NPC1Movement : MonoBehaviour
{
    public float patrolRange = 2f;
    public float speed = 2f;
    public float waitTime = 2f;

    [Header("Jump Settings")]
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;
    public float jumpChance = 0.1f; // 10% 확률로 점프
    public float jumpCooldown = 3f;

    [Header("Animation")]
    public Animator animator; // Inspector에서 할당

    private Vector3 startPoint;
    private int dir = 1;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private BasePossessable possessable;

    // Jump variables
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private float jumpStartY;
    private float lastJumpTime = 0f;

    // Animation parameter names
    private const string ANIM_IS_WALKING = "IsWalking";
    private const string ANIM_SPEED = "Speed";

    void Start()
    {
        startPoint = transform.position;
        possessable = GetComponent<BasePossessable>();
        jumpStartY = transform.position.y;

        // Animator가 할당되지 않았으면 자동으로 찾기
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (possessable != null && possessable.IsPossessedState)
        {
            // 플레이어 입력으로 조작 (1.5배 빠르게) - WASD로 상하좌우
            float h = Input.GetAxis("Horizontal"); // A, D
            float v = Input.GetAxis("Vertical");   // W, S
            Vector3 move = new Vector3(h, v, 0) * speed * 1.5f * Time.deltaTime;
            transform.position += move;

            // 애니메이션: 이동 속도와 걷기 상태 설정
            float moveSpeed = move.magnitude / Time.deltaTime;
            SetAnimationFloat(ANIM_SPEED, moveSpeed);
            SetAnimationBool(ANIM_IS_WALKING, moveSpeed > 0.1f);

            // 이동 방향에 따라 스프라이트 뒤집기
            if (h > 0.01f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (h < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // 점프 처리
            if (isJumping)
            {
                HandleJump();
                // 점프 중에는 걷기 애니메이션 끄기
                SetAnimationBool(ANIM_IS_WALKING, false);
                SetAnimationFloat(ANIM_SPEED, 0f);
            }
            else
            {
                // 자동 순찰 모드
                if (isWaiting)
                {
                    waitTimer += Time.deltaTime;
                    SetAnimationBool(ANIM_IS_WALKING, false);
                    SetAnimationFloat(ANIM_SPEED, 0f);

                    if (waitTimer >= waitTime)
                    {
                        isWaiting = false;
                        waitTimer = 0f;
                        dir *= -1;

                        // 방향 전환 시 점프 시도
                        TryJump();
                    }
                    return;
                }

                // 일반 이동
                transform.position += Vector3.right * dir * speed * Time.deltaTime;
                SetAnimationBool(ANIM_IS_WALKING, true);
                SetAnimationFloat(ANIM_SPEED, speed);

                // 랜덤 점프 시도 (이동 중)
                if (Time.time - lastJumpTime > jumpCooldown && Random.value < jumpChance * Time.deltaTime)
                {
                    TryJump();
                }

                if (Mathf.Abs(transform.position.x - startPoint.x) > patrolRange)
                {
                    isWaiting = true;
                    waitTimer = 0f;
                    float clampedX = startPoint.x + Mathf.Clamp((transform.position.x - startPoint.x), -patrolRange, patrolRange);
                    transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
                }
            }
        }
    }

    void TryJump()
    {
        if (!isJumping && Time.time - lastJumpTime > jumpCooldown)
        {
            isJumping = true;
            jumpTimer = 0f;
            jumpStartY = transform.position.y;
            lastJumpTime = Time.time;
        }
    }

    void HandleJump()
    {
        jumpTimer += Time.deltaTime;
        float jumpProgress = jumpTimer / jumpDuration;

        if (jumpProgress >= 1f)
        {
            // 점프 완료
            isJumping = false;
            jumpTimer = 0f;
            Vector3 pos = transform.position;
            pos.y = jumpStartY;
            transform.position = pos;
        }
        else
        {
            // 포물선 점프 (sin 곡선 사용)
            float jumpY = jumpStartY + Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
            Vector3 pos = transform.position;
            pos.y = jumpY;
            transform.position = pos;

            // 점프하면서도 계속 이동
            transform.position += Vector3.right * dir * speed * Time.deltaTime;
        }
    }

    // 애니메이션 파라미터 설정 헬퍼 함수들
    private void SetAnimationBool(string paramName, bool value)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(paramName, value);
        }
    }

    private void SetAnimationFloat(string paramName, float value)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetFloat(paramName, value);
        }
    }
}