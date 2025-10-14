using System;
using _01.Scripts.Extensions;
using _01.Scripts.Object.BaseClasses.Interfaces;
using UnityEngine;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_TurnOnHighlight : MonoBehaviour, IInteractable
    {
        [Header("Highlight")] 
        [SerializeField] GameObject highlight;

        Player.Player player;
        bool isScannable;

        void Awake() {
            Transform component = 
                gameObject.GetComponentInChildren_SearchByName<Transform>("Highlight", true);
            highlight = component != null ? component.gameObject : null;
        }

        void Start() {
            player = GameManager.Instance.Player;
            isScannable = true;
            
            highlight?.SetActive(false);
        }
        
        public void SetScannable(bool value) => isScannable = value;

        public bool IsScannable() => isScannable;

        public void ShowHighlight(bool pop) => highlight?.SetActive(pop);

        public void TriggerEvent() { }
        
        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                player.InteractSystem.AddInteractable(gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                ShowHighlight(false);
                player.InteractSystem.RemoveInteractable(gameObject);
            }
        }

    }
}