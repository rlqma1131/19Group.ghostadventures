using System;
using _01.Scripts.Player;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    readonly static int Move = Animator.StringToHash("Move");

    [Header("References")] 
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer mainSprite;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] public BasePossessable currentTarget;

    Player player;
    Vector3 move;
    float h;
    float v;
    
    public Animator Animator => animator;

    public void Initialize(Player player) {
        this.player = player;
        animator = player.Animator;
        mainSprite = player.SpriteRenderer;
    }

    void Update()
    {
        if (PossessionSystem.Instance == null ||
            PossessionQTESystem.Instance == null ||
            !player.PossessionSystem.CanMove ||
            PossessionQTESystem.Instance.isRunning)
            return;

        HandleMovement();
        HandlePossession();
    }

    void HandlePossession() {
        if (!currentTarget) return;

        if (Input.GetKeyDown(KeyCode.E)) {
            if (CurrentTargetIsPossessable()) {
                PossessionSystem.Instance.TryPossess();
            }
            else {
                Debug.Log("빙의불가능 상태");
                currentTarget.CantPossess();
            }
        }
    }

    void OnDestroy() {
        // GameManager에 Player 파괴 알림
        if (GameManager.Instance != null) {
            GameManager.Instance.OnPlayerDestroyed();
        }
    }

    void HandleMovement() {
        // Get Input values
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        
        // Rotate Sprite which is defined by horizontal input
        mainSprite.flipX = h switch {
            > 0.01f => false,
            < -0.01f => true,
            _ => mainSprite.flipX
        };

        // Play Move Animation
        animator.SetBool(Move, move.magnitude > 0.01f);
        
        // Move character
        move = new Vector3(h, v, 0); 
        transform.position += move * (moveSpeed * Time.deltaTime);
    }

    bool CurrentTargetIsPossessable() {
        // 가까운 대상이 빙의 가능 상태인지 확인
        return currentTarget != null
            && player.InteractSystem.CurrentClosest == currentTarget.gameObject
            && !currentTarget.IsPossessedState
            && currentTarget.HasActivated;
    }
}