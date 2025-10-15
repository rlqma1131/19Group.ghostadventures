using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Extensions;
using _01.Scripts.Object.NormalObject;
using _01.Scripts.Object.PossessableObject.PO_Object;
using _01.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace _01.Scripts.Managers.Puzzle
{
    public enum TransitionResult { TurnedOn, TurnedOff, Failed }
    
    public class Ch4_FurnacePuzzleManager : TemporalSingleton<Ch4_FurnacePuzzleManager>
    {
        #region Fields

        readonly static int Opacity = Shader.PropertyToID("_Opacity");

        [Header("References")] 
        [SerializeField] Ch4_Furnace furnace;
        [SerializeField] List<Volume> volumes;
        [SerializeField] List<Ch4_BackgroundSwitch> switches;
        
        [Header("Door to Unlock")]
        [SerializeField] LockedDoor door;
        [SerializeField] Renderer doorRenderer;
        [SerializeField] int currentProgress;

        [Header("Transition Settings")]
        [SerializeField] float transitionTime = 4f;
        [Range(0f, 1f)] [SerializeField] float finalWeight = 1f;
        
        // Fields
        UnityAction synchronizeLight = delegate { };
        Action transition = delegate { };
        Coroutine transitionCoroutine;
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
            if (currentProgress >= switches.Count) door.UnlockDoors();
        }
        
        void ChangeBackground(VolumeProfile profile) => ChangeVolumeProfile(profile, 1f);

        void ResetBackground() => ChangeVolumeProfile(null, 0f);

        void ChangeVolumeProfile(VolumeProfile profile, float weight) {
            if (volumes[0].profile == profile && Mathf.Approximately(volumes[0].weight, weight)) return;
            
            if (profile) transition = () => SetVolumeProfile(profile);
            InitCoroutine(weight);
        }
        
        /// <summary>
        /// Start Transition Coroutine of Background
        /// </summary>
        /// <param name="weight"></param>
        void InitCoroutine(float weight) {
            if (transitionCoroutine != null) {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            transitionCoroutine = StartCoroutine(TransitionCoroutine(Mathf.Clamp01(weight) * finalWeight));
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
        IEnumerator TransitionCoroutine(float targetWeight) {
            if (volumes.Count <= 0) yield break;
            
            float halfTime = transitionTime / 2f;
            float currentWeight = volumes[0].weight;

            while (!Mathf.Approximately(currentWeight, 0)) {
                currentWeight = Mathf.SmoothDamp(currentWeight, 0, ref transitionVelocity, halfTime * Time.deltaTime);
                for (int i = 0; i < volumes.Count; i++) volumes[i].weight = currentWeight;
                yield return null;
            }
            for (int i = 0; i < volumes.Count; i++) volumes[i].weight = 0f;

            transition?.Invoke();
            synchronizeLight?.Invoke();
            
            while (!Mathf.Approximately(currentWeight, targetWeight)) {
                currentWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref transitionVelocity, halfTime * Time.deltaTime);
                for (int i = 0; i < volumes.Count; i++) volumes[i].weight = currentWeight;
                yield return null;
            }
            for (int i = 0; i < volumes.Count; i++) volumes[i].weight = targetWeight;
            
            transitionCoroutine = null;
        }
    }
}