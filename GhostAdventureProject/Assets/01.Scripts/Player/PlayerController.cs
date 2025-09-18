using System;
using _01.Scripts.Player;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    readonly static int Move = Animator.StringToHash("Move");

    [Header("References")] 
    [SerializeField] float moveSpeed = 5f;

    // Components
    Player player;
    Animator animator;
    SpriteRenderer mainSprite;
    
    // Parameter Caches
    Vector3 move;
    
    public Animator Animator => animator;

    public void Initialize(Player value) {
        player = value;
        animator = value.Animator;
        mainSprite = value.SpriteRenderer;
    }

    void Update() {
        if (!PossessionQTESystem.Instance ||
            PossessionQTESystem.Instance.isRunning ||
            !player.PossessionSystem.CanMove)
            return;

        HandleMovement();
        HandlePossession();
    }

    void HandlePossession() {
        if (Input.GetKeyDown(KeyCode.E)) player.PossessionSystem.TryPossess();
    }

    void OnDestroy() {
        // GameManager에 Player 파괴 알림
        GameManager.Instance?.OnPlayerDestroyed();
    }

    void HandleMovement() {
        // Get Input values
        move = GetInputValue();
        
        // Rotate Sprite which is defined by horizontal input
        mainSprite.flipX = move.x switch {
            > 0.01f => false,
            < -0.01f => true,
            _ => mainSprite.flipX
        };

        // Play Move Animation
        animator.SetBool(Move, move.magnitude > 0.01f);
        
        // Move character 
        transform.position += move * (moveSpeed * Time.deltaTime);
    }

    Vector2 GetInputValue() => new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
}