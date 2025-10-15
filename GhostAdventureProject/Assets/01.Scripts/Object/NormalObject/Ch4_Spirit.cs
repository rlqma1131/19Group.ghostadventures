using System;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Extensions;
using _01.Scripts.Managers.Puzzle;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_Spirit : BasePossessable
    {
        [Header("References")] 
        [SerializeField] List<Transform> teleportPoints;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] GameObject q_Key;

        [Header("Spirit Settings")] 
        [SerializeField] bool isTeleportAvailable;
        [SerializeField] float fadeDuration = 1f;
        [SerializeField] List<string> linesWhenTeleported = new() { "도망갔어..." };
        [SerializeField] List<string> linesWhenSuccess = new() { "됐다!" };
        [SerializeField] List<string> linesWhenFailed = new() { "놓쳤어.. 어디 간거지?" };

        [Header("Scan Settings")] 
        [SerializeField] LayerMask layerMask;
        [SerializeField] float scanRadius = 5f;
        [SerializeField] float scanInterval = 0.1f;

        Ch4_FurnacePuzzleManager manager;
        Collider2D[] colliders = new Collider2D[1];
        float currentTime;
        bool isTriggered;

        override protected void Start() {
            base.Start();

            manager = Ch4_FurnacePuzzleManager.TryGetInstance();
            currentTime = scanInterval;
        }

        override protected void Update() => TriggerEvent();

        public override void TriggerEvent() {
            if (!hasActivated) return;
            
            if (!isPossessed) {
                CheckToTeleport();
                q_Key.SetActive(false); 
                return;
            }
        
            q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) {
                // TODO : Trigger Exorcism Event (Show afterimage, update progress of furnace puzzle and unpossess)
            }
        }

        void CheckToTeleport() {
            if (!isTeleportAvailable) return;

            if (currentTime <= 0f) {
                currentTime = scanInterval;
                if (!IsPlayerSlowdown() && !isTriggered) 
                    TeleportToRandomPoint(linesWhenTeleported.ToArray());
            } else currentTime -= Time.deltaTime;
        }

        bool IsPlayerSlowdown() {
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, scanRadius, colliders, layerMask);
            for (int i = 0; i < size; i++) {
                if (!colliders[i].TryGetComponent(out PlayerController pc)) continue;
                if (!pc.IsSlowdownActive) return false;
            }
            return true;
        }

        void TeleportToRandomPoint(string[] linesToPlay) {
            Sequence teleportSequence = DOTween.Sequence();
            
            teleportSequence.Append(spriteRenderer.DOFade(0f, fadeDuration));
            teleportSequence.JoinCallback(() =>
            {
                UIManager.Instance.PromptUI.ShowPrompt_2(linesToPlay);
                isScannable = false; isTriggered = true;
            });
            teleportSequence.AppendCallback(() => {
                Transform[] list = teleportPoints.Where(item => item.transform.position != transform.position).ToArray();
                transform.position = list[Random.Range(0, list.Length)].position;
            });

            teleportSequence.Append(spriteRenderer.DOFade(1f, fadeDuration));
            teleportSequence.AppendCallback(() => { isScannable = true; isTriggered = false; });
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
    }
}