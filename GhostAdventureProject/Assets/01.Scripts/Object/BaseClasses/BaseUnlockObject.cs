using UnityEngine;

namespace _01.Scripts.Object.BaseClasses
{
    public abstract class BaseUnlockObject : MonoBehaviour
    {
        protected Animator anim;
        protected Player.Player player;

        void Start() {
            anim = GetComponentInChildren<Animator>();
            player = GameManager.Instance.Player;
        }

        public abstract void Unlock();

        // 상호작용키 UI 표시
        void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;

            player.InteractSystem.AddInteractable(gameObject);
        }

        void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) return;

            player.InteractSystem.RemoveInteractable(gameObject);
        }
    }
}