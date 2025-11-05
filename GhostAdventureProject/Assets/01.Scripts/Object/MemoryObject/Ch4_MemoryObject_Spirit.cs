using _01.Scripts.Map;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_Spirit : MemoryFragment {
        readonly static int BaseColor = Shader.PropertyToID("_Color");

        [Header("References")] 
        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Ch4_MemoryObject_ShatteredGlass glass;
        [SerializeField] Ch4_SpiritTeleportTrigger trigger;
        
        [Header("Animation Settings")] 
        [SerializeField] Transform target;
        [SerializeField] float fadeDuration = 2f;
        [SerializeField] float moveDuration = 2f;

        [Header("Teleport Settings")] 
        [SerializeField] Transform teleportTarget;
        
        MaterialPropertyBlock block;
        Vector3 originalPosition;
        
        override protected void Start() {
            base.Start();
            isScannable = false;
            originalPosition = gameObject.transform.position;
            block = new MaterialPropertyBlock();
        }

        public override void AfterScan() => glass.UpdateProgress();

        public void PlayAnimation() {
            Sequence fadeSequence = DOTween.Sequence();
            fadeSequence
                .SetEase(Ease.OutQuad)
                .Append(body.DOFade(1f, fadeDuration))
                .Append(gameObject.transform.DOMove(target.position, moveDuration))
                .Append(gameObject.transform.DOMove(originalPosition, moveDuration))
                .Append(body.DOFade(0f, fadeDuration))
                .AppendCallback(() => {
                    transform.position = teleportTarget.position;
                    SetScannable(true);
                });
        }

        public override void SetScannable(bool value) {
            base.SetScannable(value);

            if (!isScannable) return;
            
            transform.position = teleportTarget.position;
            trigger.SetTriggered(true);
            block.Clear();
            body.GetPropertyBlock(block);
            block.SetColor(BaseColor, Color.white);
            body.SetPropertyBlock(block);
        }

        public override void SetAlreadyScanned(bool val) {
            base.SetAlreadyScanned(val);

            if (!alreadyScanned) return;
            
            gameObject.transform.position = teleportTarget.position;
            block.Clear();
            body.GetPropertyBlock(block);
            block.SetColor(BaseColor, Color.white);
            body.SetPropertyBlock(block);
        }
    }
}