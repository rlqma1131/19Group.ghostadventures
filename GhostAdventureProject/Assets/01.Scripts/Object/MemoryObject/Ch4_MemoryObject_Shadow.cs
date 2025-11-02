using DG.Tweening;
using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_Shadow : MemoryFragment
    {
        [Header("References")] 
        [SerializeField] SpriteRenderer body;
        
        [Header("Fade Settings")] 
        [SerializeField] float fadeDuration = 1.5f;
        
        override protected void Start() {
            base.Start();
            isScannable = true;
        }

        override protected void OnTriggerEnter2D(Collider2D other) {
            base.OnTriggerEnter2D(other);
            body.DOFade(0.5f, fadeDuration);
        }

        override protected void OnTriggerExit2D(Collider2D other) {
            base.OnTriggerExit2D(other);
            body.DOFade(0.0f, fadeDuration);
        }
    }
}