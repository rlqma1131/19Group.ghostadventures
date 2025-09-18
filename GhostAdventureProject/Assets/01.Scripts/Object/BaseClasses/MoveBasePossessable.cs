using _01.Scripts.Object.BaseClasses.Interfaces;
using Cinemachine;
using UnityEngine;

public class MoveBasePossessable : BasePossessable, IMovable
{
    protected readonly static int MoveHash = Animator.StringToHash("Move");

    [Header("Movable BasePossessable References")] [SerializeField]
    protected CinemachineVirtualCamera zoomCamera;

    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected SpriteRenderer highlightSpriteRenderer;
    [SerializeField] protected bool allowVerticalMovement;

    override protected void Awake() {
        base.Awake();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        highlightSpriteRenderer = Highlight?.GetComponent<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    override protected void Update() {
        if (!isPossessed || !player.PossessionSystem.CanMove) return;

        Move();

        if (Input.GetKeyDown(KeyCode.E)) {
            zoomCamera.Priority = 5;
            Unpossess();
            anim.SetBool(MoveHash, false);
        }
    }

    /// <summary>
    /// Called when the user possesses this object in every frame
    /// </summary>
    public virtual void Move() {
        Vector3 move = GetInputVector();

        // 이동 여부 판단
        bool isMoving = move.sqrMagnitude > 0.01f;
        if (anim) anim.SetBool(MoveHash, isMoving);

        if (isMoving) {
            transform.position += move * (moveSpeed * Time.deltaTime);

            // 좌우 Flip
            if (spriteRenderer && Mathf.Abs(move.x) > 0.01f) spriteRenderer.flipX = move.x < 0f;
        }
    }

    public virtual Vector2 GetInputVector() =>
        new(Input.GetAxis("Horizontal"), allowVerticalMovement ? Input.GetAxis("Vertical") : 0f);

    public override void OnPossessionEnterComplete() {
        base.OnPossessionEnterComplete();
        zoomCamera.Priority = 20;
    }

    /// <summary>
    /// Need to be overriden by child class 
    /// </summary>
    protected virtual void OnDoorInteract() { }
}