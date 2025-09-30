using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_Furnace : BaseInteractable
    {
        [Header("References of Furnace")]
        [SerializeField] Animator anim;
        [SerializeField] Light2D fire;
        [SerializeField] bool isTurnedOn;

        [Header("Lines of Interactions")] 
        [SerializeField] List<string> linesWhenNotTurnedOn = new() {
            "벽난로 불이 꺼져있다.", 
            "이 불을 켜야할 것 같은 기분이 든다."
        };
        
        bool isPlayerInside;

        override protected void Awake() {
            base.Awake();
            anim = GetComponentInChildren<Animator>(true);
            fire = GetComponentInChildren<Light2D>(true);
        }

        override protected void Start() {
            base.Start();

            isScannable = true;
            isTurnedOn = false;
        }

        void Update() => TriggerEvent();

        public override void TriggerEvent() {
            if (!isScannable) return;
            
            if (Input.GetKeyDown(KeyCode.E) && isPlayerInside) {
                if (!isTurnedOn) {
                    UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenNotTurnedOn.ToArray());
                }
            }
        }

        override protected void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                player.InteractSystem.AddInteractable(gameObject);
                isPlayerInside = true;
            }
        }

        override protected void OnTriggerExit2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                player.InteractSystem.RemoveInteractable(gameObject);
                isPlayerInside = false;
            }
        }
    }
}