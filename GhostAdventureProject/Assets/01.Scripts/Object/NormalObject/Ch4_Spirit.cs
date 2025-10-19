using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Extensions;
using _01.Scripts.Managers.Puzzle;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_Spirit : BasePossessable
    {
        #region Fields
    
        readonly static int IsDead = Animator.StringToHash("IsDead");
        
        [Header("References")] 
        [SerializeField] SpriteRenderer bodyRenderer;
        [SerializeField] SpriteRenderer highlightRenderer;
        [SerializeField] GameObject q_Key;
        [SerializeField] Light2D light2D;

        [Header("Exorcism Settings")] 
        [SerializeField] Ch4_Picture target;
        [SerializeField] float exorcismDuration = 3f;
        [Range(0f, 1f)] [SerializeField] float shakeStrength = 0.5f;
        [SerializeField] AnimationCurve lightCurve;
        [SerializeField] float maxLightIntensity = 12f;
        [SerializeField] UnityEvent onExorcism;
        
        [Header("Teleport Settings")]
        [SerializeField] bool isTeleportAvailable;
        [SerializeField] float teleportDuration = 2f;
        [SerializeField] List<Transform> teleportPoints;
        
        [Header("Spirit Event Settings")] 
        [SerializeField] float fadeDuration = 1f;
        [SerializeField] List<string> linesWhenTeleported = new() { "도망갔어..." };
        [SerializeField] List<string> linesWhenSuccess = new() { "됐다!" };
        [SerializeField] List<string> linesWhenFailed = new() { "놓쳤어.. 어디 간거지?" };

        [Header("Scan Settings")] 
        [SerializeField] LayerMask layerMask;
        [SerializeField] float scanRadius = 5f;
        [SerializeField] float scanInterval = 0.1f;

        Ch4_FurnacePuzzleManager manager;
        Collider2D[] results = new Collider2D[1];
        float currentTime;
        bool isTriggered;
        bool isInTransition;
        bool isInQTE;

        #endregion
        
        override protected void Awake() {
            base.Awake();

            if (!bodyRenderer)
                bodyRenderer = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Body");
            if (!highlightRenderer)
                highlightRenderer = gameObject.GetComponentInChildren_SearchByName<SpriteRenderer>("Highlight");
            if (!light2D) light2D = GetComponent<Light2D>();
        }

        override protected void Start() {
            base.Start();

            manager = Ch4_FurnacePuzzleManager.TryGetInstance();
            currentTime = scanInterval;
        }

        override protected void Update() => TriggerEvent();

        public override void SetActivated(bool value) {
            if (!value) manager.UpdateProgress();
            base.SetActivated(value);
        }

        public override void TriggerEvent() {
            if (!hasActivated) return;

            if (isPossessed) return;
            CheckToTeleport();
        }

        #region PossessionEvents

        public override bool TryPossess() {
            isInQTE = true;
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("은신 시도 중...", 2f);
            player.PossessionSystem.PossessedTarget = this;
            RequestQTEEvent();
            return true;
        }

        public override void OnPossessionEnterComplete() {
            base.OnPossessionEnterComplete();

            ProceedExorcism();
        }

        #endregion

        #region QTE Event Handlers

        public override void OnQTESuccess() {
            base.OnQTESuccess();
            isInQTE = false;
        }

        public override void OnQTEFailure() {
            base.OnQTEFailure();
            isInQTE = false;
            TeleportToRandomPoint(linesWhenFailed.ToArray());
        }

        #endregion

        #region Teleportation & Exorcism Methods

        void ProceedExorcism() {
            Sequence exorcismSequence = DOTween.Sequence();
            exorcismSequence
                .Append(transform.DOShakePosition(exorcismDuration, shakeStrength, fadeOut: true))
                .JoinCallback(() => {
                    onExorcism?.Invoke();
                    UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenSuccess.ToArray());
                    hasActivated = false;
                    MarkActivatedChanged();
                    anim.SetTrigger(IsDead);
                    StartCoroutine(LightCurveCoroutine());
                })
                .AppendCallback(() => {
                    Unpossess();
                    TeleportToPicture();
                });
        }

        void TeleportToPicture() {
            if (!target) return;

            UIManager.Instance.FadeOutIn(teleportDuration,
                () => { GameManager.Instance.Player.PossessionSystem.CanMove = false; },
                () =>
                {
                    GameManager.Instance.Player.transform.position = target.transform.position;
                    target.SetPictureState(false, true);
                },
                () => { GameManager.Instance.Player.PossessionSystem.CanMove = true; });
        }

        void TeleportToRandomPoint(string[] linesToPlay) {
            Sequence teleportSequence = DOTween.Sequence();

            teleportSequence.Append(bodyRenderer.DOFade(0f, fadeDuration));
            teleportSequence.JoinCallback(() =>
            {
                UIManager.Instance.PromptUI.ShowPrompt_2(linesToPlay);
                isScannable = false;
                isTriggered = true;
            });
            teleportSequence.AppendCallback(() =>
            {
                Transform[] list = teleportPoints.Where(item => item.transform.position != transform.position).ToArray();
                Transform selected = list[Random.Range(0, list.Length)];
                
                bodyRenderer.flipX = !selected.gameObject.name.EndsWith("Reversed");
                transform.position = selected.position;
            });

            teleportSequence.Append(bodyRenderer.DOFade(1f, fadeDuration));
            teleportSequence.AppendCallback(() =>
            {
                isScannable = true;
                isTriggered = false;
            });
        }
        
        #endregion

        #region Helper Methods

        IEnumerator LightCurveCoroutine() {
            float time = 0f;
            while (time < exorcismDuration) {
                light2D.intensity = maxLightIntensity * lightCurve.Evaluate(time / exorcismDuration);
                bodyRenderer.color = new Color(bodyRenderer.color.r, bodyRenderer.color.g, bodyRenderer.color.b,
                    1f - time / exorcismDuration);
                time += Time.deltaTime;
                yield return null;
            }
        }

        void CheckToTeleport() {
            if (!isTeleportAvailable) return;

            if (currentTime <= 0f) {
                currentTime = scanInterval;
                if (!IsPlayerSlowdown() && !isTriggered && !isInQTE)
                    TeleportToRandomPoint(linesWhenTeleported.ToArray());
            }
            else currentTime -= Time.deltaTime;
        }

        bool IsPlayerSlowdown() {
            ContactFilter2D filter = new ContactFilter2D { 
                useTriggers = true,
                useDepth = false, 
                useLayerMask = true, layerMask = layerMask
            };
            int size = Physics2D.OverlapCircle(transform.position, scanRadius, filter, results);
            for (int i = 0; i < size; i++) {
                if (!results[i].TryGetComponent(out PlayerController pc)) continue;
                if (pc.CurrentSpeed > pc.MinSpeed) return false;
            }

            return true;
        }
        
        #endregion

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, scanRadius);
        }
    }
}