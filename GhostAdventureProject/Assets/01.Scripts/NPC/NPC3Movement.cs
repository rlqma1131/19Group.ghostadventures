using UnityEngine;

public class NPC3Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float possessedSpeedMultiplier = 1.5f;

    [Header("Patrol Settings")]
    public float patrolMinX = -5f;
    public float patrolMinY = 0f;
    public float patrolCenterX = 0f;
    public float patrolCenterY = 0f;
    public float patrolMaxX = 6f;
    public float patrolMaxY = 0f;
    public float waitTimeAtMin = 1f;
    public float waitTimeAtCenter = 1f;
    public float waitTimeAtMax = 2f;

    [Header("Animation")]
    public Animator animator;

    private BasePossessable possessable;
    private Vector3[] waypoints;
    private float[] waitTimes;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        possessable = GetComponent<BasePossessable>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // Inspector 설정값으로 웨이포인트 생성
        UpdateWaypoints();

        // 첫 번째 웨이포인트로 이동
        transform.position = new Vector3(waypoints[0].x, waypoints[0].y, transform.position.z);
        StartWaiting();
    }

    void UpdateWaypoints()
    {
        waypoints = new Vector3[] {
            new Vector3(patrolMinX, patrolMinY, 0),
            new Vector3(patrolCenterX, patrolCenterY, 0),
            new Vector3(patrolMaxX, patrolMaxY, 0)
        };
        waitTimes = new float[] { waitTimeAtMin, waitTimeAtCenter, waitTimeAtMax };
    }

    void Update()
    {
        if (possessable != null && possessable.IsPossessedState)
        {
            HandlePlayerControl();
        }
        else
        {
            HandleAIMovement();
        }
    }

    void HandlePlayerControl()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0) * speed * possessedSpeedMultiplier * Time.deltaTime;
        transform.position += move;

        // 애니메이션 설정
        bool isMovingNow = move.magnitude > 0.1f;
        SetAnimation("Move", isMovingNow);

        // 스프라이트 방향
        if (h > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (h < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleAIMovement()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            SetAnimation("Move", false);

            if (waitTimer >= waitTimes[currentWaypointIndex])
            {
                StartMoving();
            }
        }
        else if (isMoving)
        {
            MoveToNextWaypoint();
            SetAnimation("Move", true);
        }
    }

    void StartWaiting()
    {
        isWaiting = true;
        isMoving = false;
        waitTimer = 0f;
    }

    void StartMoving()
    {
        isWaiting = false;
        isMoving = true;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void MoveToNextWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        targetPosition.z = transform.position.z; // Z축은 현재 위치 유지
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance < 0.1f)
        {
            transform.position = targetPosition;
            StartWaiting();
        }
        else
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // 이동 방향에 따라 스프라이트 뒤집기
            if (direction.x > 0.01f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void SetAnimation(string paramName, bool value)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(paramName, value);
        }
    }

    // Inspector에서 실시간으로 값 변경할 때 호출 (에디터용)
    void OnValidate()
    {
        if (Application.isPlaying && waypoints != null)
        {
            UpdateWaypoints();
        }
    }
}