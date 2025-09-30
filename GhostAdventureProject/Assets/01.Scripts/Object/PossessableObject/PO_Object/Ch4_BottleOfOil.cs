using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    public class Ch4_BottleOfOil : BasePossessable
    {
        const string OnBottleBreak = "Ch4_OilBottle_Break";

        [Header("Brick Transform")] 
        [SerializeField] Transform brickBody;
        [SerializeField] List<Vector3> path = new();
        [SerializeField] Quaternion endRotation;
        [SerializeField, ReadOnly] Quaternion startRotation;

        [Header("Drop Animation Settings")]
        [SerializeField] AudioClip dropSound;
        [SerializeField] Ease easeMode = Ease.InQuad;
        [SerializeField] float dropDuration = 1.5f;
        [SerializeField] List<string> linesToPlayWhenDropped = new() {
            "병을 꺠트렸더니 어떤 액체가 나왔다.",
        };
        [SerializeField] UnityEvent OnDropped;
        
        [Header("UI References")]
        [SerializeField] GameObject q_Key;
        
        override protected void Start() {
            base.Start();
            startRotation = brickBody.rotation;
            
            q_Key.SetActive(false);
        }
        
        public override void TriggerEvent() {
            if (!isPossessed) { q_Key.SetActive(false); return; }
        
            q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) OnBrickDropped();
        }

        void OnBrickDropped() {
            // Turn off UI Element
            q_Key.SetActive(false);
            
            Sequence dropSequence = DOTween.Sequence();
            dropSequence.Append(brickBody.DOShakePosition(0.4f, strength: 0.1f, randomness: 45f, randomnessMode: ShakeRandomnessMode.Harmonic));
            dropSequence.AppendCallback(() => {
                hasActivated = false;
                Unpossess();
            });

            dropSequence.Append(transform.DOLocalPath(path.ToArray(), dropDuration, PathType.CatmullRom, PathMode.TopDown2D).SetEase(Ease.InQuad));
            dropSequence.Join(brickBody.DOLocalRotateQuaternion(endRotation, dropDuration).SetEase(Ease.Linear));
            dropSequence.AppendCallback(() => {
                UIManager.Instance.PromptUI.ShowPrompt_2(linesToPlayWhenDropped.ToArray());
                // Add Event when brick dropped
                isScannable = false;
                brickBody.localRotation = startRotation;
                anim.CrossFade(OnBottleBreak, 0.01f);
                SoundManager.Instance.PlaySFX(dropSound);
                
                OnDropped?.Invoke();
                MarkActivatedChanged();
            });
        }
    }
}