using System;
using System.Collections.Generic;
using _01.Scripts.Managers.Puzzle;
using _01.Scripts.Object.MemoryObject;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    public class Ch4_BackgroundSwitch : BasePossessable
    {
        [Header("References")] 
        [SerializeField] VolumeProfile profile;
        [SerializeField] Light2D switchLight;
        [SerializeField] Ch4_Picture picture;
        
        [Header("Button Settings")]
        [SerializeField] List<string> linesWhenButtonTurnedOn = new() { "불이 켜졌다.", "뭔가 달라진 게 있을려나?" };
        [SerializeField] List<string> linesWhenButtonTurnedOff = new() { "불이 꺼졌다.", };
        [SerializeField] List<string> linesWhenButtonFailed = new() { "아무 일도 일어나지 않았다...", "일단 거실 쪽으로 가볼까?", };
        
        [Header("UI References")]
        [SerializeField] GameObject q_Key;

        // Fields
        Ch4_FurnacePuzzleManager manager;
        bool alreadyPressed;

        override protected void Awake() {
            base.Awake();

            if (!switchLight) switchLight = GetComponentInChildren<Light2D>(true);
            if (!picture) Debug.LogError($"GameObject Picture is Missing in {gameObject.name}");
        }

        override protected void Start() {
            base.Start();

            manager = Ch4_FurnacePuzzleManager.TryGetInstance();
        }

        void OnDestroy() => manager = null;

        override protected void Update() {
            if (Input.GetKeyDown(KeyCode.E) && isPossessed && !alreadyPressed) Unpossess();
            TriggerEvent();
        }

        public override void TriggerEvent() {
            if (!isPossessed) { q_Key.SetActive(false); return; }
        
            if (!alreadyPressed) q_Key.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Q)) OnTriggerButton();
        }

        public void SynchronizeState() {
            bool state = manager.CurrentProfile == profile;
            
            switchLight.gameObject.SetActive(state);
            if (picture) picture.SetPictureState(state);
        }

        void OnTriggerButton() {
            q_Key.SetActive(false);
            
            if (!manager) return;
            
            Sequence triggerSequence = DOTween.Sequence();
            triggerSequence.AppendInterval(.2f);
            triggerSequence.JoinCallback(() => {
                alreadyPressed = true;
                hasActivated = false;
                isScannable = false;
            });
            triggerSequence.AppendCallback(() => {
                switch (manager.TriggerBackgroundTransition(profile)) {
                    case TransitionResult.TurnedOn:
                        UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenButtonTurnedOn.ToArray());
                        break;
                    case TransitionResult.TurnedOff:
                        UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenButtonTurnedOff.ToArray());
                        break;
                    case TransitionResult.Failed:
                        UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenButtonFailed.ToArray());
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            });

            triggerSequence.AppendInterval(1f);
            triggerSequence.AppendCallback(() => {
                hasActivated = true;
                isScannable = true;
                alreadyPressed = false;
                Unpossess();
            });
        }
    }
}