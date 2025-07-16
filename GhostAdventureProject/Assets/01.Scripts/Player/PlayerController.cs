using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public BasePossessable currentTarget;

    public Animator animator { get; private set; }

    private void Start()
    {
        animator = GetComponent<Animator>();

        // GameManager에 Player 등록
        if (GameManager.Instance != null)
        {
            // GameManager의 SpawnPlayer에서 이미 처리되므로 여기서는 추가 확인만
            Debug.Log("[PlayerController] Player 초기화 완료");
        }
    }

    void Update()
    {
        if (PossessionSystem.Instance == null ||
            PossessionQTESystem.Instance == null ||
            !PossessionSystem.Instance.CanMove ||
            PossessionQTESystem.Instance.isRunning)
            return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.E) && CurrentTargetIsPossessable())
        {
            PossessionSystem.Instance.TryPossess();
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;

        // 회전
        if (h > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1);
            FlipEKey(true);
        }
        else if (h < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            FlipEKey(false);
        }

        bool isMoving = move.magnitude > 0.01f;
        animator.SetBool("Move", isMoving);
    }

    private void FlipEKey(bool facingRight)
    {
        if (PlayerInteractSystem.Instance != null && PlayerInteractSystem.Instance.CurrentClosest != null)
        {
            var eKey = PlayerInteractSystem.Instance.GetEKey();
            if (eKey != null)
            {
                Vector3 scale = eKey.transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                eKey.transform.localScale = scale;
            }
        }
    }

    private bool CurrentTargetIsPossessable()
    {
        // 가까운 대상이 빙의 가능 상태인지 확인
        return currentTarget != null
            && PlayerInteractSystem.Instance.CurrentClosest == currentTarget.gameObject
            && currentTarget.HasActivated
            && !currentTarget.IsPossessedState;
    }

    private void OnDestroy()
    {
        // GameManager에 Player 파괴 알림
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDestroyed();
        }

        Debug.Log("[PlayerController] Player 파괴됨 - GameManager에 알림 전송");
    }
}