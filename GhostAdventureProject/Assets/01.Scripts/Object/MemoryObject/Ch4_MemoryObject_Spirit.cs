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

        [Header("Animation Settings")] 
        [SerializeField] Transform target;
        [SerializeField] float fadeDuration = 0.5f;
        [SerializeField] float moveDuration = 1f;

        MaterialPropertyBlock block;
        
        override protected void Start() {
            base.Start();
            isScannable = false;
            block = new MaterialPropertyBlock();
        }

        public override void AfterScan() => glass.UpdateProgress();

        public void PlayAnimation() {
            Sequence fadeSequence = DOTween.Sequence();
            fadeSequence
                .Append(body.DOFade(1f, fadeDuration))
                .Append(gameObject.transform.DOMove(target.position, moveDuration))
                .AppendCallback(() => SetScannable(true));
        }

        public override void SetScannable(bool value) {
            base.SetScannable(value);

            if (!isScannable) return;
            block.Clear();
            body.GetPropertyBlock(block);
            block.SetColor(BaseColor, Color.white);
            body.SetPropertyBlock(block);
        }

        public override void SetAlreadyScanned(bool val) {
            base.SetAlreadyScanned(val);

            if (!alreadyScanned) return;
            
            gameObject.transform.position = target.position;
            block.Clear();
            body.GetPropertyBlock(block);
            block.SetColor(BaseColor, Color.white);
            body.SetPropertyBlock(block);
        }
    }
}