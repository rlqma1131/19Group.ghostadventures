using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Extensions;
using _01.Scripts.Map;
using _01.Scripts.Object.MemoryObject;
using _01.Scripts.Object.NormalObject;
using _01.Scripts.Object.PossessableObject.PO_Object;
using _01.Scripts.Utilities;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01.Scripts.Managers.Puzzle
{
    public enum TransitionResult { TurnedOn, TurnedOff, Failed }
    
    public class Ch4_FurnacePuzzleManager : TemporalSingleton<Ch4_FurnacePuzzleManager>
    {
        #region Fields

        readonly static int Opacity = Shader.PropertyToID("_Opacity");

        [Header("References")] 
        [SerializeField] Ch4_Furnace furnace;
        [SerializeField] Ch4_SpiritTeleportTrigger trigger;
        [SerializeField] List<Volume> volumes;
        [SerializeField] List<Ch4_BackgroundSwitch> switches;
        
        [Header("Door to Unlock")]
        [SerializeField] LockedDoor door;
        [SerializeField] Renderer doorRenderer;
        [SerializeField] Light2D doorLight;
        [SerializeField] int currentProgress;
        [SerializeField] float finalIntensity = 5f;
        [Range(1f, 10f)][SerializeField] float glowTime = 5f;

        [Header("Transition Settings")]
        [SerializeField] float transitionTime = 4f;
        [Range(0f, 1f)] [SerializeField] float finalWeight = 1f;

        [Header("Focus Changed Event")]
        [SerializeField] UnityEvent OnCameraFocusChanged;
        
        // Fields
        UnityAction synchronizeLight = delegate { };
        Action transition = delegate { };
        Coroutine glowCoroutine;
        MaterialPropertyBlock blockOfDoor;
        float transitionVelocity;
        
        // Properties
        bool isDisplaying => volumes[0].weight > 0f;
        public Ch4_Furnace Furnace => furnace;
        public VolumeProfile CurrentProfile => volumes[0].profile;

        #endregion

        override protected void Awake() {
            base.Awake();

            if (!furnace) furnace = FindFirstObjectByType<Ch4_Furnace>();
            if (volumes.Count <= 0) volumes.AddRange(GetComponentsInChildren<Volume>());
            if (switches.Count <= 0) switches.AddRange(GetComponentsInChildren<Ch4_BackgroundSwitch>());
            if (!door) { Debug.LogError("Fatal Error: Door is not allocated!"); return; }
            if (!doorRenderer) door.gameObject.GetComponentInChildren_SearchByName<Renderer>("Locked");
            if (!doorLight) door.gameObject.GetComponentInChildren_SearchByName<Light2D>("Silhouette");
        }

        void Start() {
            blockOfDoor = new MaterialPropertyBlock();
            foreach (Ch4_BackgroundSwitch sw in switches) synchronizeLight += sw.SynchronizeState;
        }

        void OnDestroy() {
            foreach (Ch4_BackgroundSwitch sw in switches) synchronizeLight -= sw.SynchronizeState;
        }

        public TransitionResult TriggerBackgroundTransition(VolumeProfile profile) {
            if (!furnace.IsTurnedOn) return TransitionResult.Failed;
            
            if (isDisplaying && volumes[0].profile == profile) {
                ResetBackground();
                return TransitionResult.TurnedOff;
            }
            
            ChangeBackground(profile);
            return TransitionResult.TurnedOn;
        }

        public void ResetBackGroundTransition() => ResetBackground();

        /// <summary>
        /// Update Progress of furnace puzzle
        /// </summary>
        public void UpdateProgress() {
            // Update Progress
            currentProgress++;
            
            // Update Material Value
            doorRenderer.GetPropertyBlock(blockOfDoor);
            blockOfDoor.SetFloat(Opacity, 1f - (float)currentProgress / switches.Count);
            doorRenderer.SetPropertyBlock(blockOfDoor);
            
            // If condition fulfilled, unlock allocated door.
            if (currentProgress >= switches.Count) {
                trigger.SetTriggerable(true);
                door.UnlockDoors();
            }
        }

        public void FocusAndReleaseTarget(CinemachineVirtualCamera focusTargetAfterTeleport) {
            if (!focusTargetAfterTeleport) {
                ResetControlOfPlayer();
                return;
            }
            
            Sequence focusSequence = DOTween.Sequence();
            focusSequence.JoinCallback(() => {
                    UIManager.Instance.FadeOutIn(2f,
                        () => { GameManager.Instance.Player.PossessionSystem.CanMove = false; },
                        () => { focusTargetAfterTeleport.Priority = 20; },
                        GlowSilhouetteOfDoor);
                })
                .AppendInterval(6f)
                .OnComplete(() => { 
                    UIManager.Instance.FadeOutIn(2f, 
                        () => {},
                        () => { focusTargetAfterTeleport.Priority = 0; },
                        ResetControlOfPlayer); 
                });
        }
        
        public void GlowSilhouetteOfDoor() {
            if (glowCoroutine != null) StopCoroutine(glowCoroutine);
            glowCoroutine = StartCoroutine(GlowCoroutine(glowTime));
        }
        
        void ResetControlOfPlayer() {
            if (GameManager.Instance.PlayerController.GetSlowdownAvailable()) {
                GameManager.Instance.PlayerController.SetSlowdownAvailable(false);
            }
            GameManager.Instance.Player.PossessionSystem.CanMove = true;
        }

        IEnumerator GlowCoroutine(float duration) {
            const float holdTime = 1f;
            float halfDuration = (duration - 1f) * 0.5f;
            float startIntensity = doorLight.intensity;
            
            float time = 0f;
            while (time < halfDuration) {
                doorLight.intensity = Mathf.Lerp(startIntensity, finalIntensity, Mathf.SmoothStep(0f, 1f, time / halfDuration));
                time += Time.deltaTime;
                yield return null;
            }
            doorLight.intensity = finalIntensity;

            yield return new WaitForSeconds(holdTime);

            time = 0f;
            while (time < halfDuration) {
                doorLight.intensity = Mathf.Lerp(finalIntensity, 0f, Mathf.SmoothStep(0f, 1f, time / halfDuration));
                time += Time.deltaTime;
                yield return null;
            }
            doorLight.intensity = 0f;
            
            glowCoroutine = null;
        }
        
        void ChangeBackground(VolumeProfile profile) => ChangeVolumeProfile(profile, 1f);

        void ResetBackground() => ChangeVolumeProfile(null, 0f);

        void ChangeVolumeProfile(VolumeProfile profile, float weight) {
            if (volumes[0].profile == profile && Mathf.Approximately(volumes[0].weight, weight)) {
                Debug.Log("Background Change Request Ignored!");
                return;
            }
            
            transition = () => SetVolumeProfile(profile);
            ProfileTransition(weight);
        }

        /// <summary>
        /// Set Volume Profile of Background
        /// </summary>
        /// <param name="profile"></param>
        void SetVolumeProfile(VolumeProfile profile) {
            if (volumes[0].profile == profile) return;
            foreach (Volume volume in volumes) volume.profile = profile;
        }

        /// <summary>
        /// Transition Coroutine of Background PostProcessing
        /// </summary>
        /// <param name="targetWeight"></param>
        /// <returns></returns>
        void ProfileTransition(float targetWeight) {
            if (volumes.Count <= 0) return;
            transition?.Invoke();
            for (int i = 0; i < volumes.Count; i++) volumes[i].weight = targetWeight;
            synchronizeLight?.Invoke();
        }
    }
}