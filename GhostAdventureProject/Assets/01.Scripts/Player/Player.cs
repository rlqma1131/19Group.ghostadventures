using System;
using UnityEngine;

namespace _01.Scripts.Player
{
    public class Player : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Rigidbody2D rb;
        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer spriteRenderer;
        
        [Header("Related Components")]
        [SerializeField] PlayerController controller;
        [SerializeField] PlayerCamera playerCam;
        [SerializeField] PlayerCondition condition;
        [SerializeField] PlayerInteractSystem interactSystem;
        [SerializeField] PlayerSoulEnergy soulEnergy;
        [SerializeField] PossessionSystem possessionSystem;
        [SerializeField] MemoryScan memoryScan;

        // Player Related Component Properties
        public PlayerController Controller => controller;
        public PlayerCamera PlayerCamera => playerCam;
        public PlayerCondition Condition => condition;
        public PlayerInteractSystem InteractSystem => interactSystem;
        public PlayerSoulEnergy SoulEnergy => soulEnergy;
        public PossessionSystem PossessionSystem => possessionSystem;
        public MemoryScan MemoryScan => memoryScan;
        
        // Component Properties
        public Animator Animator => animator;
        public Rigidbody2D Rigidbody => rb;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        
        void Awake() => Initialize();

        void Reset() => Initialize();

        void Start() {
            controller.Initialize(this);
            memoryScan.Initialize(this);
            interactSystem.Initialize(this);
            possessionSystem.Initialize(this);
            condition.Initialize(this);
            
            if (!GameManager.Instance.PlayerObj)
                Debug.LogError("Fatal Error! : GameManager에 Player GameObject 등록이 안되어 있습니다!");
        }

        void Initialize() {
            if (!rb) rb = GetComponent<Rigidbody2D>();
            if (!animator) animator = GetComponent<Animator>();
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            if (!controller) controller = GetComponent<PlayerController>();
            if (!playerCam) playerCam = GetComponent<PlayerCamera>();
            if (!condition) condition = GetComponent<PlayerCondition>();
            if (!soulEnergy) soulEnergy = GetComponent<PlayerSoulEnergy>();
            if (!possessionSystem) possessionSystem = GetComponent<PossessionSystem>();
            if (!interactSystem) interactSystem = GetComponent<PlayerInteractSystem>();
            if (!memoryScan) memoryScan = GetComponent<MemoryScan>();
        }
    }
}