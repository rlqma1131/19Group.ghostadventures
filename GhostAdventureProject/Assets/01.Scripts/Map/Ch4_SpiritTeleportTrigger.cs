using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _01.Scripts.Map
{
    public class Ch4_SpiritTeleportTrigger : MonoBehaviour {
        [Header("Camera To Focus")] 
        [SerializeField] CinemachineVirtualCamera focusableCamera;
        
        [Header("Camera Event")]
        [SerializeField] UnityEvent OnCameraFocusChanged;
        
        bool isTriggerable;
        bool isTriggered;
        
        public void SetTriggerable(bool triggerable) => isTriggerable = triggerable;
        public void SetTriggered(bool triggered) => isTriggered = triggered;
        
        void FocusAndReleaseTarget(CinemachineVirtualCamera focusTargetAfterTeleport) {
            if (!focusTargetAfterTeleport) {
                GameManager.Instance.Player.PossessionSystem.CanMove = true;
                return;
            }

            Sequence focusSequence = DOTween.Sequence();
            focusSequence
                .JoinCallback(() => { 
                    UIManager.Instance.FadeOutIn(
                        fadeDuration: 2f,
                        onStart: () => { GameManager.Instance.Player.PossessionSystem.CanMove = false; }, 
                        onProcess: () => { focusTargetAfterTeleport.Priority = 20; }); 
                })
                .AppendInterval(2f)
                .AppendCallback(() => OnCameraFocusChanged?.Invoke())
                .AppendInterval(4f)
                .OnComplete(() => {
                    UIManager.Instance.FadeOutIn(
                        2f,
                        () => { },
                        () => { focusTargetAfterTeleport.Priority = 0; },
                        () => { GameManager.Instance.Player.PossessionSystem.CanMove = true; });
                });
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (!isTriggerable || isTriggered) return;
            if (!other.CompareTag("Player")) return;
            
            isTriggered = true;
            FocusAndReleaseTarget(focusableCamera);
        }
    }
}
