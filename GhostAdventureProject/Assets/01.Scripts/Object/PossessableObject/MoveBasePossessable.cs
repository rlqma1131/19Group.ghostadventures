using Cinemachine;
using UnityEngine;

public class MoveBasePossessable : BasePossessable
{
    [Header("Movable BasePossessable References")]
    [SerializeField] protected CinemachineVirtualCamera zoomCamera;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected SpriteRenderer highlightSpriteRenderer;

    override protected void Awake() {
        base.Awake();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        highlightSpriteRenderer = Highlight?.GetComponent<SpriteRenderer>(); 
        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        if (!isPossessed || !PossessionSystem.Instance.CanMove)
            return;
        
        Move();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;
            Unpossess();
            anim.SetBool("Move", false);
        }
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;
    }

    protected virtual void Move()
    {
        float h = Input.GetAxis("Horizontal");

        Vector3 move = new Vector3(h, 0, 0);

        // 이동 여부 판단
        bool isMoving = move.sqrMagnitude > 0.01f;

        if (anim != null)
        {
            anim.SetBool("Move", isMoving);
        }

        if (isMoving)
        {
            transform.position += move * moveSpeed * Time.deltaTime;

            // 좌우 Flip
            if (spriteRenderer != null && Mathf.Abs(h) > 0.01f)
            {
                spriteRenderer.flipX = h < 0f;
            }
        }
    }

    protected virtual void OnDoorInteract()
    { // 자식클래스에서 설정
    }
}
