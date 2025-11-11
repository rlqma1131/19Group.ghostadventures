using System;
using System.Collections.Generic;
using _01.Scripts.CustomPropertyAttribute;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_Furnace : BaseInteractable
    {
        [Header("References of Furnace")]
        [SerializeField] Animator anim;
        [SerializeField] Light2D fire;
        [SerializeField, ReadOnly] bool isOiled;
        [SerializeField, ReadOnly] bool isCandleTurnedOn;
        [SerializeField, ReadOnly] bool isCloseWithFire;
        [SerializeField] bool isTurnedOn;

        [Header("Lines of Interactions")] 
        [SerializeField] List<string> linesWhenNotTurnedOn = new() {
            "벽난로 불이 꺼져있다.", 
            "이 불을 켜야할 것 같은 기분이 든다."
        };
        [SerializeField] List<string> linesWhenOiledUp = new() {
            "나무가 기름에 적셔있다.",
            "이제 불을 붙힐 것만 찾으면 된다."
        };
        [SerializeField] List<string> linesWhenTurnedOn = new() {
            "불이 켜졌다.",
            "이 다음에 뭘하면 될까?",
            "주변을 한번 살펴보자."
        };
        
        // Fields
        bool isPlayerInside;

        // Properties
        public bool IsOiled => isOiled;
        public bool IsCandleTurnedOn => isCandleTurnedOn;
        public bool IsTurnedOn => isTurnedOn;

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
                switch (isTurnedOn) {
                    case true when isOiled: UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenTurnedOn.ToArray()); break;
                    case false when isOiled: UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenOiledUp.ToArray()); break;
                    case false: UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenNotTurnedOn.ToArray()); break;
                }
            }
        }
        
        public void SetOiled(bool value) {
            isOiled = value;
            if (isOiled && isCloseWithFire) isTurnedOn = true;
            fire.gameObject.SetActive(isTurnedOn);
        }
        
        public void SetCloseWithFire(bool value) {
            isCloseWithFire = value;
            if (isOiled && isCloseWithFire) isTurnedOn = true;
            fire.gameObject.SetActive(isTurnedOn);
        }
        
        public void SetCandleTurnedOn() => isCandleTurnedOn = !isCandleTurnedOn;

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