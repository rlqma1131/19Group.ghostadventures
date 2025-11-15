using System;
using _01.Scripts.Player;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    readonly static int Move = Animator.StringToHash("Move");

    [Header("References")] 
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float slowSpeed = 2f;
    [SerializeField] bool isSlowdownAvailable = false;

    // Components
    Player player;
    Animator animator;
    SpriteRenderer mainSprite;
    
    // Parameter Caches
    Vector3 move;
    float currentSpeed;
    
    public Animator Animator => animator;
    public bool IsSlowdownActive { get; private set; }
    public float CurrentSpeed => (move * currentSpeed).magnitude;
    public float MinSpeed => slowSpeed;

    public void Initialize(Player value) {
        player = value;
        animator = value.Animator;
        mainSprite = value.SpriteRenderer;

        currentSpeed = moveSpeed;
    }

    void Update() {
        if (!player.PossessionSystem.CanMove)
            return;

        if (!PossessionQTESystem.Instance || 
            PossessionQTESystem.Instance.isRunning)
            return;

        HandleMovement();
        HandlePossession();
    }
    
    void OnDestroy() {
        // GameManager에 Player 파괴 알림
        GameManager.Instance?.OnPlayerDestroyed();
    }
    
    public void SetSlowdownAvailable(bool value) => isSlowdownAvailable = value;
    
    public bool GetSlowdownAvailable() => isSlowdownAvailable;
    
    void HandlePossession() {
        if (Input.GetKeyDown(KeyCode.E)) player.PossessionSystem.TryPossess();
    }

    void HandleMovement() {
        // Get Input values
        move = GetInputValue();

        // Slow down by input && Fulfilled condition
        if (isSlowdownAvailable) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                currentSpeed = slowSpeed;
                IsSlowdownActive = !IsSlowdownActive;
                currentSpeed = IsSlowdownActive ? slowSpeed : moveSpeed;
            }
        }
        else {
            if (IsSlowdownActive) {
                IsSlowdownActive = false;
                currentSpeed = moveSpeed;
            }
        }
        
        // Rotate Sprite which is defined by horizontal input
        mainSprite.flipX = move.x switch {
            > 0.01f => false,
            < -0.01f => true,
            _ => mainSprite.flipX
        };

        // Play Move Animation
        animator.SetBool(Move, move.magnitude > 0.01f);
        
        // Move character 
        transform.position += move * (currentSpeed * Time.deltaTime);
    }

    Vector2 GetInputValue() => new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
}