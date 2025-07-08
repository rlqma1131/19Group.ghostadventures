using UnityEngine;

public class NPC2Movement : MonoBehaviour
{
    [Header("Start Position")]
    public Vector3 startPosition = new Vector3(0f, -1.5f, 0f);

    [Header("Movement Settings")]
    public float speed = 1.5f;
    public float moveRange = 0.25f; // 시작위치 기준 ±0.25 범위

    [Header("Animation")]
    public Animator animator; // Inspector에서 할당

    private Vector3 targetPoint;
    private float waitTime = 0;
    private BasePossessable possessable;

    // Animation parameter names
    private const string ANIM_IS_WALKING = "IsWalking";
    private const string ANIM_SPEED = "Speed";

    void Start()
    {
        // 설정된 시작 위치로 이동
        transform.position = startPosition;

        possessable = GetComponent<BasePossessable>();
        PickNewTarget();

        // Animator가 할당되지 않았으면 자동으로 찾기
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (possessable != null && possessable.IsPossessedState)
        {
            // 플레이어 조작 (1.5배 빠르게) - WASD로 상하좌우
            float h = Input.GetAxis("Horizontal"); // A, D
            float v = Input.GetAxis("Vertical");   // W, S
            Vector3 move = new Vector3(h, v, 0) * speed * 1.5f * Time.deltaTime;

            Vector3 newPos = transform.position + move;
            ClampToStartArea(ref newPos);
            transform.position = newPos;

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
            // AI 패턴: 시작위치 근처에서 랜덤하게 이동
            if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
            {
                waitTime += Time.deltaTime;
                SetAnimationBool(ANIM_IS_WALKING, false);
                SetAnimationFloat(ANIM_SPEED, 0f);

                if (waitTime > 1f)
                {
                    PickNewTarget();
                    waitTime = 0;
                }
            }
            else
            {
                Vector3 dir = (targetPoint - transform.position).normalized;
                Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
                ClampToStartArea(ref newPos);
                transform.position = newPos;

                // 애니메이션: 걷기 상태 설정
                SetAnimationBool(ANIM_IS_WALKING, true);
                SetAnimationFloat(ANIM_SPEED, speed);
            }
        }
    }

    void PickNewTarget()
    {
        // 시작위치 기준 ±0.25 범위 내에서 랜덤 타겟 선택
        float newX = startPosition.x + Random.Range(-moveRange, moveRange);
        float newY = startPosition.y + Random.Range(-moveRange, moveRange);
        float newZ = startPosition.z; // Z축은 고정

        targetPoint = new Vector3(newX, newY, newZ);
    }

    // 시작위치 기준 범위 내로 제한
    void ClampToStartArea(ref Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, startPosition.x - moveRange, startPosition.x + moveRange);
        position.y = Mathf.Clamp(position.y, startPosition.y - moveRange, startPosition.y + moveRange);
        position.z = startPosition.z; // Z축 고정
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
