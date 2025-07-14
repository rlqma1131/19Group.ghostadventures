using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public BasePossessable currentTarget;
    
    public Animator animator { get; private set; }
    private PlayerHide playerHide;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        playerHide = GetComponent<PlayerHide>();
    }

    void Update()
    {
        if (PossessionSystem.Instance == null||
            PossessionQTESystem.Instance == null ||
            !PossessionSystem.Instance.CanMove ||
            PossessionQTESystem.Instance.isRunning)
            return;

        if (playerHide.IsHiding)
            return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.E) && CurrentTargetIsPossessable())
        {
            PossessionSystem.Instance.TryPossess();
        }
    }

    private void HandleMovement() // 기본 이동 처리
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        transform.position += move * moveSpeed * Time.deltaTime;
        
        // 회전
        if (h > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (h < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        
        bool isMoving = move.magnitude > 0.01f;
        animator.SetBool("Move", isMoving);
    }

    private bool CurrentTargetIsPossessable()
    {
        // 가까운 대상이 빙의 가능 상태인지 확인
        return currentTarget != null
            && PlayerInteractSystem.Instance.CurrentClosest == currentTarget.gameObject
            && currentTarget.HasActivated
            && !currentTarget.IsPossessedState;
    }
}
