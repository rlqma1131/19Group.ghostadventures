using _01.Scripts.Map;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace _01.Scripts.Object.MemoryObject
{
    public class Ch4_MemoryObject_Spirit : MemoryFragment 
    {
        #region Fields

        readonly static int BaseColor = Shader.PropertyToID("_Color");

        [Header("References")] 
        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Ch4_MemoryObject_ShatteredGlass glass;
        [SerializeField] Ch4_SpiritTeleportTrigger trigger;

        [Header("Animation Settings")] 
        [SerializeField] Transform target;
        [SerializeField] AudioClip surpriseSound;
        [SerializeField] float fadeDuration = 2f;
        [SerializeField] float moveDuration = 2f;

        [Header("Teleport Settings")] 
        [SerializeField] Transform teleportTarget;

        Vector3 originalPosition;

        #endregion
        
        override protected void Start() {
            base.Start();
            isScannable = false;
            originalPosition = gameObject.transform.position;
        }

        public override void AfterScan() => glass.UpdateProgress();

        public void PlayAnimation() {
            Sequence fadeSequence = DOTween.Sequence();
            fadeSequence
                .Append(body.DOFade(1f, fadeDuration))
                .Append(gameObject.transform.DOMove(target.position, moveDuration))
                .AppendInterval(1.5f)
                .Append(gameObject.transform.DOJump(target.position, 1f,1,0.4f))
                .JoinCallback(() => {
                    if (surpriseSound != null)
                        SoundManager.Instance.PlaySFX(surpriseSound, 1f);
                })
                .AppendInterval(0.5f)
                .AppendCallback(() => body.flipX = true)
                .Append(gameObject.transform.DOMove(originalPosition, moveDuration / 2f))
                .Append(body.DOFade(0f, fadeDuration / 2f))
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
            body.color = Color.white;
        }

        public override void SetAlreadyScanned(bool val) {
            base.SetAlreadyScanned(val);

            if (!alreadyScanned) return;
            
            gameObject.transform.position = teleportTarget.position;
            body.color = Color.white;
        }
    }
}