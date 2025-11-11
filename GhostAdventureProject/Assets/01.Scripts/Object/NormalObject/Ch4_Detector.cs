using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] LayerMask circleLayerMask;
        [SerializeField] LayerMask raycastLayerMask;
        [SerializeField] float scanInterval = 0.1f;

        [Header("Angle Settings")] 
        [SerializeField] float angleSpeed = 1f;
        [SerializeField] float scanAngle = 45f;
        [SerializeField] float movingAngle = 30f;
        [Range(0f, 360f)] [SerializeField] float angleOffset = 180f;

        [Header("Teleport Settings")] 
        [SerializeField] Transform targetToTeleport;
        [SerializeField] float resetDelay = 1f;
        [SerializeField] float resetDuration = 2f;
        [SerializeField] float fadeDuration = 4f;

        [Header("Dialogues")] 
        [SerializeField] List<string> linesWhenDetected = new() { "앗... 걸렸다!" };

        Collider2D[] overlaps = new Collider2D[5];
        RaycastHit2D[] hits = new RaycastHit2D[5];
        CountdownTimer scanTimer;
        bool isInTransition;
        bool isDetected;

        #endregion
        
#if UNITY_EDITOR
        void OnValidate() {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, angleOffset);   
        }
#endif
        
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
            float angle = Vector2.SignedAngle(Quaternion.Euler(0f, 0f, 180f) * Vector2.up, transform.up);
            for (int i = 0; i < 6; i++) {
                if (angle >= -90f + 30f * i && angle < -90f + 30f * (i + 1)) {
                    bodyRenderer.sprite = listOfSprites[i];
                }
                else {
                    bodyRenderer.sprite = angle switch {
                        < -90f => listOfSprites.First(),
                        > 90f => listOfSprites.Last(),
                        _ => bodyRenderer.sprite
                    };
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
                        () => { GameManager.Instance.Player.transform.position = targetToTeleport.transform.position; },
                        () => {
                            isInTransition = false;
                            GameManager.Instance.Player.PossessionSystem.CanMove = true;
                        });
                });
        }
        
        /// <summary>
        /// Check if the player is in detection area
        /// </summary>
        /// <returns></returns>
        bool IsPlayerDetected() {
            ContactFilter2D circleFilter = new ContactFilter2D {
                useTriggers = true,
                useLayerMask = true,
                layerMask = circleLayerMask,
            };
            
            int size = Physics2D.OverlapCircle(transform.position, scanRadius, circleFilter, overlaps);
            if (size <= 0) return false; 

            Vector2 direction = overlaps[0].transform.position - transform.position;
            float angle = Vector2.Angle(transform.up, direction);
            if (angle > scanAngle / 2f) return false;
            
            ContactFilter2D rayFilter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = raycastLayerMask, };
            int raySize = Physics2D.Raycast(transform.position, direction.normalized, rayFilter, hits, scanRadius);
            if (raySize <= 0) return false;
            
            float closest = float.MaxValue;
            int index = 0;
            for (int i = 0; i < raySize; i++) {
                float distance = Vector2.Distance(transform.position, hits[i].point);
                if (distance >= closest) continue;
                closest = distance; index = i;
            }
            return hits[index] && hits[index].collider.gameObject.CompareTag("Player");
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, scanRadius);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, 0f, -scanAngle / 2f) * transform.up * scanRadius);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, 0f, scanAngle / 2f) * transform.up * scanRadius);
        }
    }
}