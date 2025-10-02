using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Object.NormalObject;
using _01.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _01.Scripts.Object.PossessableObject.PO_Object
{
    public enum TransitionResult { TurnedOn, TurnedOff, Failed }
    
    public class Ch4_BackgroundManager : TemporalSingleton<Ch4_BackgroundManager>
    {
        [Header("References")] 
        [SerializeField] List<Volume> volumes;
        [SerializeField] Ch4_Furnace furnace;

        [Header("Transition Settings")]
        [SerializeField] float transitionTime = 2f;
        [Range(0f, 1f)] [SerializeField] float finalWeight = 1f;

        Action transition = delegate { };
        Coroutine transitionCoroutine;
        float transitionVelocity;
        bool isDisplaying => volumes[0].weight > 0f;
        
        public Ch4_Furnace Furnace => furnace;

        override protected void Awake() {
            base.Awake();

            if (volumes.Count <= 0) volumes.AddRange(GetComponentsInChildren<Volume>());
            if (!furnace) furnace = FindFirstObjectByType<Ch4_Furnace>();
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
        
        void ChangeBackground(VolumeProfile profile) => ChangeVolumeProfile(profile, 1f);

        void ResetBackground() => ChangeVolumeProfile(null, 0f);

        void ChangeVolumeProfile(VolumeProfile profile, float weight) {
            if (volumes[0].profile == profile && Mathf.Approximately(volumes[0].weight, weight)) return;
            
            if (profile != null) transition = () => SetVolumeProfile(profile);
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