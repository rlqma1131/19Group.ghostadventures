using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static _01.Scripts.Utilities.Timer;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_Detector : MonoBehaviour
    {
        #region Fields

        [Header("References")] 
        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] List<Sprite> listOfSprites;

        [Header("Scan Settings")] 
        [SerializeField] float scanRadius = 3f;
        [SerializeField] LayerMask layerMask;
        [SerializeField] float scanInterval = 0.1f;

        [Header("Angle Settings")] 
        [SerializeField] float angleSpeed = 1f;
        [SerializeField] float scanAngle = 45f;
        [SerializeField] float movingAngle = 30f;
        [Range(0f, 360f)] [SerializeField] float angleOffset = 180f;

        [Header("Teleport Settings")] 
        [SerializeField] Transform targetToReset;
        [SerializeField] Transform targetToTeleport;
        [SerializeField] float resetDelay = 1f;
        [SerializeField] float resetDuration = 2f;
        [SerializeField] float fadeDuration = 4f;

        [Header("Dialogues")] 
        [SerializeField] List<string> linesWhenDetected = new() { "앗... 걸렸다!" };

        Collider2D[] results = new Collider2D[1];
        CountdownTimer scanTimer;
        bool isInTransition;

        #endregion
        
        void Start() {
            scanTimer = new CountdownTimer(scanInterval) {
                OnTimerStart = () => { if (IsPlayerDetected()) TeleportPlayer(); },
                OnTimerStop = () => { if (scanTimer.IsFinished) scanTimer.Start(); }
            };

            transform.rotation = Quaternion.Euler(0f, 0f, angleOffset);
        }

        void Update() {
            if (scanTimer.IsRunning) UpdateScanAngle(Time.time);
            scanTimer.Tick(Time.deltaTime);
        }

        public void StartScanning() => scanTimer.Start();
        public void StopScanning() => scanTimer.Stop();

        /// <summary>
        /// Updates Sensor Angle & Sprite of Body
        /// </summary>
        /// <param name="time"></param>
        void UpdateScanAngle(float time) {
            transform.rotation = Quaternion.Euler(0f, 0f,
                angleOffset + (movingAngle / 2 - Mathf.PingPong(time * angleSpeed, movingAngle)));

            // Update Body Sprite based on sensor angle
            float angle = Vector2.SignedAngle(Quaternion.Euler(0f, 0f, angleOffset) * Vector2.up, transform.up);
            for (int i = 0; i < 5; i++) {
                if (angle >= -movingAngle / 2f + movingAngle / 5f * i &&
                    angle < -movingAngle / 2f + movingAngle / 5f * (i + 1)) {
                    bodyRenderer.sprite = listOfSprites[i];
                    break;
                }
            }
        }

        /// <summary>
        /// Teleports player when player is detected
        /// </summary>
        void TeleportPlayer() {
            if (isInTransition) return;
            isInTransition = true;
            
            Sequence teleportSequence = DOTween.Sequence();
            teleportSequence
                .AppendInterval(resetDelay)
                .JoinCallback(() => {
                    // TODO: Play Detected SFX
                    UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenDetected.ToArray());
                })
                .AppendCallback(() => {
                    UIManager.Instance.FadeOutIn(resetDuration,
                        () => { GameManager.Instance.Player.PossessionSystem.CanMove = false; },
                        () => { GameManager.Instance.Player.transform.position = targetToReset.transform.position; },
                        () => {
                            isInTransition = false;
                            GameManager.Instance.Player.PossessionSystem.CanMove = true;
                        });
                });
        }
        
        bool IsPlayerDetected() {
            ContactFilter2D circleFilter = new ContactFilter2D {
                useTriggers = true,
                useLayerMask = true,
                layerMask = layerMask,
            };
            
            int size = Physics2D.OverlapCircle(transform.position, scanRadius, circleFilter, results);
            if (size <= 0) return false;
            
            Vector2 direction = results[0].transform.position - transform.position;
            float angle = Vector2.Angle(transform.up, direction);
            if (angle > scanAngle / 2f) return false;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, scanRadius, layerMask);
            return hit;
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, scanRadius);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, 0f, -scanAngle / 2f) * transform.up * scanRadius);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, 0f, scanAngle / 2f) * transform.up * scanRadius);
        }
    }
}