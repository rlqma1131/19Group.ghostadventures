using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using _01.Scripts.Managers.Puzzle;
using _01.Scripts.Object.NormalObject;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    /// <summary>
    /// Triggers rumble event that make the rock fall to the ground
    /// If the condition fulfills, it sets furnace on fire.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class Ch4_RumbleSwitch : BasePossessable
    {
        [Header("References of Switch")]
        [SerializeField] CinemachineImpulseSource impulseSource;

        [Header("Switch Transform")] 
        [SerializeField] GameObject switchOn;
        [SerializeField] GameObject switchOff;
        
        [Header("Brick Transform")] 
        [SerializeField] Transform brickTransform;
        [SerializeField] Vector3 endPosition;
        [SerializeField] Quaternion endRotation;
        [SerializeField, ReadOnly] Vector3 startPosition;
        [SerializeField, ReadOnly] Quaternion startRotation;
        
        [Header("Drop Animation Settings")]
        [SerializeField] AudioClip dropSound;
        [SerializeField] Ease easeMode = Ease.InQuad;
        [SerializeField] float dropDuration = 1.5f;
        [SerializeField] List<string> linesToPlayWhenNothing = new() {
            "아무 일도 일어나지 않네...",
        };
        [SerializeField] List<string> linesToPlayWhenDropped = new() {
            "밑에서 엄청난 소리가 났다.", 
            "무슨 일이 벌어졌는지 보자."
        };
        [SerializeField] UnityEvent OnDropped;
        
        [Header("UI References")]
        [SerializeField] GameObject q_Key;

        Ch4_FurnacePuzzleManager manager;
        bool alreadyPressed;

        override protected void Awake() {
            base.Awake();
            
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        override protected void Start() {
            base.Start();

            manager = Ch4_FurnacePuzzleManager.TryGetInstance();
            isScannable = true;
            startPosition = brickTransform.localPosition;
            startRotation = brickTransform.localRotation;
        }
        
        void OnDestroy() => manager = null;

        public override void TriggerEvent() {
            if (!isPossessed) { q_Key.SetActive(false); return; }
        
            if (!alreadyPressed) q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) OnInteractedSwitch();
        }

        void SetSwitchBarState(bool state) {
            switchOn.SetActive(state);
            switchOff.SetActive(!state);
        }

        void OnInteractedSwitch() {
            if (!manager) {
                Debug.LogError("Fatal Error: Puzzle Manager Not Found!");
                return;
            }
            
            Ch4_Furnace furnace = manager.Furnace;
            Sequence switchFlipSequence = DOTween.Sequence();
            
            // Switch to switchOn Sprite
            switchFlipSequence.AppendInterval(.2f);
            switchFlipSequence.JoinCallback(() => {
                q_Key.SetActive(false);
                alreadyPressed = true;
                hasActivated = false;
                isScannable = false;
            });
            switchFlipSequence.AppendCallback(() => { SetSwitchBarState(true); });
            
            // Generate Impulse after 0.5 secs passes
            switchFlipSequence.AppendInterval(.5f);
            switchFlipSequence.AppendCallback(() => {
                if (furnace.IsCandleTurnedOn && furnace.IsOiled)
                    impulseSource.GenerateImpulse();
            });
            
            // Disable Possession
            switchFlipSequence.AppendInterval(.5f);
            switchFlipSequence.AppendCallback(() => {
                if (!furnace.IsCandleTurnedOn || !furnace.IsOiled)
                    UIManager.Instance.PromptUI.ShowPrompt_2(linesToPlayWhenNothing.ToArray());
                Unpossess();
                alreadyPressed = false;
            });

            // Check if the furnace fulfilled right condition (Oiled & Candle is turned on)
            if (furnace.IsCandleTurnedOn && furnace.IsOiled) {
                switchFlipSequence.Append(brickTransform.DOLocalMove(endPosition, dropDuration).SetEase(easeMode));
                switchFlipSequence.AppendCallback(() => {
                    UIManager.Instance.PromptUI.ShowPrompt_2(linesToPlayWhenDropped.ToArray());
                    
                    brickTransform.localPosition = startPosition;
                    brickTransform.localRotation = startRotation;
                    
                    SoundManager.Instance.PlaySFX(dropSound);
                    
                    // Add Event when brick dropped
                    OnDropped?.Invoke();
                    MarkActivatedChanged();
                });
            }
            // else Reset switch
            else {
                switchFlipSequence.AppendCallback(() => {
                    hasActivated = true;
                    isScannable = true;
                    
                    brickTransform.localPosition = startPosition;
                    brickTransform.localRotation = startRotation;
                    
                    SetSwitchBarState(false);
                });
            }
        }
    }
}