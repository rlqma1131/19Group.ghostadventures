using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using _01.Scripts.Object.NormalObject;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class Ch4_Switch : BasePossessable
    {
        [Header("References of Switch")]
        [SerializeField] CinemachineImpulseSource impulseSource;
        [SerializeField] Ch4_Furnace furnace;

        [Header("Switch Transform")] 
        [SerializeField] SpriteRenderer switchBar;
        [SerializeField] Sprite spriteOn;
        [SerializeField] Sprite spriteOff;
        
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
        [SerializeField] List<string> linesToPlayWhenDropped = new() {
            "밑에서 엄청난 소리가 났다.", 
            "무슨 일이 벌어졌는지 보자."
        };
        [SerializeField] UnityEvent OnDropped;
        
        [Header("UI References")]
        [SerializeField] GameObject q_Key;

        override protected void Awake() {
            base.Awake();
            
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        override protected void Start() {
            base.Start();

            isScannable = true;
            startPosition = brickTransform.localPosition;
            startRotation = brickTransform.localRotation;
        }

        public override void TriggerEvent() {
            if (!isPossessed) { q_Key.SetActive(false); return; }
        
            q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) OnInteractedSwitch();
        }

        void OnInteractedSwitch() {
            Sequence switchFlipSequence = DOTween.Sequence();
            
            // Switch to switchOn Sprite
            switchFlipSequence.AppendInterval(.2f);
            switchFlipSequence.JoinCallback(() => {
                hasActivated = false;
                isScannable = false;
            });
            switchFlipSequence.AppendCallback(() => { switchBar.sprite = spriteOn; });
            
            // Generate Impulse after 0.5 secs passes
            switchFlipSequence.AppendInterval(.5f);
            switchFlipSequence.AppendCallback(() => {
                impulseSource.GenerateImpulse();
            });
            
            // Disable Possession
            switchFlipSequence.AppendInterval(.5f);
            switchFlipSequence.AppendCallback(Unpossess);

            // Check if the furnace fulfilled right condition (Oiled & Candle is turned on)
            if (furnace.IsCandleTurnedOn && furnace.IsOiled) {
                switchFlipSequence.Append(brickTransform.DOLocalMove(endPosition, dropDuration).SetEase(Ease.Linear));
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
                    
                    switchBar.sprite = spriteOff;
                });
            }
        }
    }
}